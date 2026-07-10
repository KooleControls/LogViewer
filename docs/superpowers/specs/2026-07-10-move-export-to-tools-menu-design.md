# Move resort export out of the WebAPI tab into a Tools menu

**Date:** 2026-07-10
**Status:** Approved (pending spec review)

## Problem

A colleague added a KC220 resort-export feature directly onto the WebAPI
source tab (`ApiSourceControl`). Two buttons — **Export** and **Export all** —
sit next to the organisation/resort dropdowns. This is the wrong place for it:
it clutters a tab whose job is fetching logs, and it is visually inconsistent
(three ad-hoc buttons manually positioned in code, combos shrunk to make room).

We want the export feature *out of the way* — reachable, but not on the log
source tab. The **220 tool** button (open the KC220 config tool on the selected
gateway) is genuinely tied to the WebAPI tab's gateway selection and stays.

## Goal

- Add a **Tools** menu between **File** and **Help**, with one item:
  **"Export for KC220 tool…"**, opening a dedicated modal dialog.
- The dialog is **self-contained**: its own organisation/resort selection and
  its own login. It does not depend on the WebAPI tab's state.
- Remove the two export buttons from `ApiSourceControl`.
- Keep the **220 tool** button on the WebAPI tab and align the layout now that
  its two sibling buttons are gone.
- **No behavioral change** to export output: same confirmation warnings, same
  `.kcresort` / `.kcbundle` file formats, same content.

Non-goals: no change to log fetching, no change to the export file schema, no
refactor of unrelated parts of `ApiSourceControl`.

## Approach

Chosen: **self-contained Export dialog** backed by an extracted export service.
Rejected alternative — driving the dialog from the WebAPI tab's current login/
selection — because it would leave Export greyed-out or erroring until the user
first picks an org and logs in on the WebAPI tab, coupling the two features.

All required building blocks already exist and are reused:
`ComboBoxManager<T>`, `ApiClientProvider` + `DialogClientCredentialsSource`,
`ApiOrganisationProvider`, `ApiResortProviderBuilder`, `ProgressBarManager`,
and the `ResortExport` / `ResortBundle` / `ResortExportDevice` models.

## Components

### 1. `ResortExportService` — `LogViewer/Files/ResortExportService.cs` (new)

Pure export logic, no UI. Lifted from `ApiSourceControl` so both the new dialog
and the KC220 launcher share one code path.

- `Task<ResortExport> BuildResortExportAsync(InternalApiClient client, OrganisationConfig org, Resort resort, IProgress<double>? progress, CancellationToken token)`
  — moved verbatim from `ApiSourceControl.BuildResortExportAsync` (uses
  `ApiObjectItemProviderBuilder` + `ApiGatewayProviderBuilder`).
- `Task<ResortBundle> BuildBundleAsync(InternalApiClient client, OrganisationConfig org, IProgress<double>? progress, CancellationToken token)`
  — the loop body of the current `ExportAllResorts` (iterate resorts, build each
  export, accumulate).
- `static void SaveResort(ResortExport export, string path)` /
  `static void SaveBundle(ResortBundle bundle, string path)` — the
  `XmlSerializer` writes.
- `static string MakeSafeFileName(string? name)` and
  `static string ExtractHost(string? basePath)` — moved here (shared).

### 2. `ExportForm` — `LogViewer/Forms/ExportForm.cs` (+ `ExportForm.Designer.cs`) (new)

Small modal dialog. Mirrors the org→resort loading pattern of
`ApiSourceControl`, trimmed to what export needs.

Controls:
- Organisation `ComboBox` (via `ComboBoxManager<OrganisationConfig>`).
- Resort `ComboBox` (via `ComboBoxManager<Resort>`).
- **Export resort** button — enabled when org + resort selected.
- **Export all** button — enabled when org selected.
- **Cancel** button and a `ProgressBar` (via `ProgressBarManager`).

Behavior:
- Constructor takes `List<OrganisationConfig>`; `Load` populates organisations
  via `ApiOrganisationProvider`.
- On organisation selected: create authenticated client via `ApiClientProvider`
  (`DialogClientCredentialsSource`), then load resorts via
  `ApiResortProviderBuilder`.
- Export resort / Export all: same confirmation `MessageBox` text as today, same
  `SaveFileDialog` filters (`*.kcresort` / `*.kcbundle`), then call
  `ResortExportService` and report completion `MessageBox`.
- Reuse a `RunWithDisabledControlsAsync`-style guard (disable controls, cancel
  token, error `MessageBox`) equivalent to the one in `ApiSourceControl`.

### 3. `Form1` — Tools menu

In `UpdateMenu` (`Form1.cs`), add between the File items and Help:

```csharp
menuStrip1.AddMenuItem("Tools/Export for KC220 tool…", _ => OpenExportForm());
```

`OpenExportForm()` builds the org list from
`configurationService.GetConfigAsync().Organisations.Values.ToList()` and shows
`ExportForm` modally (`ShowDialog(this)`).

### 4. `ApiSourceControl` — remove export, keep & align 220 tool

- Delete fields `buttonExportResort`, `buttonExportAll`, `isExporting`; delete
  methods `OnExportButtonClick`, `OnExportAllButtonClick`, `ExportResort`,
  `ExportAllResorts`, and the moved `BuildResortExportAsync`.
- Remove `comboBoxOrganisations.Width` / `comboBoxResorts.Width` overrides in the
  constructor so they return to their designer full width (230). These were only
  shrunk to make room for the now-deleted buttons.
- Keep `comboBoxGateways` shrunk and the **220 tool** button beside it. Make its
  right edge line up with the Fetch/Cancel/Search buttons (x-right ≈ 233) so the
  row reads as intentional.
- `OpenInKc220Tool` stays; its single-device `ResortExport` construction and its
  `ExtractHost` call now use `ResortExportService`.
- Remove `buttonExportResort`/`buttonExportAll` from the `ControlStateManager`
  list; keep `buttonOpen220`.

## Data flow

```
Form1 (Tools ▸ Export for KC220 tool…)
  └─ ExportForm(orgConfigs)
       ├─ ApiOrganisationProvider ──▶ organisation combo
       ├─ ApiClientProvider(DialogClientCredentialsSource) ──▶ InternalApiClient
       ├─ ApiResortProviderBuilder ──▶ resort combo
       └─ ResortExportService
            ├─ BuildResortExportAsync / BuildBundleAsync
            └─ SaveResort / SaveBundle  ──▶ .kcresort / .kcbundle

ApiSourceControl (WebAPI tab)
  └─ 220 tool button ──▶ OpenInKc220Tool ──▶ ResortExportService (single device)
```

## Error handling

- Unchanged from today: `RunWithDisabledControlsAsync` swallows
  `OperationCanceledException`, shows an error `MessageBox` for other exceptions,
  and always resets progress + re-enables controls.
- Export with no org/resort selected: same guard `MessageBox` as today
  ("Select an organisation and resort first." / "Select an organisation first.").

## Testing / verification

No automated test suite exists in this repo. Verification is manual:
1. `dotnet build LogViewer.sln -c Debug` — clean (0 errors).
2. Launch; confirm the WebAPI tab shows only the **220 tool** button, aligned,
   with full-width organisation/resort combos.
3. Confirm **Tools ▸ Export for KC220 tool…** opens the dialog; selecting an org
   prompts login and loads resorts; Export resort / Export all produce the same
   files as before.
4. Confirm **220 tool** still launches the KC220 config tool.
```
