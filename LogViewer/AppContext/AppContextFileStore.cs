using LogViewer.Config.Models;
using LogViewer.Serializers.Csv;
using LogViewer.Serializers.Yaml;

namespace LogViewer.AppContext
{
    public static class AppContextFileStore
    {

        public static void SaveLogDialog(LogViewerContext save)
        {
            try
            {
                // Create and configure SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "YAML files (*.yaml)|*.yaml|YAML compressed files (*.cyaml)|*.cyaml|All files (*.*)|*.*",
                    DefaultExt = "yaml",
                    AddExtension = true
                };

                // Show the dialog and check if the user selected a file
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                // Determine the file extension to decide on compression
                string fileExtension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                // Save the object to the selected file
                switch (fileExtension)
                {
                    case ".yaml":
                        YamlSerializer.SaveYaml(new FileInfo(saveFileDialog.FileName), save);
                        break;
                    case ".cyaml":
                        YamlSerializer.SaveYamlZip(new FileInfo(saveFileDialog.FileName), save);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public static bool LoadLogDialog(LogViewerContext save)
        {
            try
            {

                // Create and configure SaveFileDialog
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*|YAML files (*.yaml)|*.yaml|YAML compressed files (*.cyaml)|*.cyaml|220 tool export (*.csv)|*.csv",
                    DefaultExt = "yaml",
                    AddExtension = true
                };

                // Show the dialog and check if the user selected a file
                if (dialog.ShowDialog() != DialogResult.OK)
                    return false;

                // Load file
                string fileExtension = Path.GetExtension(dialog.FileName).ToLower();
                switch (fileExtension)
                {
                    case ".yaml":
                        if (YamlSerializer.LoadYaml(new FileInfo(dialog.FileName), out LogViewerContext yamlContext))
                        {
                            save.LogCollection = yamlContext.LogCollection;
                            save.ScopeViewContext = yamlContext.ScopeViewContext;
                            return true;
                        }
                        break;
                    case ".cyaml":
                        if (YamlSerializer.LoadYamlZip(new FileInfo(dialog.FileName), out LogViewerContext cyamlContext))
                        {
                            save.LogCollection = cyamlContext.LogCollection;
                            save.ScopeViewContext = cyamlContext.ScopeViewContext;
                            return true;
                        }
                        break;
                    case ".csv":
                        if (CsvSerializer.LoadCsv(new FileInfo(dialog.FileName), out LogViewerContext csvContext))
                        {
                            save.LogCollection = csvContext.LogCollection;
                            save.ScopeViewContext = csvContext.ScopeViewContext;
                            return true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            return false;
        }
    }
}




