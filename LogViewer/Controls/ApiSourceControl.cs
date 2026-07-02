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
using System.Xml.Serialization;

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
        private readonly Button buttonExportResort;
        private bool isExporting;


        CancellationTokenSource? cancellationTokenSource;

        public ApiSourceControl()
        {
            InitializeComponent();

            // "Export" naast de resort-keuzelijst: exporteert het gekozen resort (instellingen + alle
            // apparaten) naar een .kcresort-bestand dat de KC220 config tool kan importeren.
            comboBoxResorts.Width = 150;
            buttonExportResort = new Button
            {
                Text = "Export",
                Location = new Point(comboBoxResorts.Right + 6, comboBoxResorts.Top),
                Size = new Size(74, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = true,
            };
            // Tijdens het exporteren wordt deze knop een "Cancel"-knop (niet uitschakelen -> niet in
            // de controlStateManager-lijst), zodat de gebruiker het proces kan afbreken.
            buttonExportResort.Click += (s, e) => OnExportButtonClick();
            Controls.Add(buttonExportResort);
            buttonExportResort.BringToFront();

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


        // Klik op de export/cancel-knop: tijdens een lopende export fungeert hij als Cancel.
        private void OnExportButtonClick()
        {
            if (isExporting)
            {
                cancellationTokenSource?.Cancel();
                return;
            }
            ExportResort();
        }

        // Exporteert het gekozen resort (gedeelde connectie-server instellingen + alle apparaten) naar
        // een .kcresort-bestand voor de KC220 config tool. Haalt voor elk object de gateways op; elke
        // gateway wordt een apparaat (Software ID = Sid, Device ID = GatewayId).
        private async void ExportResort()
        {
            var org = organisationsManager.SelectedItem;
            var resort = resortsManager.SelectedItem;
            if (org == null || resort == null || apiClient == null || resort.Id == null)
            {
                MessageBox.Show("Select an organisation and resort first.", "Export resort",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fileName;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Export resort";
                dlg.Filter = "Resort export (*.kcresort)|*.kcresort|XML files (*.xml)|*.xml";
                dlg.FileName = MakeSafeFileName(resort.Name) + ".kcresort";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                fileName = dlg.FileName;
            }

            // De export-knop wordt tijdens het exporteren een Cancel-knop.
            isExporting = true;
            buttonExportResort.Text = "Cancel";
            try
            {
            await RunWithDisabledControlsAsync(async token =>
            {
                var export = new ResortExport
                {
                    Organisation = org.Name,
                    OrganisationHost = ExtractHost(org.BasePath),
                    ResortName = resort.Name,
                    ServerAddress = resort.Settings?.ConnectionServerSettings?.ServerAddress,
                    ComPort = resort.Settings?.ConnectionServerSettings?.ComPort?.ToString(),
                    TrgPort = resort.Settings?.ConnectionServerSettings?.TrgPort?.ToString(),
                    InstallCode = resort.Settings?.InstallCode,
                };

                // Alle objecten van het resort ophalen.
                var objectsProvider = new ApiObjectItemProviderBuilder(apiClient)
                    .ForResort(resort.Id.Value)
                    .WithSortByName()
                    .Build();

                var objects = new List<ObjectItem>();
                await foreach (var obj in objectsProvider.GetData(token, null))
                    objects.Add(obj);

                // Per object de gateways ophalen -> apparaten.
                for (int i = 0; i < objects.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    (progressBarManager.Progress as IProgress<double>)?.Report((double)i / Math.Max(1, objects.Count));

                    var obj = objects[i];
                    if (obj.Id == null)
                        continue;

                    var gatewaysProvider = new ApiGatewayProviderBuilder(apiClient)
                        .ForObjectItem(obj.Id.Value)
                        .WithSortByName()
                        .Build();

                    var gateways = new List<Gateway>();
                    await foreach (var gw in gatewaysProvider.GetData(token, null))
                        gateways.Add(gw);

                    bool multiple = gateways.Count > 1;
                    foreach (var gw in gateways)
                    {
                        export.Devices.Add(new ResortExportDevice
                        {
                            Name = multiple ? $"{obj.Name} - {gw.Name}" : obj.Name,
                            Sid = (int)(gw.Sid ?? 0),
                            DeviceId = (int)(gw.GatewayId ?? 0),
                        });
                    }
                }

                (progressBarManager.Progress as IProgress<double>)?.Report(1);

                var serializer = new XmlSerializer(typeof(ResortExport));
                using (var stream = File.Create(fileName))
                    serializer.Serialize(stream, export);

                MessageBox.Show(
                    $"Exported resort '{resort.Name}' with {export.Devices.Count} device(s) to:\n{fileName}",
                    "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            }
            finally
            {
                isExporting = false;
                buttonExportResort.Text = "Export";
            }
        }

        private static string ExtractHost(string? basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                return "";
            try { return new Uri(basePath).Host; } catch { return ""; }
        }

        private static string MakeSafeFileName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "resort";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
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
