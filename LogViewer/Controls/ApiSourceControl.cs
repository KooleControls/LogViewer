using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.AppContext;
using LogViewer.Config.Models;
using LogViewer.Controls.Helpers;
using LogViewer.Files;
using LogViewer.Providers.API;
using Microsoft.Extensions.Caching.Hybrid;
using System.Diagnostics;
using System.Security.Cryptography;

namespace LogViewer.Controls
{
    public partial class ApiSourceControl : UserControl
    {
        public LogViewerContext DataSource { get; set; } = new LogViewerContext();
        public event EventHandler? OnDataChanged;

        private readonly ComboBoxManager<OrganisationConfig> organisationsManager;
        private readonly ComboBoxManager<Resort> resortsManager;
        private readonly ListBoxManager<ObjectItem> objectItemsManager;
        private readonly ComboBoxManager<Gateway> gatewaysManager;
        private readonly InfoViewManager infoViewManager;
        private readonly ProgressBarManager progressBarManager;
        private readonly LogFetcher fetchManager;
        private InternalApiClient? apiClient;
        private readonly ControlStateManager controlStateManager;
        private readonly ApiClientProvider apiClientProvider;
        private readonly Button buttonOpen220;


        CancellationTokenSource? cancellationTokenSource;

        public ApiSourceControl()
        {
            InitializeComponent();

            // "220 tool" naast de gateway-keuzelijst: opent de KC220 config tool op het gekozen apparaat
            // (schrijft de verbinding naar een tijdelijk .kcresort en start de tool met /connect).
            buttonOpen220 = new Button
            {
                Text = "220 tool",
                Size = new Size(80, 23),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                UseVisualStyleBackColor = true,
            };
            // Rechteruitlijning met Fetch/Cancel/Search (rechterrand x=233); combo krimpt tot links van de knop.
            buttonOpen220.Location = new Point(233 - buttonOpen220.Width, comboBoxGateways.Top);
            comboBoxGateways.Width = buttonOpen220.Left - 6 - comboBoxGateways.Left;
            buttonOpen220.Click += (s, e) => OpenInKc220Tool();
            Controls.Add(buttonOpen220);
            buttonOpen220.BringToFront();

            controlStateManager = new ControlStateManager([
                richTextBoxInfoView,
                buttonFetch,
                label2,
                label1,
                dateTimePickerUntill,
                dateTimePickerFrom,
                listBoxObjectItems,
                textBoxSearch,
                progressBar1,
                comboBoxGateways,
                comboBoxResorts,
                comboBoxOrganisations,
                buttonSearch,
                buttonOpen220,
            ]);


            progressBarManager = new ProgressBarManager(progressBar1);

            organisationsManager = new(comboBoxOrganisations);
            organisationsManager.DisplayMember = nameof(OrganisationConfig.Name);
            organisationsManager.Progress = progressBarManager.Progress;
            organisationsManager.SelectedItemChanged += (s, organisation) => LoadResortsForOrganisation(organisation);

            resortsManager = new(comboBoxResorts);
            resortsManager.DisplayMember = nameof(Resort.Name);
            resortsManager.Progress = progressBarManager.Progress;
            resortsManager.SelectedItemChanged += (s, resort) =>
            {
                textBoxSearch.Text = "";
                LoadObjectItemsForResort(resortsManager.SelectedItem, null);
            };

            objectItemsManager = new(listBoxObjectItems);
            objectItemsManager.DisplayMember = nameof(ObjectItem.Name);
            objectItemsManager.Progress = progressBarManager.Progress;
            objectItemsManager.SelectedItemChanged += (s, objectItem) => LoadGatewaysForObjectItem(objectItem);

            gatewaysManager = new(comboBoxGateways);
            gatewaysManager.DisplayMember = nameof(Gateway.Name);
            gatewaysManager.Progress = progressBarManager.Progress;
            gatewaysManager.SelectedItemChanged += (s, gateway) => UpdateInfoForGateway(gateway);

            infoViewManager = new InfoViewManager(richTextBoxInfoView);

            fetchManager = new LogFetcher();
            fetchManager.Progress = progressBarManager.Progress;

            buttonCancel.Click += (s, e) => cancellationTokenSource?.Cancel();
            buttonSearch.Click += (s, e) => LoadObjectItemsForResort(resortsManager.SelectedItem, textBoxSearch.Text);
            buttonFetch.Click += (s, e) => FetchLogsForGateway(gatewaysManager.SelectedItem);

            checkBoxRequireGateways.CheckedChanged += (s, e) => LoadObjectItemsForResort(resortsManager.SelectedItem, textBoxSearch.Text);

            dateTimePickerFrom.Value = DateTime.Now.Date;
            dateTimePickerUntill.Value = DateTime.Now.Date + TimeSpan.FromDays(1);

            var clientCredentialsDialog = new DialogClientCredentialsSource();
            apiClientProvider = new ApiClientProvider(clientCredentialsDialog);
        }


        public async Task LoadOrganisations(List<OrganisationConfig> organisationConfigs)
        {
            await RunWithDisabledControlsAsync(async token =>
            {
                await organisationsManager.Load(new ApiOrganisationProvider(organisationConfigs), token);
            });
        }


        private async void LoadResortsForOrganisation(OrganisationConfig? organisation)
        {
            resortsManager.Clear();
            infoViewManager.ClearOrganisationInfo();

            if (organisation == null || organisation?.BasePath == null || organisation.OrganisationId == null) 
                return;
            
            infoViewManager.Update(organisation);

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

        private async void LoadObjectItemsForResort(Resort? resort, string? searchField)
        {
            objectItemsManager.Clear();
            infoViewManager.ClearResortInfo();

            if (resort == null || apiClient == null || resort?.Id == null)
                return;
            
            infoViewManager.Update(resort.Settings);

            await RunWithDisabledControlsAsync(async token =>
            {
                var builder = new ApiObjectItemProviderBuilder(apiClient)
                    .ForResort(resort.Id.Value)
                    .WithSortByName();

                if (checkBoxRequireGateways.Checked)
                    builder.WithRequireGateway();

                if (!string.IsNullOrWhiteSpace(searchField))
                    builder.WhereName(searchField);


                var provider = builder.Build();
                await objectItemsManager.Load(provider, token);
            });
        }


        // Opent de KC220 config tool op het gekozen apparaat: schrijft de verbinding naar een tijdelijk
        // .kcresort en start de tool met /connect "<pad>". Geen API-calls nodig (alle gegevens staan al
        // in de geselecteerde items).
        private void OpenInKc220Tool()
        {
            var org = organisationsManager.SelectedItem;
            var resort = resortsManager.SelectedItem;
            var obj = listBoxObjectItems.SelectedItem as ObjectItem;
            var gw = gatewaysManager.SelectedItem;
            if (org == null || resort == null || gw == null)
            {
                MessageBox.Show("Select an organisation, resort and gateway first.", "Open in KC220 tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string? exe = GetKc220ToolPath();
            if (string.IsNullOrEmpty(exe))
                return;

            var export = new ResortExport
            {
                Organisation = org.Name,
                OrganisationHost = ResortExportService.ExtractHost(org.BasePath),
                ResortName = resort.Name,
                ServerAddress = resort.Settings?.ConnectionServerSettings?.ServerAddress,
                ComPort = resort.Settings?.ConnectionServerSettings?.ComPort?.ToString(),
                TrgPort = resort.Settings?.ConnectionServerSettings?.TrgPort?.ToString(),
                InstallCode = resort.Settings?.InstallCode,
            };
            export.Devices.Add(new ResortExportDevice
            {
                Name = string.IsNullOrWhiteSpace(gw.Name) ? obj?.Name : gw.Name,
                Object = obj?.Name,
                Sid = (int)(gw.Sid ?? 0),
                DeviceId = (int)(gw.GatewayId ?? 0),
            });

            try
            {
                string temp = Path.Combine(Path.GetTempPath(), "kc220_connect_" + Guid.NewGuid().ToString("N") + ".kcresort");
                ResortExportService.SaveResort(export, temp);

                Process.Start(new ProcessStartInfo(exe, $"/connect \"{temp}\"") { UseShellExecute = false });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the KC220 tool:\n{ex.Message}", "Open in KC220 tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Onthoudt het pad naar 'KC220 Config tool.exe' in %LOCALAPPDATA%\LogViewer; vraagt er één keer om.
        private static string? GetKc220ToolPath()
        {
            string cfgDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LogViewer");
            string cfgFile = Path.Combine(cfgDir, "kc220_tool_path.txt");

            string? path = null;
            try { if (File.Exists(cfgFile)) path = File.ReadAllText(cfgFile).Trim(); } catch { }
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return path;

            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Locate 'KC220 Config tool.exe'";
                dlg.Filter = "KC220 config tool (KC220 Config tool.exe)|KC220 Config tool.exe|Programs (*.exe)|*.exe";
                if (!string.IsNullOrEmpty(path))
                {
                    try { dlg.InitialDirectory = Path.GetDirectoryName(path); } catch { }
                }
                if (dlg.ShowDialog() != DialogResult.OK)
                    return null;
                path = dlg.FileName;
            }

            try { Directory.CreateDirectory(cfgDir); File.WriteAllText(cfgFile, path); } catch { }
            return path;
        }

        private async void LoadGatewaysForObjectItem(ObjectItem? objectItem)
        {
            gatewaysManager.Clear();

            if (objectItem == null || apiClient == null || objectItem.Id == null)
                return;
            
            await RunWithDisabledControlsAsync(async token =>
            {
                var provider = new ApiGatewayProviderBuilder(apiClient)
                    .ForObjectItem(objectItem.Id.Value)
                    .WithSortByName()
                    .Build();

                await gatewaysManager.Load(provider, token);
                gatewaysManager.SelectIndex(0);
            });
        }

        private void UpdateInfoForGateway(Gateway? gateway)
        {
            infoViewManager.ClearGatewayInfo();

            if (gateway == null || gateway.Id == null || apiClient == null)
                return;
            
            infoViewManager.Update(gateway);
        }

        private async void FetchLogsForGateway(Gateway? gateway)
        {
            if (gateway == null || gateway.Id == null || apiClient == null)
                return;

            await RunWithDisabledControlsAsync(async token =>
            {
                var logProvider = new ApiGatewayLogProvider(apiClient, gateway.Id.Value, dateTimePickerFrom.Value, dateTimePickerUntill.Value);


                infoViewManager.ClearApiStatsInfo();
                logProvider.OnResponseTimeReported += (s, responseTime) =>
                {
                    infoViewManager.ReportApiCall(responseTime);
                };

                DataSource.ScopeViewContext.StartDate = dateTimePickerFrom.Value;
                DataSource.ScopeViewContext.EndDate = dateTimePickerUntill.Value;

                DataSource.LogCollection = await fetchManager.Load(logProvider, token);
                OnDataChanged?.Invoke(this, EventArgs.Empty);
            });
        }

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
                // Dump all we know in debug
                Debug.WriteLine(ex);

                MessageBox.Show($"An error occurred:\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBarManager.Reset();
                controlStateManager.EnableAll();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}
