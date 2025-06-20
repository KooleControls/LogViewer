using LogViewer.Providers.API;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer.Controls.Helpers
{
    public class ComboBoxManager<T>
    {
        public event EventHandler<T?>? SelectedItemChanged;
        private readonly ComboBox comboBox;

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

        public T? SelectedItem => (T?)comboBox.SelectedItem;

        public async Task Load(IApiDataProvider<T> dataProvider, CancellationToken token)
        {
            Clear();
            var newItems = dataProvider.GetData(token, Progress);
            await foreach (var item in newItems)
            {
                token.ThrowIfCancellationRequested();
                if (item != null)
                    AddItem(item);
            }
            Progress?.Report(0);
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

        public void Clear()
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(Clear);
                return;
            }

            comboBox.Items.Clear();
            comboBox.SelectedIndex = -1;
            comboBox.Text = "";  
            SelectedItemChanged?.Invoke(this, default(T));
        }
    }
}
