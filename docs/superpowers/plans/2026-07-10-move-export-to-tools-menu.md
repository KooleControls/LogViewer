# Move Resort Export to Tools Menu — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move the KC220 resort-export feature off the WebAPI source tab into a dedicated, self-contained dialog opened from a new Tools menu, keeping the 220 tool button on the tab.

**Architecture:** Extract the export business logic into a UI-free `ResortExportService` shared by both the new dialog and the existing 220 tool launcher. Add a self-contained `ExportForm` (own org/resort dropdowns + login) opened modally from a new `Tools` menu in `Form1`. Then strip the two export buttons from `ApiSourceControl` and tidy its layout.

**Tech Stack:** C# / .NET 8 WinForms, `XmlSerializer`, existing helpers (`ComboBoxManager<T>`, `ApiClientProvider`, `ApiOrganisationProvider`, `ApiResortProviderBuilder`, `ProgressBarManager`).

## Global Constraints

- Target framework: `net8.0-windows7.0`; WinForms, nullable enabled.
- No automated test suite exists. Every task is verified by `dotnet build LogViewer.sln -c Debug` (expect **0 errors**) plus, where noted, a manual launch check. **Close any running `LogViewer.exe` before building** (it locks the output DLLs): `powershell.exe -NoProfile -Command "Stop-Process -Name LogViewer -Force -ErrorAction SilentlyContinue"`.
- Export output must not change: same confirmation warnings, same `.kcresort` / `.kcbundle` XML formats and content.
- Work happens on branch `feature/export-to-tools-menu`.
- Export model types live in `LogViewer/Files/ResortExportModels.cs`: `ResortExport`, `ResortBundle`, `ResortExportDevice`.

---

### Task 1: Extract `ResortExportService`

Move the export logic out of `ApiSourceControl` into a UI-free service, and update the three existing call sites in `ApiSourceControl` to use it. The export buttons stay for now — this task only relocates logic, keeping the app fully working.

**Files:**
- Create: `LogViewer/Files/ResortExportService.cs`
- Modify: `LogViewer/Controls/ApiSourceControl.cs` (call sites: `ExportResort`, `ExportAllResorts`, `OpenInKc220Tool`; delete private `BuildResortExportAsync`, `ExtractHost`, `MakeSafeFileName`)

**Interfaces:**
- Produces (consumed by Tasks 2 and 4):
  - `Task<ResortExport> ResortExportService.BuildResortExportAsync(InternalApiClient client, OrganisationConfig org, Resort resort, IProgress<double>? progress, CancellationToken token)`
  - `Task<ResortBundle> ResortExportService.BuildBundleAsync(InternalApiClient client, OrganisationConfig org, IProgress<double>? progress, CancellationToken token)`
  - `static void ResortExportService.SaveResort(ResortExport export, string path)`
  - `static void ResortExportService.SaveBundle(ResortBundle bundle, string path)`
  - `static string ResortExportService.MakeSafeFileName(string? name)`
  - `static string ResortExportService.ExtractHost(string? basePath)`

- [ ] **Step 1: Create the service file**

Create `LogViewer/Files/ResortExportService.cs`:

```csharp
using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.Config.Models;
using LogViewer.Providers.API;
using System.Xml.Serialization;

namespace LogViewer.Files
{
    /// <summary>
    /// UI-free logic for exporting resorts/organisations to the .kcresort / .kcbundle
    /// files the KC220 config tool imports. Shared by the Tools ▸ Export dialog and the
    /// "220 tool" launcher on the WebAPI tab.
    /// </summary>
    public static class ResortExportService
    {
        // Bouwt de export van één resort (instellingen + alle apparaten via objecten -> gateways).
        public static async Task<ResortExport> BuildResortExportAsync(
            InternalApiClient client, OrganisationConfig org, Resort resort,
            IProgress<double>? progress, CancellationToken token)
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
            if (resort.Id == null)
                return export;

            var objectsProvider = new ApiObjectItemProviderBuilder(client)
                .ForResort(resort.Id.Value)
                .WithSortByName()
                .Build();

            var objects = new List<ObjectItem>();
            await foreach (var obj in objectsProvider.GetData(token, null))
                objects.Add(obj);

            for (int i = 0; i < objects.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                progress?.Report((double)i / Math.Max(1, objects.Count));

                var obj = objects[i];
                if (obj.Id == null)
                    continue;

                var gatewaysProvider = new ApiGatewayProviderBuilder(client)
                    .ForObjectItem(obj.Id.Value)
                    .WithSortByName()
                    .Build();

                var gateways = new List<Gateway>();
                await foreach (var gw in gatewaysProvider.GetData(token, null))
                    gateways.Add(gw);

                foreach (var gw in gateways)
                {
                    export.Devices.Add(new ResortExportDevice
                    {
                        Name = string.IsNullOrWhiteSpace(gw.Name) ? obj.Name : gw.Name,
                        Object = obj.Name,
                        Sid = (int)(gw.Sid ?? 0),
                        DeviceId = (int)(gw.GatewayId ?? 0),
                    });
                }
            }
            return export;
        }

        // Bouwt de export van ALLE resorts van een organisatie tot één bundle.
        public static async Task<ResortBundle> BuildBundleAsync(
            InternalApiClient client, OrganisationConfig org,
            IProgress<double>? progress, CancellationToken token)
        {
            var bundle = new ResortBundle();
            if (org.OrganisationId == null)
                return bundle;

            var resortProvider = new ApiResortProviderBuilder(client)
                .ForOrganization(org.OrganisationId.Value)
                .WithSortByName()
                .Build();

            var resorts = new List<Resort>();
            await foreach (var r in resortProvider.GetData(token, null))
                resorts.Add(r);

            for (int i = 0; i < resorts.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                progress?.Report((double)i / Math.Max(1, resorts.Count));
                var export = await BuildResortExportAsync(client, org, resorts[i], null, token);
                bundle.Resorts.Add(export);
            }
            return bundle;
        }

        public static void SaveResort(ResortExport export, string path)
        {
            var serializer = new XmlSerializer(typeof(ResortExport));
            using var stream = File.Create(path);
            serializer.Serialize(stream, export);
        }

        public static void SaveBundle(ResortBundle bundle, string path)
        {
            var serializer = new XmlSerializer(typeof(ResortBundle));
            using var stream = File.Create(path);
            serializer.Serialize(stream, bundle);
        }

        public static string ExtractHost(string? basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                return "";
            try { return new Uri(basePath).Host; } catch { return ""; }
        }

        public static string MakeSafeFileName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "resort";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
```

- [ ] **Step 2: Rewrite `ExportResort` in `ApiSourceControl.cs` to use the service**

Replace the body from the `RunWithDisabledControlsAsync` block (currently ApiSourceControl.cs:330-342) so it uses the service. The full method becomes:

```csharp
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
                buttonExportResort.Text = "Export";
                buttonExportAll.Enabled = true;
            }
        }
```

- [ ] **Step 3: Rewrite `ExportAllResorts` in `ApiSourceControl.cs` to use the service**

Replace the whole method with:

```csharp
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
```

- [ ] **Step 4: Update `OpenInKc220Tool` and delete the now-duplicated private helpers**

In `OpenInKc220Tool` (ApiSourceControl.cs), change the two calls `ExtractHost(org.BasePath)` and the serialize block to use the service, and delete the private `BuildResortExportAsync`, `ExtractHost`, and `MakeSafeFileName` methods (now on the service).

Change the `OrganisationHost` line inside the `export` initializer:

```csharp
                OrganisationHost = ResortExportService.ExtractHost(org.BasePath),
```

Change the serialize block inside the `try`:

```csharp
                string temp = Path.Combine(Path.GetTempPath(), "kc220_connect_" + Guid.NewGuid().ToString("N") + ".kcresort");
                ResortExportService.SaveResort(export, temp);

                Process.Start(new ProcessStartInfo(exe, $"/connect \"{temp}\"") { UseShellExecute = false });
```

Then delete these three now-unused private methods from `ApiSourceControl.cs`: `BuildResortExportAsync` (lines ~239-293), `ExtractHost` (lines ~429-434), `MakeSafeFileName` (lines ~436-443).

Add `using LogViewer.Files;` to the top of `ApiSourceControl.cs` if not already present (it is present — `LogViewer.Files` is imported). Remove the now-unused `using System.Xml.Serialization;` only if the compiler flags it as unused (leave it if `XmlSerializer` is still referenced elsewhere in the file — after these edits it is not, so remove it).

- [ ] **Step 5: Build**

```bash
powershell.exe -NoProfile -Command "Stop-Process -Name LogViewer -Force -ErrorAction SilentlyContinue"
dotnet build LogViewer.sln -c Debug 2>&1 | tail -4
```
Expected: `0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add LogViewer/Files/ResortExportService.cs LogViewer/Controls/ApiSourceControl.cs
git commit -m "Extract ResortExportService from ApiSourceControl"
```

---

### Task 2: Create the self-contained `ExportForm`

A modal dialog with its own organisation/resort dropdowns and login, calling `ResortExportService`. Not yet wired to any menu — this task just makes it compile.

**Files:**
- Create: `LogViewer/Forms/ExportForm.cs`
- Create: `LogViewer/Forms/ExportForm.Designer.cs`

**Interfaces:**
- Consumes: `ResortExportService.*` (Task 1), `ComboBoxManager<T>`, `ProgressBarManager`, `ApiClientProvider`, `DialogClientCredentialsSource`, `ApiOrganisationProvider`, `ApiResortProviderBuilder`.
- Produces (consumed by Task 3): `public ExportForm(List<OrganisationConfig> organisationConfigs)` and `public Task LoadOrganisations()`.

- [ ] **Step 1: Create the Designer file**

Create `LogViewer/Forms/ExportForm.Designer.cs`:

```csharp
namespace LogViewer.Forms
{
    partial class ExportForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            labelOrg = new Label();
            comboBoxOrganisations = new ComboBox();
            labelResort = new Label();
            comboBoxResorts = new ComboBox();
            buttonExportResort = new Button();
            buttonExportAll = new Button();
            buttonCancel = new Button();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            //
            // labelOrg
            //
            labelOrg.AutoSize = true;
            labelOrg.Location = new Point(12, 15);
            labelOrg.Name = "labelOrg";
            labelOrg.Size = new Size(79, 15);
            labelOrg.Text = "Organisation";
            //
            // comboBoxOrganisations
            //
            comboBoxOrganisations.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxOrganisations.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOrganisations.FormattingEnabled = true;
            comboBoxOrganisations.Location = new Point(12, 33);
            comboBoxOrganisations.Name = "comboBoxOrganisations";
            comboBoxOrganisations.Size = new Size(360, 23);
            comboBoxOrganisations.TabIndex = 0;
            //
            // labelResort
            //
            labelResort.AutoSize = true;
            labelResort.Location = new Point(12, 66);
            labelResort.Name = "labelResort";
            labelResort.Size = new Size(42, 15);
            labelResort.Text = "Resort";
            //
            // comboBoxResorts
            //
            comboBoxResorts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxResorts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxResorts.FormattingEnabled = true;
            comboBoxResorts.Location = new Point(12, 84);
            comboBoxResorts.Name = "comboBoxResorts";
            comboBoxResorts.Size = new Size(360, 23);
            comboBoxResorts.TabIndex = 1;
            //
            // buttonExportResort
            //
            buttonExportResort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportResort.Location = new Point(12, 123);
            buttonExportResort.Name = "buttonExportResort";
            buttonExportResort.Size = new Size(110, 27);
            buttonExportResort.TabIndex = 2;
            buttonExportResort.Text = "Export resort";
            buttonExportResort.UseVisualStyleBackColor = true;
            //
            // buttonExportAll
            //
            buttonExportAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportAll.Location = new Point(128, 123);
            buttonExportAll.Name = "buttonExportAll";
            buttonExportAll.Size = new Size(110, 27);
            buttonExportAll.TabIndex = 3;
            buttonExportAll.Text = "Export all";
            buttonExportAll.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCancel.Location = new Point(262, 123);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(110, 27);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // progressBar1
            //
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(12, 162);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(360, 8);
            progressBar1.TabIndex = 5;
            //
            // ExportForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 182);
            Controls.Add(progressBar1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonExportAll);
            Controls.Add(buttonExportResort);
            Controls.Add(comboBoxResorts);
            Controls.Add(labelResort);
            Controls.Add(comboBoxOrganisations);
            Controls.Add(labelOrg);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Name = "ExportForm";
            Text = "Export for KC220 tool";
            ResumeLayout(false);
            PerformLayout();
        }

        private Label labelOrg;
        private ComboBox comboBoxOrganisations;
        private Label labelResort;
        private ComboBox comboBoxResorts;
        private Button buttonExportResort;
        private Button buttonExportAll;
        private Button buttonCancel;
        private ProgressBar progressBar1;
    }
}
```

- [ ] **Step 2: Create the form code-behind**

Create `LogViewer/Forms/ExportForm.cs`:

```csharp
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
```

- [ ] **Step 3: Build**

```bash
powershell.exe -NoProfile -Command "Stop-Process -Name LogViewer -Force -ErrorAction SilentlyContinue"
dotnet build LogViewer.sln -c Debug 2>&1 | tail -4
```
Expected: `0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add LogViewer/Forms/ExportForm.cs LogViewer/Forms/ExportForm.Designer.cs
git commit -m "Add self-contained ExportForm dialog"
```

---

### Task 3: Wire up the Tools menu in `Form1`

Add the `Tools ▸ Export for KC220 tool…` menu item that opens `ExportForm` modally. After this task, export works from the menu (the tab buttons still exist too — removed in Task 4).

**Files:**
- Modify: `LogViewer/Form1.cs` (`UpdateMenu`, plus a new `OpenExportForm` method and a `using`)

**Interfaces:**
- Consumes: `ExportForm(List<OrganisationConfig>)`, `ExportForm.LoadOrganisations()` (Task 2).

- [ ] **Step 1: Add the using**

At the top of `LogViewer/Form1.cs`, add:

```csharp
using LogViewer.Forms;
```

- [ ] **Step 2: Add the Tools menu item in `UpdateMenu`**

In `UpdateMenu` (Form1.cs), insert the Tools item between the File items and the Help item, so the block reads:

```csharp
            menuStrip1.AddMenuItem("File/New", (menuItem) => NewDocument());
            menuStrip1.AddMenuItem("File/Open", (menuItem) => LoadFileDialog());
            menuStrip1.AddMenuItem("File/Save", (menuItem) => SaveFileDialog());
            menuStrip1.AddMenuItem("File/Close", (menuItem) => this.Close());

            menuStrip1.AddMenuItem("Tools/Export for KC220 tool…", (menuItem) => OpenExportForm());

            menuStrip1.AddMenuItem("Help", (menuItem) => Help.ShowHelp());
```

- [ ] **Step 3: Add the `OpenExportForm` method**

Add this method to `Form1` (e.g. right after `UpdateMenu`):

```csharp
        private async void OpenExportForm()
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
```

- [ ] **Step 4: Build**

```bash
powershell.exe -NoProfile -Command "Stop-Process -Name LogViewer -Force -ErrorAction SilentlyContinue"
dotnet build LogViewer.sln -c Debug 2>&1 | tail -4
```
Expected: `0 Error(s)`.

- [ ] **Step 5: Manual launch check**

```bash
powershell.exe -NoProfile -Command "Start-Process 'C:/Workspace/logviewer/LogViewer/bin/Debug/net8.0-windows7.0/LogViewer.exe'"
```
Confirm: a **Tools** menu appears between File and Help; clicking **Export for KC220 tool…** opens the dialog with an Organisation dropdown. Close the app afterward.

- [ ] **Step 6: Commit**

```bash
git add LogViewer/Form1.cs
git commit -m "Add Tools menu opening the export dialog"
```

---

### Task 4: Remove export buttons from `ApiSourceControl` and align the 220 tool

Strip the two export buttons and their handlers from the WebAPI tab, restore the org/resort combos to full width, and align the remaining 220 tool button.

**Files:**
- Modify: `LogViewer/Controls/ApiSourceControl.cs` (constructor + delete methods)

**Interfaces:** none produced.

- [ ] **Step 1: Delete the export button fields**

In `ApiSourceControl.cs`, remove these three field declarations:

```csharp
        private readonly Button buttonExportResort;
        private readonly Button buttonExportAll;
        private bool isExporting;
```

(Keep `private readonly Button buttonOpen220;`.)

- [ ] **Step 2: Remove export button construction from the constructor**

Delete the two blocks that build `buttonExportResort` (the `comboBoxResorts.Width = 150;` block, ApiSourceControl.cs ~44-57) and `buttonExportAll` (the `comboBoxOrganisations.Width = 146;` block, ~59-72), including their comments. This also removes the `comboBoxResorts.Width` and `comboBoxOrganisations.Width` overrides so both combos keep their designer full width (230).

- [ ] **Step 3: Align the 220 tool button**

Keep the `buttonOpen220` block but make its alignment explicit so it lines up with the Fetch/Cancel/Search buttons (which end at x = 233). Replace the `comboBoxGateways.Width = 144;` block with:

```csharp
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
```

- [ ] **Step 4: Remove export methods and the ControlStateManager entries**

Delete the methods `OnExportButtonClick` and `OnExportAllButtonClick` from `ApiSourceControl.cs`. Delete `ExportResort` and `ExportAllResorts` (the versions edited in Task 1). Do NOT touch `OpenInKc220Tool`, `GetKc220ToolPath`, `LoadGatewaysForObjectItem`, `FetchLogsForGateway`, or `RunWithDisabledControlsAsync`.

The `controlStateManager` list already only references `buttonOpen220` (not the export buttons), so it needs no change — verify it does not list `buttonExportResort`/`buttonExportAll` (it does not).

- [ ] **Step 5: Build**

```bash
powershell.exe -NoProfile -Command "Stop-Process -Name LogViewer -Force -ErrorAction SilentlyContinue"
dotnet build LogViewer.sln -c Debug 2>&1 | tail -4
```
Expected: `0 Error(s)`.

- [ ] **Step 6: Manual launch check**

```bash
powershell.exe -NoProfile -Command "Start-Process 'C:/Workspace/logviewer/LogViewer/bin/Debug/net8.0-windows7.0/LogViewer.exe'"
```
Confirm on the **WebAPI** tab: no Export / Export all buttons; organisation and resort combos are full width; only the **220 tool** button remains, right-aligned next to the gateway dropdown. Tools ▸ Export still works. Close the app afterward.

- [ ] **Step 7: Commit**

```bash
git add LogViewer/Controls/ApiSourceControl.cs
git commit -m "Remove export buttons from WebAPI tab, align 220 tool"
```

---

## Verification (whole feature)

1. `dotnet build LogViewer.sln -c Debug` → 0 errors.
2. Launch the app.
3. WebAPI tab: only the 220 tool button, aligned; full-width org/resort combos.
4. Tools ▸ Export for KC220 tool… → dialog opens; select org → login prompt → resorts load; Export resort → `.kcresort`; Export all → `.kcbundle`; same warnings/messages as before.
5. WebAPI tab 220 tool still launches the KC220 config tool.
```
