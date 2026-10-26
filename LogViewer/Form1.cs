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
using LogViewer.Forms;
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
        private readonly FavoritesSourceControl favoritesSourceControl1;
        private readonly TcpSourceControl tcpSourceControl1;
        private readonly TraceManager traceManager;
        private readonly LogViewerFileService fileService;
        private CommunicationStatusForm? communicationStatusForm;

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

            // Favorites source control: saved gateways, several at a time
            favoritesSourceControl1 = new FavoritesSourceControl();
            tabPageFavorites.Controls.Add(favoritesSourceControl1);
            favoritesSourceControl1.Dock = DockStyle.Fill;
            favoritesSourceControl1.DataSource = appContext;

            // Favorites change → full rebuild
            favoritesSourceControl1.OnDataChanged += (s, e) => UpdateLogView();

            // Device (TCP) source control
            tcpSourceControl1 = new TcpSourceControl();
            tabPageDevice.Controls.Add(tcpSourceControl1);
            tcpSourceControl1.Dock = DockStyle.Fill;

            // Device (TCP) → incremental update
            tcpSourceControl1.OnLogReceived += (s, entry) => AppendLiveEntry(entry);
            tcpSourceControl1.OnClearRequested += (s, e) => ClearLog();

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
                new GatewayMapper(),
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

                var organisations = config.Organisations.Values.ToList();
                favoritesSourceControl1.LoadOrganisations(organisations);
                await apiSourceControl1.LoadOrganisations(organisations);
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

            // Categories come from the mappers: "Temperature" is the measured value on thermostats,
            // HVAC units and legacy indoor units - the three places an actual temperature is logged.
            menuStrip1.AddCheckboxMenuItem("View/Only temperatures", (menuItem) =>
            {
                traceManager.SetCategoryFilter(menuItem.Checked ? ["Temperature"] : []);
                scopeController.RedrawAll();
            });

            menuStrip1.AddMenuItem("Tools/Export for KC220 tool…", (menuItem) => OpenExportForm());
            menuStrip1.AddMenuItem("Tools/Communication status…", (menuItem) => OpenCommunicationStatusForm());
            menuStrip1.AddMenuItem("Tools/Gap analysis (saved logs)…", (menuItem) => new GapAnalysisForm().Show());
            menuStrip1.AddMenuItem("Tools/Open config folder…", (menuItem) => OpenUserConfigFolder());

            menuStrip1.AddMenuItem("Help", (menuItem) => Help.ShowHelp());
        }

        /// <summary>
        /// Opens Explorer on the per-user config folder with user_organisations.yaml - where
        /// custom organisations are defined - selected, so it can be edited by hand.
        /// </summary>
        private void OpenUserConfigFolder()
        {
            string folder = ConfigurationService.UserConfigFolder;
            string file = ConfigurationService.UserOrganisationsFile;
            try
            {
                string args = File.Exists(file) ? $"/select,\"{file}\"" : $"\"{folder}\"";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", args)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open {folder}: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenExportForm()
        {
            List<Config.Models.OrganisationConfig> orgs;
            try
            {
                var config = configurationService.GetConfigAsync();
                orgs = config.Organisations.Values.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var form = new ExportForm(orgs);
            form.Shown += async (s, e) => await form.LoadOrganisations();
            form.ShowDialog(this);
        }

        /// <summary>
        /// Opened modeless, so the logs of a house shown from it can be inspected in the main
        /// view while the list stays open.
        /// </summary>
        private void OpenCommunicationStatusForm()
        {
            if (communicationStatusForm != null && !communicationStatusForm.IsDisposed)
            {
                communicationStatusForm.Activate();
                return;
            }

            List<Config.Models.OrganisationConfig> orgs;
            try
            {
                var config = configurationService.GetConfigAsync();
                orgs = config.Organisations.Values.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new CommunicationStatusForm(orgs);
            form.Shown += async (s, e) => await form.LoadOrganisations();
            form.ShowLogsRequested += (s, context) =>
            {
                currentFile = null;
                SetContext(context);
                Activate();
            };
            communicationStatusForm = form;
            form.Show();
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
            favoritesSourceControl1.DataSource = appContext;
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

        private void ClearLog()
        {
            appContext.LogCollection.Entries.Clear();
            traceManager.LoadAll(appContext.LogCollection.Entries);
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




