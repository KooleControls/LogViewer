using FormsLib.Maths;
using FormsLib.Scope;
using LogViewer.Files;
using LogViewer.Mapping;
using System.Diagnostics;
using static FormsLib.Scope.Trace;

namespace LogViewer.Forms
{
    /// <summary>
    /// All houses of a fetched period below each other, one state row per house showing when it
    /// had communication problems ("OK", "NO SNTP", "MODBUS ERR", ...).
    /// Works on saved logs only, so changing the allowed gap never hits the web API.
    /// </summary>
    public partial class GapAnalysisForm : Form
    {
        private static readonly Color OkColor = Color.FromArgb(0x1F, 0xFF, 0x53);
        private static readonly Color ProblemColor = Color.FromArgb(0xFF, 0x50, 0x50);

        private readonly ScopeController scopeController = new();
        private List<HouseLog> houses = new();

        public GapAnalysisForm()
        {
            InitializeComponent();

            scopeController.Settings.ApplySettings(AppScopeStyles.GetDarkScope());
            // Rows are stacked downwards from the top, see AddHouseRow.
            scopeController.Settings.ZeroPosition = VerticalZeroPosition.Top;
            scopeController.Settings.GridZeroPosition = VerticalZeroPosition.Top;

            scopeView1.DataSource = scopeController;
            cursorsView1.DataSource = scopeController;
            traceView1.DataSource = scopeController;

            numericUpDownMaxGap.ValueChanged += (s, e) => ShowHouses();
            buttonOpen.Click += (s, e) => OpenSavedFolder();
        }

        public void ShowHouses(List<HouseLog> houses, string folder)
        {
            this.houses = houses;
            labelFolder.Text = folder;
            ShowHouses();
        }

        private void OpenSavedFolder()
        {
            Directory.CreateDirectory(HouseLogFolder.RootFolder);
            using var dlg = new FolderBrowserDialog
            {
                Description = "Folder with saved house logs (.cyaml)",
                InitialDirectory = HouseLogFolder.RootFolder,
                UseDescriptionForTitle = true,
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                ShowHouses(HouseLogFolder.Load(dlg.SelectedPath), dlg.SelectedPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show($"Failed to load {dlg.SelectedPath}:\r\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ShowHouses()
        {
            scopeController.Clear();
            if (houses.Count == 0)
            {
                labelSummary.Text = "No houses";
                return;
            }

            var maxGap = TimeSpan.FromHours((double)numericUpDownMaxGap.Value);
            int housesWithProblems = 0;
            for (int row = 0; row < houses.Count; row++)
            {
                if (AddHouseRow(houses[row], row, maxGap))
                    housesWithProblems++;
            }

            scopeController.Settings.SetHorizontal(
                houses.Min(h => h.Context.ScopeViewContext.StartDate),
                houses.Max(h => h.Context.ScopeViewContext.EndDate));
            scopeController.RedrawAll();

            labelSummary.Text = $"{houses.Count} houses, {housesWithProblems} with problems";
        }

        /// <summary>Returns whether the house had any problem in the period.</summary>
        private bool AddHouseRow(HouseLog house, int row, TimeSpan maxGap)
        {
            var entries = house.Context.LogCollection.Entries;
            DateTime from = house.Context.ScopeViewContext.StartDate;
            DateTime until = house.Context.ScopeViewContext.EndDate;

            var timeline = CommunicationGapAnalyzer.Analyze(entries, from, until, maxGap);
            bool hadProblems = timeline.Any(change => change.Problems != CommunicationProblems.None);

            // A state trace is drawn at y = 1 + Offset in units of (view height / (VerticalDivisions * Scale)).
            // With room for one unit per house plus a margin, row n sits at n + 1 units from the top.
            var trace = new FormsLib.Scope.Trace
            {
                Name = house.Name,
                Tag = house,
                Color = hadProblems ? ProblemColor : OkColor,
                DrawStyle = DrawStyles.State,
                DrawOption = DrawOptions.DrawNames | DrawOptions.ExtendEnd,
                ToHumanReadable = value => CommunicationGapAnalyzer.ToText((CommunicationProblems)(int)value),
                Scale = (houses.Count + 1) / (double)scopeController.Settings.VerticalDivisions,
                Offset = -(row + 2),
            };

            foreach (var change in timeline)
                trace.Points.Add(new PointD(change.Time.Ticks, (int)change.Problems));
            // Closes the last state at the end of the period instead of at the last change.
            trace.Points.Add(new PointD(until.Ticks, (int)timeline[^1].Problems));

            scopeController.Traces.Add(trace);
            return hadProblems;
        }
    }
}
