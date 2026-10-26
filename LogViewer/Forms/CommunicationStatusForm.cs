using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.AppContext;
using LogViewer.Config.Models;
using LogViewer.Controls.Helpers;
using LogViewer.Files;
using LogViewer.Providers.API;
using System.Collections;
using System.Diagnostics;

namespace LogViewer.Forms
{
    /// <summary>
    /// Shows, per house of a resort, when the gateway (SNTP), the OpenTherm thermostat and the
    /// Modbus indoor unit last logged something - to see at a glance which houses stopped communicating.
    /// Houses are scanned one at a time so the web API is never hit by parallel requests.
    /// The gap analysis fetches the complete log of the listed houses instead, saves it and shows
    /// it in a <see cref="GapAnalysisForm"/>.
    /// </summary>
    public partial class CommunicationStatusForm : Form
    {
        private const int ColumnHouse = 0;
        private const int ColumnGateway = 1;
        private const int ColumnSntp = 2;
        private const int ColumnThermostat = 3;
        private const int ColumnIndoorUnit = 4;
        private const int ColumnAnyLog = 5;
        private const int ColumnStatus = 6;

        private static readonly Color MissingColor = Color.MistyRose;

        /// <summary>Raised on double-click: the logs already fetched for that house, to show in the main view.</summary>
        public event EventHandler<LogViewerContext>? ShowLogsRequested;

        private readonly ComboBoxManager<OrganisationConfig> organisationsManager;
        private readonly ComboBoxManager<Resort> resortsManager;
        private readonly ProgressBarManager progressBarManager;
        private readonly ApiClientProvider apiClientProvider;
        private readonly List<OrganisationConfig> organisationConfigs;

        // All houses of the selected resort; the name filter only changes which ones are listed.
        private readonly List<HouseRow> houses = new();

        private InternalApiClient? apiClient;
        private CancellationTokenSource? cancellationTokenSource;
        // The button that turned into "Cancel" while its scan runs; null when idle.
        private Button? runningButton;
        private int sortColumn = ColumnHouse;
        private bool sortAscending = true;

        public CommunicationStatusForm(List<OrganisationConfig> organisationConfigs)
        {
            InitializeComponent();
            this.organisationConfigs = organisationConfigs;

            progressBarManager = new ProgressBarManager(progressBar1);

            organisationsManager = new(comboBoxOrganisations);
            organisationsManager.DisplayMember = nameof(OrganisationConfig.Name);
            organisationsManager.Progress = progressBarManager.Progress;
            organisationsManager.SelectedItemChanged += (s, org) => LoadResortsForOrganisation(org);

            resortsManager = new(comboBoxResorts);
            resortsManager.DisplayMember = nameof(Resort.Name);
            resortsManager.Progress = progressBarManager.Progress;
            resortsManager.SelectedItemChanged += (s, resort) => LoadHousesForResort(resort);

            comboBoxLookBackUnit.SelectedIndex = 0;
            textBoxFilter.TextChanged += (s, e) => ShowFilteredHouses();
            buttonStart.Click += (s, e) => OnStartClick();
            buttonGaps.Click += (s, e) => OnGapsClick();
            listViewHouses.ColumnClick += (s, e) => SortByColumn(e.Column);
            listViewHouses.DoubleClick += (s, e) => OnHouseDoubleClick();

            apiClientProvider = new ApiClientProvider(new DialogClientCredentialsSource());
        }

        public async Task LoadOrganisations()
        {
            await RunWithDisabledControlsAsync(async token =>
            {
                await organisationsManager.Load(new ApiOrganisationProvider(organisationConfigs), token);
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            base.OnFormClosing(e);
        }

        private async void LoadResortsForOrganisation(OrganisationConfig? organisation)
        {
            resortsManager.Clear();
            ClearHouses();
            apiClient = null;

            if (organisation == null || organisation.BasePath == null || organisation.OrganisationId == null)
                return;

            await RunWithDisabledControlsAsync(async token =>
            {
                apiClient = await apiClientProvider.CreateAuthenticatedClientAsync(organisation, token);
                if (apiClient == null)
                    return;

                var provider = new ApiResortProviderBuilder(apiClient)
                    .ForOrganization(organisation.OrganisationId.Value)
                    .WithSortByName()
                    .Build();

                await resortsManager.Load(provider, token);
            });
        }

        private async void LoadHousesForResort(Resort? resort)
        {
            ClearHouses();

            if (resort == null || resort.Id == null || apiClient == null)
                return;

            await RunWithDisabledControlsAsync(async token =>
            {
                labelStatus.Text = "Loading houses…";
                var provider = new ApiObjectItemProviderBuilder(apiClient)
                    .ForResort(resort.Id.Value)
                    .WithRequireGateway()
                    .WithSortByName()
                    .Build();

                await foreach (var obj in provider.GetData(token, progressBarManager.Progress))
                    houses.Add(new HouseRow(obj));

                ShowFilteredHouses();
            });
        }

        private void ClearHouses()
        {
            houses.Clear();
            listViewHouses.Items.Clear();
            labelStatus.Text = "";
        }

        private void ShowFilteredHouses()
        {
            string filter = textBoxFilter.Text.Trim();
            var visible = houses
                .Where(h => (h.Object.Name ?? "").Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                .Select(h => h.Item)
                .ToArray();

            listViewHouses.BeginUpdate();
            listViewHouses.Items.Clear();
            listViewHouses.Items.AddRange(visible);
            listViewHouses.EndUpdate();

            if (runningButton == null)
                labelStatus.Text = $"{visible.Length} of {houses.Count} houses";
        }

        private void OnStartClick()
        {
            if (runningButton != null)
            {
                cancellationTokenSource?.Cancel();
                return;
            }
            ScanListedHouses();
        }

        private void OnGapsClick()
        {
            if (runningButton != null)
            {
                cancellationTokenSource?.Cancel();
                return;
            }
            FetchAndAnalyseListedHouses();
        }

        private List<HouseRow> ListedHouses()
            => listViewHouses.Items.Cast<ListViewItem>().Select(item => (HouseRow)item.Tag!).ToList();

        private TimeSpan LookBack()
            => comboBoxLookBackUnit.SelectedIndex == 0
                ? TimeSpan.FromDays((double)numericUpDownLookBack.Value)
                : TimeSpan.FromHours((double)numericUpDownLookBack.Value);

        private async void ScanListedHouses()
        {
            if (apiClient == null)
                return;

            var client = apiClient;
            var toScan = ListedHouses();
            TimeSpan maxLookBack = LookBack();

            foreach (var house in toScan)
                house.ClearResult();

            await RunScanAsync(buttonStart, async token =>
            {
                for (int i = 0; i < toScan.Count; i++)
                {
                    var house = toScan[i];
                    labelStatus.Text = $"{i + 1}/{toScan.Count}: {house.Object.Name}";
                    ((IProgress<double>)progressBarManager.Progress).Report((double)i / toScan.Count);

                    await ScanHouseAsync(client, house, maxLookBack, token);
                }
                labelStatus.Text = $"Done, {toScan.Count} houses scanned";
            });
        }

        private async void FetchAndAnalyseListedHouses()
        {
            if (apiClient == null)
                return;

            var client = apiClient;
            var toFetch = ListedHouses();
            TimeSpan period = LookBack();
            if (toFetch.Count == 0)
                return;

            if (MessageBox.Show(
                    $"This fetches the complete log of {toFetch.Count} house(s) over the last {numericUpDownLookBack.Value} " +
                    $"{comboBoxLookBackUnit.Text.ToLower()}, one house at a time.\n\n" +
                    "That is a lot of API calls and can take a long time. You can cancel while it runs; " +
                    "the houses fetched until then are kept.\n\nContinue?",
                    "Gap analysis", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            string folder = HouseLogFolder.CreateRunFolder(resortsManager.SelectedItem?.Name);
            var fetched = new List<HouseLog>();

            foreach (var house in toFetch)
                house.ClearResult();

            await RunScanAsync(buttonGaps, async token =>
            {
                DateTime until = DateTime.Now;
                DateTime from = until - period;
                IProgress<double> overall = progressBarManager.Progress;

                for (int i = 0; i < toFetch.Count; i++)
                {
                    var house = toFetch[i];
                    int done = i;
                    labelStatus.Text = $"{i + 1}/{toFetch.Count}: {house.Object.Name}";
                    var houseProgress = new Progress<double>(p =>
                    {
                        p = Math.Clamp(p, 0, 1);
                        house.ShowFetchProgress(p);
                        overall.Report((done + p) / toFetch.Count);
                    });

                    var log = await FetchHouseLogAsync(client, house, from, until, houseProgress, token);
                    if (log == null)
                        continue;

                    HouseLogFolder.Save(folder, log);
                    fetched.Add(log);
                }
                labelStatus.Text = $"Done, {fetched.Count} house logs saved";
            });

            if (fetched.Count == 0 || IsDisposed)
                return;

            var gapForm = new GapAnalysisForm();
            gapForm.Show();
            gapForm.ShowHouses(fetched, folder);
        }

        /// <summary>The scan's button acts as its cancel button while it runs.</summary>
        private async Task RunScanAsync(Button button, Func<CancellationToken, Task> scan)
        {
            string idleText = button.Text;
            runningButton = button;
            button.Text = "Cancel";
            try
            {
                await RunWithDisabledControlsAsync(scan);
            }
            finally
            {
                runningButton = null;
                if (!IsDisposed)
                    button.Text = idleText;
            }
        }

        /// <summary>The complete log of one house over the period, or null when it could not be fetched (shown in the row).</summary>
        private static async Task<HouseLog?> FetchHouseLogAsync(InternalApiClient client, HouseRow house,
            DateTime from, DateTime until, IProgress<double> progress, CancellationToken token)
        {
            house.ShowScanning();
            try
            {
                var gateway = await GetFirstGatewayAsync(client, house.Object, token);
                if (gateway?.Id == null)
                {
                    house.ShowError("No gateway");
                    return null;
                }
                house.ShowGateway(gateway);

                var provider = new ApiGatewayLogProvider(client, gateway.Id.Value, (int?)gateway.GatewayId, from, until);
                var logs = await provider.GetData(token, progress);
                // The provider returns what it has on cancel; a partial log would show up as fake gaps.
                token.ThrowIfCancellationRequested();

                var result = new LastCommunication { From = from, Until = until };
                result.Add(logs.Entries);
                house.ShowResult(result);

                var context = new LogViewerContext { LogCollection = result.Logs };
                context.ScopeViewContext.StartDate = from;
                context.ScopeViewContext.EndDate = until;
                return new HouseLog(house.Object.Name ?? $"#{house.Object.Id}", context);
            }
            catch (Exception ex) when (!token.IsCancellationRequested)
            {
                Debug.WriteLine(ex);
                house.ShowError(ex.Message);
                return null;
            }
            catch (Exception) when (token.IsCancellationRequested)
            {
                house.ShowError("Cancelled");
                throw;
            }
        }

        private static async Task ScanHouseAsync(InternalApiClient client, HouseRow house, TimeSpan maxLookBack, CancellationToken token)
        {
            house.ShowScanning();
            try
            {
                var gateway = await GetFirstGatewayAsync(client, house.Object, token);
                if (gateway == null)
                {
                    house.ShowError("No gateway");
                    return;
                }
                house.ShowGateway(gateway);
                house.ShowResult(await LastCommunicationScanner.ScanAsync(client, gateway, maxLookBack, token));
            }
            catch (Exception ex) when (!token.IsCancellationRequested)
            {
                // One broken house must not stop the scan of the others.
                Debug.WriteLine(ex);
                house.ShowError(ex.Message);
            }
            catch (Exception) when (token.IsCancellationRequested)
            {
                house.ShowError("Cancelled");
                throw;
            }
        }

        // Houses with more than one gateway are rare; the first one by name is scanned.
        private static async Task<Gateway?> GetFirstGatewayAsync(InternalApiClient client, ObjectItem obj, CancellationToken token)
        {
            if (obj.Id == null)
                return null;

            var provider = new ApiGatewayProviderBuilder(client)
                .ForObjectItem(obj.Id.Value)
                .WithSortByName()
                .Build();

            await foreach (var gateway in provider.GetData(token, null))
            {
                if (gateway.Id != null)
                    return gateway;
            }
            return null;
        }

        private void OnHouseDoubleClick()
        {
            if (listViewHouses.SelectedItems.Count == 0)
                return;

            var house = (HouseRow)listViewHouses.SelectedItems[0].Tag!;
            if (house.Result == null)
                return;

            var context = new LogViewerContext { LogCollection = house.Result.Logs };
            context.ScopeViewContext.StartDate = house.Result.From;
            context.ScopeViewContext.EndDate = house.Result.Until;
            ShowLogsRequested?.Invoke(this, context);
        }

        private void SortByColumn(int column)
        {
            sortAscending = column != sortColumn || !sortAscending;
            sortColumn = column;
            listViewHouses.ListViewItemSorter = new HouseRowComparer(column, sortAscending);
        }

        private async Task RunWithDisabledControlsAsync(Func<CancellationToken, Task> task)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            SetControlsEnabled(false);
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
                MessageBox.Show($"An error occurred:\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    progressBarManager.Reset();
                    SetControlsEnabled(true);
                }
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        // The running scan's button stays usable: it is the cancel button then.
        // The name filter and the list stay usable too, so results can be browsed during a scan.
        private void SetControlsEnabled(bool enabled)
        {
            comboBoxOrganisations.Enabled = enabled;
            comboBoxResorts.Enabled = enabled;
            numericUpDownLookBack.Enabled = enabled;
            comboBoxLookBackUnit.Enabled = enabled;
            buttonStart.Enabled = enabled || runningButton == buttonStart;
            buttonGaps.Enabled = enabled || runningButton == buttonGaps;
        }

        /// <summary>One house in the list: the object, its list item and the scan result once known.</summary>
        private class HouseRow
        {
            public ObjectItem Object { get; }
            public ListViewItem Item { get; }
            public LastCommunication? Result { get; private set; }

            public HouseRow(ObjectItem obj)
            {
                Object = obj;
                Item = new ListViewItem(obj.Name ?? $"#{obj.Id}") { Tag = this, UseItemStyleForSubItems = false };
                for (int column = ColumnGateway; column <= ColumnStatus; column++)
                    Item.SubItems.Add("");
            }

            public void ClearResult()
            {
                Result = null;
                for (int column = ColumnSntp; column <= ColumnStatus; column++)
                    SetCell(column, "", missing: false);
            }

            public void ShowScanning() => SetCell(ColumnStatus, "Scanning…", missing: false);

            // The overall progress bar moves only 1/n per house, so the row shows how far this house is.
            public void ShowFetchProgress(double fraction)
            {
                if (Result == null)
                    SetCell(ColumnStatus, $"Fetching… {fraction:P0}", missing: false);
            }

            public void ShowError(string message) => SetCell(ColumnStatus, message, missing: true);

            public void ShowGateway(Gateway gateway)
            {
                string name = string.IsNullOrWhiteSpace(gateway.Name) ? "" : $"{gateway.Name} ";
                SetCell(ColumnGateway, $"{name}({gateway.GatewayId})", missing: false);
            }

            public void ShowResult(LastCommunication result)
            {
                Result = result;
                SetTimeCell(ColumnSntp, result.Sntp);
                SetTimeCell(ColumnThermostat, result.Thermostat);
                SetTimeCell(ColumnIndoorUnit, result.IndoorUnit);
                SetTimeCell(ColumnAnyLog, result.AnyLog);

                var problems = new List<string>();
                if (result.Sntp == null) problems.Add("Gateway offline");
                if (result.Thermostat == null) problems.Add("OpenTherm");
                if (result.IndoorUnit == null) problems.Add("Modbus");
                SetCell(ColumnStatus, problems.Count == 0 ? "OK" : string.Join(", ", problems), missing: problems.Count > 0);
            }

            public DateTime? TimeOf(int column) => column switch
            {
                ColumnSntp => Result?.Sntp,
                ColumnThermostat => Result?.Thermostat,
                ColumnIndoorUnit => Result?.IndoorUnit,
                ColumnAnyLog => Result?.AnyLog,
                _ => null,
            };

            private void SetTimeCell(int column, DateTime? time)
                => SetCell(column, time?.ToString("yyyy-MM-dd HH:mm:ss") ?? "not found", missing: time == null);

            // Called from the scan loop, which runs on the UI thread (async continuations), so no Invoke needed.
            private void SetCell(int column, string text, bool missing)
            {
                var cell = Item.SubItems[column];
                cell.Text = text;
                cell.BackColor = missing ? MissingColor : Item.BackColor;
            }
        }

        private class HouseRowComparer : IComparer
        {
            private readonly int column;
            private readonly bool ascending;

            public HouseRowComparer(int column, bool ascending)
            {
                this.column = column;
                this.ascending = ascending;
            }

            public int Compare(object? x, object? y)
            {
                var a = (HouseRow)((ListViewItem)x!).Tag!;
                var b = (HouseRow)((ListViewItem)y!).Tag!;

                int result = column is ColumnSntp or ColumnThermostat or ColumnIndoorUnit or ColumnAnyLog
                    ? Nullable.Compare(a.TimeOf(column), b.TimeOf(column))
                    : string.Compare(a.Item.SubItems[column].Text, b.Item.SubItems[column].Text, StringComparison.CurrentCultureIgnoreCase);

                return ascending ? result : -result;
            }
        }
    }
}
