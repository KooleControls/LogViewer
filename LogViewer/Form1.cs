using FormsLib.Extentions;
using FormsLib.Scope;
using LogViewer.AppContext;
using LogViewer.Config;
using LogViewer.Config.Models;
using LogViewer.Controls;
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
                new HiddenCodesMapper(),
                new ConnectionMapper(),
                new LegacyIndoorUnitMapper(),
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

        private void UpdateMenu(LogViewerConfig config)
        {
            menuStrip1.Items.Clear();

            menuStrip1.AddMenuItem("File/New", (menuItem) => scopeController.ClearData());
            menuStrip1.AddMenuItem("File/Open", (menuItem) => { AppContextFileStore.LoadLogDialog(appContext); UpdateLogView(); });
            menuStrip1.AddMenuItem("File/Save", (menuItem) => { AppContextFileStore.SaveLogDialog(appContext); });
            menuStrip1.AddMenuItem("File/Close", (menuItem) => this.Close());

            menuStrip1.AddMenuItem("Help", (menuItem) => Help.ShowHelp());
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




