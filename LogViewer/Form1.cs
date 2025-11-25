using FormsLib.Extentions;
using FormsLib.Scope;
using LogViewer.AppContext;
using LogViewer.Config;
using LogViewer.Config.Models;
using LogViewer.Controls;
using LogViewer.Logging;
using LogViewer.Mapping;
using LogViewer.Mapping.Mappers;
using LogViewer.Mapping.Models;
using LogViewer.Utils;
using System;
using System.Reflection;

namespace LogViewer
{
    public partial class Form1 : Form
    {
        private readonly LogViewerContext appContext;
        private readonly ScopeController scopeController;
        private readonly ConfigurationService configurationService;
        private readonly ApiSourceControl apiSourceControl1;
        private readonly TraceManager traceManager;

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
                new UnknownMapper(),
            };

            traceManager = new TraceManager(scopeController, mappers);

            this.Load += Form1_Load;

#if RELEASE
            this.Size = new Size(1280, 720);
#endif

            UpdateTitle();
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            // Load config from cache
            
            try
            {
                var config = await configurationService.GetConfigAsync();
                await LoadConfig(config);
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check for new config in background
            _ = CheckForUpdates();
        }

        private async Task LoadConfig(LogViewerConfig config)
        {

            // Update menu, so it shows profiles
            UpdateMenu(config);

            // Load the organisations
            await apiSourceControl1.LoadOrganisations(config.Organisations.Values.ToList());
        }
        private async Task<bool> CheckForApplicationUpdates()
        {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            var checker = new GithubUpdateChecker("KooleControls", "LogViewer");
            var latestVersion = await checker.GetLatestVersionAsync();
            return latestVersion > version; 
        }
        private async Task<bool> CheckForConfigurationUpdates()
        {
            return await configurationService.DownloadIfUpdatedAsync();
        }
        private async Task CheckForUpdates()
        {
            toolStripStatusLabel1.Text = "Checking for config updates";
            bool configUpdateAvailable = await CheckForConfigurationUpdates();
            if (configUpdateAvailable)
            {
                toolStripStatusLabel1.Text = "New config available, reloading...";
                // Reload the configuration
                var config = await configurationService.GetConfigAsync();
                if (config != null)
                    await LoadConfig(config);

            }

            toolStripStatusLabel1.Text = "Checking for application updates";
            bool applicationUpdateAvailable = await CheckForApplicationUpdates();
            if (applicationUpdateAvailable)
            {
                toolStripStatusLabel1.Text = $"New application version available";
                toolStripStatusLabel1.IsLink = true;
                toolStripStatusLabel1.Click += (sender, e) => {
                    var url = $"https://github.com/KooleControls/LogViewer/releases";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                };
                return;
            }

            toolStripStatusLabel1.Text = "Up-to-date";
        }
        private void UpdateMenu(LogViewerConfig config)
        {
            // Clear existing menu items
            menuStrip1.Items.Clear();

            // Add menu items for file operations
            menuStrip1.AddMenuItem("File/New", (menuItem) => scopeController.ClearData());
            menuStrip1.AddMenuItem("File/Open", (menuItem) => { AppContextFileStore.LoadLogDialog(appContext); UpdateLogView(); });
            menuStrip1.AddMenuItem("File/Save", (menuItem) => { AppContextFileStore.SaveLogDialog(appContext); });
            menuStrip1.AddMenuItem("File/Close", (menuItem) => this.Close());

            // Add menu items for profile selection


            // Add help menu item
            menuStrip1.AddMenuItem("Help", (menuItem) => Help.ShowHelp());
        }

        private void UpdateLogView()
        {
            scopeController.Settings.SetHorizontal(
                appContext.ScopeViewContext.StartDate,
                appContext.ScopeViewContext.EndDate);
            traceManager.LoadAll(appContext.LogCollection.Entries);
        }

        private void AppendLiveEntry(LogEntry entry)
        {
            appContext.LogCollection.Entries.Add(entry);
            traceManager.LoadAll(appContext.LogCollection.Entries);
        }

        private void UpdateTitle()
        {
            // Get the version of the application
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;

            // Retrieve the build type from the configuration
            string buildType = GetBuildType();

            // Update the form title based on the build type
            this.Text = $"Log viewer '{version}' ({buildType})";
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 537: //WM_DEVICECHANGE
                    //comConsoleControl1.RefresComPorts();
                    break;
            }
            base.WndProc(ref m);
        }

        public static string GetBuildType()
        {
#if DEBUG
            return "DEBUG";
#elif RELEASE
            return "RELEASE";
#elif DEMO
            return "DEMO";
#else
            return "UNKNOWN";
#endif
        }
    }
}




