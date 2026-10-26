using KC.InternalApiClient;
using LogViewer.AppContext;
using LogViewer.Config.Models;
using LogViewer.Controls.Helpers;
using LogViewer.Favorites;
using LogViewer.Logging;
using LogViewer.Providers.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer.Controls
{
    /// <summary>
    /// Fetches several saved gateways into one graph. The WebAPI tab is for *finding* a gateway
    /// by walking organisation -> resort -> object; this is for the ones you already found.
    /// The selection is the query: what's highlighted is what ends up in the graph.
    /// </summary>
    public partial class FavoritesSourceControl : UserControl
    {
        public LogViewerContext DataSource { get; set; } = new LogViewerContext();
        public event EventHandler? OnDataChanged;

        private readonly ControlStateManager controlStateManager;
        private readonly ProgressBarManager progressBarManager;
        private readonly ApiClientProvider apiClientProvider;

        private List<OrganisationConfig> organisations = new();
        private CancellationTokenSource? cancellationTokenSource;

        public FavoritesSourceControl()
        {
            InitializeComponent();

            // Pick several gateways to compare. Switch to MultiSimple in the designer if you'd
            // rather have click-to-toggle without holding Ctrl.
            listBoxFavorites.SelectionMode = SelectionMode.MultiExtended;

            controlStateManager = new ControlStateManager([
                listBoxFavorites,
                buttonFetch,
                dateTimePickerFrom,
                dateTimePickerUntill,
                progressBarFetch,
                labelFrom,
                labelUntill,
            ]);

            progressBarManager = new ProgressBarManager(progressBarFetch);
            apiClientProvider = new ApiClientProvider(new DialogClientCredentialsSource());

            buttonFetch.Click += (s, e) => FetchSelectedFavorites();
            buttonCancel.Click += (s, e) => cancellationTokenSource?.Cancel();

            // The star on the WebAPI tab writes to the same store.
            FavoritesStore.Shared.Changed += (s, e) => RunOnUi(RefreshList);

            dateTimePickerFrom.Value = DateTime.Now.Date;
            dateTimePickerUntill.Value = DateTime.Now.Date + TimeSpan.FromDays(1);

            RefreshList();
        }

        /// <summary>Needed to authenticate: a favorite stores which organisation it belongs to, not how to reach it.</summary>
        public void LoadOrganisations(List<OrganisationConfig> organisationConfigs)
        {
            organisations = organisationConfigs;
        }

        /// <summary>Rebuilds the list, keeping whatever was selected still selected.</summary>
        private void RefreshList()
        {
            var selected = SelectedFavorites();

            listBoxFavorites.BeginUpdate();
            listBoxFavorites.Items.Clear();

            foreach (var favorite in FavoritesStore.Shared.Items)
            {
                int index = listBoxFavorites.Items.Add(favorite);
                if (selected.Any(s => s.IsSameGateway(favorite)))
                    listBoxFavorites.SetSelected(index, true);
            }

            listBoxFavorites.EndUpdate();
        }

        private List<Favorite> SelectedFavorites()
            => listBoxFavorites.SelectedItems.Cast<Favorite>().ToList();

        private async void FetchSelectedFavorites()
        {
            var selected = SelectedFavorites();
            if (selected.Count == 0)
            {
                MessageBox.Show("Select one or more favorites to fetch.", "Favorites",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime from = dateTimePickerFrom.Value;
            DateTime until = dateTimePickerUntill.Value;

            await RunWithDisabledControlsAsync(async token =>
            {
                var collection = new LogCollection();
                var failures = new List<string>();
                var counts = new List<string>();

                // One client per organisation, resolved before fetching: authentication can prompt,
                // and a dialog appearing halfway through with the controls disabled is horrible.
                var clients = await ResolveClientsAsync(selected, failures, token);

                // One bar for the whole run: each gateway reports 0..1 over its own slice of it.
                IProgress<double> bar = progressBarManager.Progress;

                for (int i = 0; i < selected.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var favorite = selected[i];
                    if (!clients.TryGetValue(OrganisationKey(favorite), out var client))
                    {
                        bar.Report((double)(i + 1) / selected.Count);
                        continue;
                    }

                    int index = i;
                    var slice = new Progress<double>(p => bar.Report((index + p) / selected.Count));

                    try
                    {
                        var provider = new ApiGatewayLogProvider(
                            client, favorite.GatewayOid, favorite.GatewayDeviceId, from, until);

                        var log = await provider.GetData(token, slice);

                        // Each gateway's entries carry it as their parent, so they stay in their own
                        // traces even when two gateways have the same thermostat id.
                        collection.Entries.AddRange(log.Entries);
                        counts.Add($"{favorite.Label ?? favorite.GatewayOid.ToString()}: {log.Entries.Count}");
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // One dead gateway shouldn't cost you the others.
                        Debug.WriteLine(ex);
                        failures.Add($"{favorite}: {ex.Message}");
                    }

                    // The provider derives its progress from timestamps and rarely lands exactly on
                    // 1.0, so close the slice off here - failures included, they're done either way.
                    bar.Report((double)(index + 1) / selected.Count);
                }

                DataSource.ScopeViewContext.StartDate = from;
                DataSource.ScopeViewContext.EndDate = until;
                DataSource.LogCollection = collection;
                OnDataChanged?.Invoke(this, EventArgs.Empty);

                if (failures.Count > 0)
                {
                    MessageBox.Show(
                        $"Fetched {selected.Count - failures.Count} of {selected.Count}, " +
                        $"{collection.Entries.Count} entries.\r\n\r\nFailed:\r\n" +
                        string.Join("\r\n", failures),
                        "Favorites", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (collection.Entries.Count == 0)
                {
                    // Say so rather than drawing an empty graph: the provider suppresses a 403 by
                    // returning no entries, so "no rights" and "nothing logged" look identical here.
                    MessageBox.Show(
                        $"No log entries for {selected.Count} favorite(s) between\r\n" +
                        $"{from:yyyy-MM-dd HH:mm} and {until:yyyy-MM-dd HH:mm}.\r\n\r\n" +
                        string.Join("\r\n", counts) + "\r\n\r\n" +
                        "Either nothing was logged in this period, or the account has no rights " +
                        "on these gateways (the API returns 403, which is suppressed as 'empty').",
                        "Favorites", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
        }

        private async Task<Dictionary<string, InternalApiClient>> ResolveClientsAsync(
            List<Favorite> selected, List<string> failures, CancellationToken token)
        {
            var clients = new Dictionary<string, InternalApiClient>();

            foreach (var favorite in selected)
            {
                string key = OrganisationKey(favorite);
                if (clients.ContainsKey(key))
                    continue;

                var organisation = FindOrganisation(favorite);
                if (organisation == null)
                {
                    failures.Add($"{favorite}: organisation '{favorite.Organisation}' is not configured.");
                    continue;
                }

                var client = await apiClientProvider.CreateAuthenticatedClientAsync(organisation, token);
                if (client == null)
                {
                    failures.Add($"{favorite}: could not authenticate against '{organisation.Name}'.");
                    continue;
                }

                clients[key] = client;
            }

            return clients;
        }

        private OrganisationConfig? FindOrganisation(Favorite favorite)
            => favorite.FindOrganisation(organisations);

        private static string OrganisationKey(Favorite favorite)
            => favorite.OrganisationBasePath ?? favorite.Organisation ?? "?";

        private async Task RunWithDisabledControlsAsync(Func<CancellationToken, Task> task)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            controlStateManager.DisableAll();
            try
            {
                await task(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException ocex)
            {
                Debug.WriteLine(ocex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show($"An error occurred:\r\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBarManager.Reset();
                controlStateManager.EnableAll();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void RunOnUi(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
    }
}
