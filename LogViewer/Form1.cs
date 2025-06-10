using FormsLib.Extentions;
using FormsLib.Scope;
using KCObjectsStandard.Data.Api.KC;
using LogViewer.AppContext;
using LogViewer.Config;
using LogViewer.Config.Models;
using LogViewer.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;




namespace LogViewer
{

    public partial class Form1 : Form
    {
        private readonly LogViewerContext appContext;
        private readonly ScopeController scopeController;
        private readonly LogScopeMapper scopeMapper;
        private readonly ConfigurationService configurationService;

        public Form1()
        {
            InitializeComponent();

            // Initialize application context and profile
            configurationService = new ConfigurationService();
            appContext = new LogViewerContext();

            // Initialize and configure scope controller
            scopeController = new ScopeController();
            scopeController.Settings.ApplySettings(AppScopeStyles.GetDarkScope());

            // Set up data sources for views
            scopeView1.DataSource = scopeController;
            markerView1.DataSource = scopeController;
            traceView1.DataSource = scopeController;

            // Initialize log scope mapper
            scopeMapper = new LogScopeMapper(scopeController, appContext);

            // Configure data source and event handler for source selection control
            apiSourceControl1.DataSource = appContext;
            apiSourceControl1.OnDataChanged += (s, e) => UpdateLogView();

            // Set up the menu items and update the window title
            UpdateTitle();

            // Load configuration settings
            this.Load += Form1_Load;

#if RELEASE                        
            this.Size = new Size(1280, 720);
#endif
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            // Load config from cache
            var config = await configurationService.GetConfigAsync();
            await LoadConfig(config);

            // Check for new config in background
            _= CheckForUpdates();
        }

        private async Task LoadConfig(LogViewerConfig config)
        {

            // Update menu, so it shows profiles
            UpdateMenu(config);

            // Load default profile
            var profile = config.Profiles.FirstOrDefault().Value;
            if (profile != null)
                LoadProfile(config, profile);

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

        private void LoadProfile(LogViewerConfig config, ProfileConfig profile)
        {
            appContext.Profile = profile;
            UpdateLogView();
            UpdateMenu(config);
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
            foreach (var profile in config.Profiles)
            {
                string activeText = profile.Value == appContext.Profile ? "(active)" : "";
                menuStrip1.AddMenuItem($"Profiles/{profile.Value.Name} {activeText}", (menuItem) =>
                {
                    LoadProfile(config, profile.Value);
                });
            }

            // Add help menu item
            menuStrip1.AddMenuItem("Help", (menuItem) => Help.ShowHelp());
        }

        private void UpdateLogView()
        {
            // Redraw the log view based on the current data
            scopeMapper.Redraw();
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




