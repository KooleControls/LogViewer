using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.Config.Models;
using LogViewer.Controls.Helpers;
using LogViewer.Files;
using LogViewer.Providers.API;
using System.Diagnostics;

namespace LogViewer.Forms
{
    /// <summary>
    /// Self-contained dialog to export a resort (.kcresort) or a whole organisation
    /// (.kcbundle) for the KC220 config tool. Has its own organisation/resort selection
    /// and login, so it does not depend on the WebAPI source tab.
    /// </summary>
    public partial class ExportForm : Form
    {
        private readonly ComboBoxManager<OrganisationConfig> organisationsManager;
        private readonly ComboBoxManager<Resort> resortsManager;
        private readonly ProgressBarManager progressBarManager;
        private readonly ApiClientProvider apiClientProvider;
        private readonly List<OrganisationConfig> organisationConfigs;

        private InternalApiClient? apiClient;
        private CancellationTokenSource? cancellationTokenSource;
        private bool isExporting;

        public ExportForm(List<OrganisationConfig> organisationConfigs)
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

            buttonExportResort.Click += (s, e) => OnExportResortClick();
            buttonExportAll.Click += (s, e) => OnExportAllClick();
            buttonCancel.Click += (s, e) => cancellationTokenSource?.Cancel();

            apiClientProvider = new ApiClientProvider(new DialogClientCredentialsSource());
        }

        public async Task LoadOrganisations()
        {
            await RunWithDisabledControlsAsync(async token =>
            {
                await organisationsManager.Load(new ApiOrganisationProvider(organisationConfigs), token);
            });
        }

        private async void LoadResortsForOrganisation(OrganisationConfig? organisation)
        {
            resortsManager.Clear();
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

        private void OnExportResortClick()
        {
            if (isExporting) { cancellationTokenSource?.Cancel(); return; }
            ExportResort();
        }

        private void OnExportAllClick()
        {
            if (isExporting) { cancellationTokenSource?.Cancel(); return; }
            ExportAllResorts();
        }

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

            if (MessageBox.Show(
                    "Exporting this resort fetches all its objects and gateways from the web API.\n\n" +
                    "That means many API calls and it can take a while (a minute or more for large resorts).\n\n" +
                    "Continue?",
                    "Export resort", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            string fileName;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Export resort";
                dlg.Filter = "Resort export (*.kcresort)|*.kcresort|XML files (*.xml)|*.xml";
                dlg.FileName = ResortExportService.MakeSafeFileName(resort.Name) + ".kcresort";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                fileName = dlg.FileName;
            }

            isExporting = true;
            buttonExportResort.Text = "Cancel";
            buttonExportAll.Enabled = false;
            try
            {
                await RunWithDisabledControlsAsync(async token =>
                {
                    var export = await ResortExportService.BuildResortExportAsync(apiClient, org, resort, progressBarManager.Progress, token);
                    ((IProgress<double>)progressBarManager.Progress).Report(1);
                    ResortExportService.SaveResort(export, fileName);

                    MessageBox.Show(
                        $"Exported resort '{resort.Name}' with {export.Devices.Count} device(s) to:\n{fileName}",
                        "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            finally
            {
                isExporting = false;
                buttonExportResort.Text = "Export resort";
                buttonExportAll.Enabled = true;
            }
        }

        private async void ExportAllResorts()
        {
            var org = organisationsManager.SelectedItem;
            if (org == null || apiClient == null || org.OrganisationId == null)
            {
                MessageBox.Show("Select an organisation first (and log in).", "Export all resorts",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    "Exporting ALL resorts of this organisation fetches every resort, object and gateway " +
                    "from the web API.\n\n" +
                    "That is a LOT of API calls and can take a long time — many minutes for large " +
                    "organisations (e.g. Roompot took several minutes for ~60 resorts).\n\n" +
                    "You can cancel while it runs. Continue?",
                    "Export all resorts", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            string fileName;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Export all resorts of this organisation";
                dlg.Filter = "Resort bundle (*.kcbundle)|*.kcbundle|XML files (*.xml)|*.xml";
                dlg.FileName = ResortExportService.MakeSafeFileName(org.Name) + ".kcbundle";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                fileName = dlg.FileName;
            }

            isExporting = true;
            buttonExportAll.Text = "Cancel";
            buttonExportResort.Enabled = false;
            try
            {
                await RunWithDisabledControlsAsync(async token =>
                {
                    var bundle = await ResortExportService.BuildBundleAsync(apiClient, org, progressBarManager.Progress, token);
                    ((IProgress<double>)progressBarManager.Progress).Report(1);
                    ResortExportService.SaveBundle(bundle, fileName);

                    int totalDevices = bundle.Resorts.Sum(r => r.Devices.Count);
                    MessageBox.Show(
                        $"Exported {bundle.Resorts.Count} resort(s) with {totalDevices} device(s) to:\n{fileName}",
                        "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            finally
            {
                isExporting = false;
                buttonExportAll.Text = "Export all";
                buttonExportResort.Enabled = true;
            }
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
                progressBarManager.Reset();
                SetControlsEnabled(true);
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        // Cancel blijft altijd bruikbaar; export-knoppen fungeren als Cancel tijdens een lopende export.
        private void SetControlsEnabled(bool enabled)
        {
            comboBoxOrganisations.Enabled = enabled;
            comboBoxResorts.Enabled = enabled;
            if (!isExporting)
            {
                buttonExportResort.Enabled = enabled;
                buttonExportAll.Enabled = enabled;
            }
        }
    }
}
