namespace LogViewer.Controls.Helpers
{
    public class ControlStateManager
    {
        private readonly Control[] _controls;

        public ControlStateManager(params Control[] controls)
        {
            _controls = controls;
        }

        public void DisableAll() => SetEnabled(false);
        public void EnableAll() => SetEnabled(true);

        private void SetEnabled(bool enabled)
        {
            foreach (var ctrl in _controls)
                ctrl.Enabled = enabled;
        }
    }
}
