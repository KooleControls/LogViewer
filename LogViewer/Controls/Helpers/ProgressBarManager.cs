namespace LogViewer.Controls.Helpers
{
    public class ProgressBarManager
    {
        private readonly ProgressBar progressBar;
        public Progress<double> Progress { get; } = new Progress<double>();

        public ProgressBarManager(ProgressBar progressBar)
        {
            this.progressBar = progressBar;
            Progress.ProgressChanged += Progress_ProgressChanged;
        }

        private void Progress_ProgressChanged(object? sender, double e)
        {
            if (e >= 0 && e <= 1)
                progressBar.Value = (int)(e * 100);
        }

        public void Reset()
        {
            progressBar.Value = 0;
        }
    }
}
