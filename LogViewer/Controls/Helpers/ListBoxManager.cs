using KCObjectsStandard.Device.KC.Gateway;
using LogViewer.Providers.API;

namespace LogViewer.Controls.Helpers
{
    public class ListBoxManager<T>
    {
        public event EventHandler<T>? SelectedItemChanged;
        private readonly ListBox listBox;
        private CancellationTokenSource? cancellationTokenSource;

        public ListBoxManager(ListBox listBox)
        {
            this.listBox = listBox;
            this.listBox.SelectedIndexChanged += (s, e) => OnSelectedItemChanged();
        }
        public IProgress<double>? Progress { get; set; }
        public string DisplayMember { get => listBox.DisplayMember; set => listBox.DisplayMember = value; }
        public void Cancel() => cancellationTokenSource?.Cancel();

        public async Task Load(IApiDataProvider<T> dataProvider)
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                var newItems = dataProvider.GetData(cancellationToken, Progress);
                ClearItems();
                await foreach (var item in newItems)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if(item != null)
                        AddItem(item);
                }
            }
            //catch (Exception exception)
            //{
            //}
            finally
            {
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                Progress?.Report(0);
            }
        }
        private void OnSelectedItemChanged()
        {
            if (listBox.SelectedItem is T item)
            {
                if (listBox.InvokeRequired)
                {
                    listBox.Invoke(new Action(() => SelectedItemChanged?.Invoke(this, item)));
                }
                else
                {
                    SelectedItemChanged?.Invoke(this, item);
                }
            }
        }

        private void AddItem(object item)
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => listBox.Items.Add(item)));
            }
            else
            {
                listBox.Items.Add(item);
            }
        }

        private void ClearItems()
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => listBox.Items.Clear()));
            }
            else
            {
                listBox.Items.Clear();
            }
        }
    }

}
