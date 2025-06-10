using DevExpress.Data.TreeList;
using DevExpress.Entity.Model.Metadata;
using FormsLib.Controls;
using FormsLib.Extentions;
using KC.InternalApi.Api;
using KC.InternalApi.Client;
using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.AppContext;
using LogViewer.Config.Models;
using LogViewer.Controls.Helpers;
using LogViewer.Logging;
using LogViewer.Providers.API;
using System.Diagnostics;

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


        public ApiSourceControl()
        {
            InitializeComponent();

            progressBarManager = new ProgressBarManager(progressBar1);

            organisationsManager = new (comboBoxOrganisations);
            organisationsManager.DisplayMember = nameof(OrganisationConfig.Name);
            organisationsManager.Progress = progressBarManager.Progress;
            organisationsManager.SelectedItemChanged += OrganisationsManager_SelectedItemChanged;

            resortsManager = new (comboBoxResorts);
            resortsManager.DisplayMember = nameof(Resort.Name);
            resortsManager.Progress = progressBarManager.Progress;
            resortsManager.SelectedItemChanged += ResortsManager_SelectedItemChanged;

            objectItemsManager = new (listBoxObjectItems);
            objectItemsManager.DisplayMember = nameof(ObjectItem.Name);
            objectItemsManager.Progress = progressBarManager.Progress;
            objectItemsManager.SelectedItemChanged += ObjectItemsManager_SelectedItemChanged;

            gatewaysManager = new (comboBoxGateways);
            gatewaysManager.DisplayMember = nameof(Gateway.Name);
            gatewaysManager.Progress = progressBarManager.Progress;
            gatewaysManager.SelectedItemChanged += GatewaysManager_SelectedItemChanged;

            infoViewManager = new InfoViewManager(richTextBoxInfoView);

            fetchManager = new LogFetcher();
            fetchManager.Progress = progressBarManager.Progress;

            buttonCancel.Click += ButtonCancel_Click;
            buttonSearch.Click += ButtonSearch_Click;
            buttonFetch.Click += ButtonFetch_Click;

            dateTimePickerFrom.Value = DateTime.Now.Date;
            dateTimePickerUntill.Value = DateTime.Now.Date + TimeSpan.FromDays(1);
        }

        public async Task LoadOrganisations(List<OrganisationConfig> organisationConfigs)
        {
            DisableControls();
            await organisationsManager.Load(new ApiOrganisationProvider(organisationConfigs));
            EnableControls();
        }


        private async void OrganisationsManager_SelectedItemChanged(object? sender, OrganisationConfig organisation)
        {
            if (organisation?.BasePath == null)
                return;

            apiClient = CreateClient(organisation);
            if(apiClient == null) 
                return;

            apiClient.AuthApi.SessionRenewed += AuthApi_SessionRenewed;

            infoViewManager.ApiUrl = organisation.BasePath?.ToString() ?? "";
            DisableControls();
            if (apiClient != null)
                await resortsManager.Load(new ApiResortProvider(apiClient, organisation.OrganisationId ?? throw new Exception()));
            EnableControls();
        }

        private void AuthApi_SessionRenewed(object? sender, EventArgs e)
        {
            
        }

        private InternalApiClient? CreateClient(OrganisationConfig organisation)
        {
            return organisation.AuthenticationMethod switch
            {
                AuthenticationMethods.GetOAuth2_OpenIdConnectClient => InternalApiClient.GetOAuth2OpenIdConnectClient(organisation.BasePath, organisation.AuthPath, organisation.ClientId),
                AuthenticationMethods.GetOAuth2_ApplicationFlowClient => InternalApiClient.GetOAuth2ApplicationFlowClient(organisation.BasePath, organisation.ClientId, organisation.ClientSecret),
                _ => null,
            };
        }

        private async void ResortsManager_SelectedItemChanged(object? sender, Resort resort)
        {
            if (apiClient == null)
                return;
            if (resort?.Id == null)
                return;

            infoViewManager.IST = resort?.Settings?.InstallCode ?? "";
            infoViewManager.Host = resort?.Settings?.ConnectionServerSettings?.ServerAddress ?? "";
            infoViewManager.COM = resort?.Settings?.ConnectionServerSettings?.ComPort?.ToString() ?? "";
            infoViewManager.TRG = resort?.Settings?.ConnectionServerSettings?.TrgPort?.ToString() ?? "";

            DisableControls();
            textBoxSearch.Text = "";
            if(resort?.Id != null)
                await objectItemsManager.Load(new ApiObjectItemProvider(apiClient, resort.Id.Value));
            EnableControls();
        }

        private async void ButtonSearch_Click(object? sender, EventArgs e)
        {
            var resort = resortsManager.SelectedItem;
            if (apiClient == null)
                return;
            if (resort?.Id == null)
                return;
            DisableControls();
            await objectItemsManager.Load(new ApiObjectItemProvider(apiClient, resort.Id.Value));
            EnableControls();
        }

        private async void ObjectItemsManager_SelectedItemChanged(object? sender, ObjectItem objectItem)
        {
            if (apiClient == null)
                return;
            if (objectItem.Id == null)
                return;
            DisableControls();
            await gatewaysManager.Load(new ApiGatewayProvider(apiClient, objectItem.Id.Value));
            gatewaysManager.SelectIndex(0);
            EnableControls();
        }

        private void GatewaysManager_SelectedItemChanged(object? sender, Gateway gateway)
        {

            if (apiClient == null)
                return;
            if (gateway.Id == null)
                return;
            DisableControls();
            infoViewManager.SID = gateway.Sid?.ToString() ?? "";
            infoViewManager.DEVID = gateway.GatewayId?.ToString() ?? "";
            EnableControls();
            
        }

        private async void ButtonFetch_Click(object? sender, EventArgs e)
        {
            var gateway = gatewaysManager.SelectedItem;
            if (apiClient == null)
                return;
            if (gateway?.Id == null)
                return;
            DisableControls();
            var logProvider = new ApiGatewayLogProvider(apiClient, gateway.Id.Value, dateTimePickerFrom.Value, dateTimePickerUntill.Value);
            string name = $"{organisationsManager.SelectedItem.Name} - {resortsManager.SelectedItem.Name} - {gatewaysManager.SelectedItem.Name}";

            DataSource.ScopeViewContext.StartDate = dateTimePickerFrom.Value;
            DataSource.ScopeViewContext.EndDate = dateTimePickerUntill.Value;

            DataSource.LogCollection = await fetchManager.Load(logProvider);
            OnDataChanged?.Invoke(this, EventArgs.Empty);
            EnableControls();
        }


        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            organisationsManager.Cancel();
            resortsManager.Cancel();
            objectItemsManager.Cancel();
            gatewaysManager.Cancel();
            fetchManager.Cancel();
        }

        private void DisableControls()
        {
            richTextBoxInfoView.Enabled = false;
            buttonFetch.Enabled = false;
            label2.Enabled = false;
            label1.Enabled = false;
            dateTimePickerUntill.Enabled = false;
            dateTimePickerFrom.Enabled = false;
            listBoxObjectItems.Enabled = false;
            textBoxSearch.Enabled = false;
            progressBar1.Enabled = false;
            comboBoxGateways.Enabled = false;
            comboBoxResorts.Enabled = false;
            comboBoxOrganisations.Enabled = false;
            buttonSearch.Enabled = false;
            buttonCancel.Enabled = true;
        }

        private void EnableControls()
        {
            richTextBoxInfoView.Enabled = true;
            buttonFetch.Enabled = true;
            label2.Enabled = true;
            label1.Enabled = true;
            dateTimePickerUntill.Enabled = true;
            dateTimePickerFrom.Enabled = true;
            listBoxObjectItems.Enabled = true;
            textBoxSearch.Enabled = true;
            progressBar1.Enabled = true;
            comboBoxGateways.Enabled = true;
            comboBoxResorts.Enabled = true;
            comboBoxOrganisations.Enabled = true;
            buttonSearch.Enabled = true;
            buttonCancel.Enabled = false;
        }
    }
}
