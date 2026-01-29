using FormsLib.Extentions;
using FormsLib.Scope;
using LogViewer.AppContext;
using LogViewer.Config;
using LogViewer.Config.Models;
using LogViewer.Controls;
using LogViewer.Files;
using LogViewer.Files.Core;
using LogViewer.Files.Csv;
using LogViewer.Files.Json;
using LogViewer.Files.Yml;
using LogViewer.Logging;
using LogViewer.Mapping;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Mappers;
using LogViewer.Utils;
using System.Reflection;

namespace LogViewer
{
    public partial class Form1 : Form
    {

        private LogViewerContext appContext;
        private FileInfo? currentFile;

        private readonly ScopeController scopeController;
        private readonly ConfigurationService configurationService;
        private readonly ApiSourceControl apiSourceControl1;
        private readonly TraceManager traceManager;
        private readonly LogViewerFileService fileService;

        public Form1()
        {
            InitializeComponent();

            configurationService = new ConfigurationService();
            appContext = new LogViewerContext();

            // Scope controller
            scopeController = new ScopeController();
            scopeController.Settings.ApplySettings(AppScopeStyles.GetDarkScope());

            scopeView1.DataSource = scopeController;
            markerView1.DataSource = scopeController;
            traceView1.DataSource = scopeController;

            scopeController.Settings.SetHorizontal(
                DateTime.Now.Date + TimeSpan.FromHours(7),
                DateTime.Now.Date + TimeSpan.FromHours(18));
            scopeController.RedrawAll();

            // API source control
            apiSourceControl1 = new ApiSourceControl();
            tabPageApi.Controls.Add(apiSourceControl1);
            apiSourceControl1.Dock = DockStyle.Fill;
            apiSourceControl1.DataSource = appContext;

            // API change → full rebuild
            apiSourceControl1.OnDataChanged += (s, e) => UpdateLogView();

            // MQTT → incremental update
            mqttSourceControl1.OnLogReceived += (s, entry) => AppendLiveEntry(entry);

            // Mappers
            var mappers = new List<ITraceMapper>
            {
                new ThermostatMapper(),
                new HvacMapper(),
                new SmarthomeMapper(),
                new GatewayGpioMapper(),
                new HiddenCodesMapper(),
                new ConnectionMapper(),
                new LegacyIndoorUnitMapper(),
            };

            traceManager = new TraceManager(scopeController, mappers);

            IFileFormatDetector detector = new FileFormatDetector();
            var registry = new FileImportExportRegistry();
            registry.AddImporter(new CyamlFileImporter());
            registry.AddImporter(new YamlFileImporter());
            registry.AddImporter(new CsvFileImporter());
            registry.AddImporter(new JsonFileImporter());
            registry.AddImporter(new GzFileImporter());
            registry.AddExporter(new YamlFileExporter());
            registry.AddExporter(new CyamlFileExporter());
            fileService = new LogViewerFileService(registry, detector);

            this.Load += Form1_Load;

#if RELEASE
            this.Size = new Size(1280, 720);
#endif

            UpdateTitle();
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                var config = configurationService.GetConfigAsync();
                UpdateMenu(config);
                await apiSourceControl1.LoadOrganisations(config.Organisations.Values.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _ = CheckForUpdates();
        }

        private void UpdateMenu(LogViewerConfig config)
        {
            menuStrip1.Items.Clear();

            menuStrip1.AddMenuItem("File/New", (menuItem) => NewDocument());
            menuStrip1.AddMenuItem("File/Open", (menuItem) => LoadFileDialog());
            menuStrip1.AddMenuItem("File/Save", (menuItem) => SaveFileDialog());
            menuStrip1.AddMenuItem("File/Close", (menuItem) => this.Close());

            menuStrip1.AddMenuItem("Help", (menuItem) => Help.ShowHelp());
        }

        private void NewDocument()
        {
            currentFile = null;
            SetContext(new LogViewerContext());
        }

        private void SetContext(LogViewerContext newContext)
        {
            appContext = newContext ?? new LogViewerContext();
            apiSourceControl1.DataSource = appContext; // important when swapping
            UpdateLogView();
        }

        private void LoadFileDialog()
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Open",
                Filter =
                    "Supported (*.yaml;*.yml;*.cyaml;*.json;*.gz;*.csv)|*.yaml;*.yml;*.cyaml;*.json;*.gz;*.csv|" +
                    "All files (*.*)|*.*",
                CheckFileExists = true,
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var file = new FileInfo(dlg.FileName);

            if (!fileService.TryLoad(file, out var loaded))
            {
                MessageBox.Show(this,
                    "Import failed or no importer registered for this file type.",
                    "Open failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            currentFile = file;
            SetContext(loaded);
        }

        private void SaveFileDialog()
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Save",
                Filter =
                    "YAML (*.yaml)|*.yaml|" +
                    "Compressed YAML (*.cyaml)|*.cyaml|" +
                    "All files (*.*)|*.*",
                AddExtension = true,
                DefaultExt = "yaml",
                RestoreDirectory = true,
                OverwritePrompt = true,
                FileName = currentFile?.Name ?? "logviewer.yaml",
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var file = new FileInfo(dlg.FileName);

            try
            {
                fileService.Save(file, appContext);
                currentFile = file;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(this,
                    ex.Message,
                    "Save not supported",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    ex.Message,
                    "Save failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateLogView()
        {
            scopeController.Settings.SetHorizontal(appContext.ScopeViewContext.StartDate, appContext.ScopeViewContext.EndDate);
            traceManager.LoadAll(appContext.LogCollection.Entries);
            scopeController.RedrawAll();
        }

        private void AppendLiveEntry(LogEntry entry)
        {
            appContext.LogCollection.Entries.Add(entry);
            traceManager.Append(entry);
            scopeController.RedrawAll();
        }

        private async Task<AppVersion?> CheckForApplicationUpdates()
        {
            AppVersion version = ApplicationIdentity.GetAppVersion();
            var checker = new GithubUpdateChecker("KooleControls", "LogViewer");
            var latestVersion = await checker.GetLatestStableVersionAsync();
            return latestVersion.BaseVersion > version.BaseVersion ? latestVersion : null;
        }

        private async Task CheckForUpdates()
        {
            toolStripStatusLabel1.Text = "Checking for application updates";
            var latestVersion = await CheckForApplicationUpdates();
            if (latestVersion == null)
            {
                toolStripStatusLabel1.Text = "Up-to-date";
                return;
            }
            
            toolStripStatusLabel1.Text = $"New application available {latestVersion.ToString()}";
            toolStripStatusLabel1.IsLink = true;
            toolStripStatusLabel1.Click += (sender, e) =>
            {
                var url = $"https://github.com/KooleControls/LogViewer/releases";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            };
        }

        private void UpdateTitle()
        {
            AppVersion version = ApplicationIdentity.GetAppVersion();
            

#if DEBUG
            this.Text = $"Log viewer (DEBUG)";
#elif DEMO
            if (version.IsPrerelease)
                this.Text = $"Log viewer '{version}' (DEMO, PRE-RELEASE)";   
            else
                this.Text = $"Log viewer '{version}' (DEMO)";   
#else
            if (version.IsPrerelease)
                this.Text = $"Log viewer '{version}' (PRE-RELEASE)";   
            else
                this.Text = $"Log viewer '{version}'";   
#endif

        }
    }
}




