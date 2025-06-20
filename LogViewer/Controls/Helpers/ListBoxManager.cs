using KCObjectsStandard.Device.KC.Gateway;
using LogViewer.Providers.API;
using System.Threading;

namespace LogViewer.Controls.Helpers
{
    public class ListBoxManager<T>
    {
        public event EventHandler<T?>? SelectedItemChanged;
        private readonly ListBox listBox;

        public ListBoxManager(ListBox listBox)
        {
            this.listBox = listBox;
            this.listBox.SelectedIndexChanged += (s, e) => OnSelectedItemChanged();
        }
        public IProgress<double>? Progress { get; set; }
        public string DisplayMember { get => listBox.DisplayMember; set => listBox.DisplayMember = value; }

        public async Task Load(IApiDataProvider<T> dataProvider, CancellationToken token)
        {
            var newItems = dataProvider.GetData(token, Progress);
            Clear();
            await foreach (var item in newItems)
            {
                token.ThrowIfCancellationRequested();
                if (item != null)
                    AddItem(item);
            }
            Progress?.Report(0);

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

        public void Clear()
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(Clear);
                return;
            }

            listBox.Items.Clear();
            SelectedItemChanged?.Invoke(this, default(T));
        }
    }

}
