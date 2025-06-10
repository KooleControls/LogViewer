using LogViewer.Providers.API;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer.Controls.Helpers
{
    public class ComboBoxManager<T>
    {
        public event EventHandler<T>? SelectedItemChanged;
        private readonly ComboBox comboBox;
        private CancellationTokenSource? cancellationTokenSource;

        public ComboBoxManager(ComboBox comboBox)
        {
            this.comboBox = comboBox;
            this.comboBox.SelectedIndexChanged += (s, e) => OnSelectedItemChanged();
        }

        public IProgress<double>? Progress { get; set; }
        public string DisplayMember
        {
            get => comboBox.DisplayMember;
            set => comboBox.DisplayMember = value;
        }

        public void Cancel() => cancellationTokenSource?.Cancel();

        public T SelectedItem => (T)comboBox.SelectedItem;

        public async Task Load(IApiDataProvider<T> dataProvider)
        {
            try
            {
                ClearItems();
                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                var newItems = dataProvider.GetData(cancellationToken, Progress);
                await foreach (var item in newItems)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (item != null)
                        AddItem(item);
                }
            }
            //catch (Exception exception)
            //{
            //    // Handle exceptions as needed
            //}
            finally
            {
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                Progress?.Report(0);
            }
        }

        public int Count => comboBox.Items.Count;


        public void SelectIndex(int i)
        {
            if (i >= 0 && i < comboBox.Items.Count)
                comboBox.SelectedIndex = i;
        }

        private void OnSelectedItemChanged()
        {
            if (comboBox.SelectedItem is T item)
            {
                if (comboBox.InvokeRequired)
                {
                    comboBox.Invoke(new Action(() => SelectedItemChanged?.Invoke(this, item)));
                }
                else
                {
                    SelectedItemChanged?.Invoke(this, item);
                }
            }
        }

        private void AddItem(object item)
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(AddItem, item);
                return;
            }
            
            comboBox.Items.Add(item);
            
        }

        private void ClearItems()
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(ClearItems);
                return;
            }

            comboBox.Items.Clear();
            comboBox.SelectedIndex = -1; // Ensure no item is selected
            comboBox.Text = "";  
        }
    }
}
