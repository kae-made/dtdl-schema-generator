using Kae.Utility.Logging;
using Kae.Utility.Logging.WPF;
using Kae.XTUML.Tools.Generator.DTDL;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kae.XTUML.Tools.WpfAppDTDLGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly string version = "1.0.0";

        Logger tbLogger = null;
        ObservableCollection<TreeViewData> generatedViewerItems = new ObservableCollection<TreeViewData>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbLogger = WPFLogger.CreateLogger(this.tbLog);
            tvGenFolder.ItemsSource = generatedViewerItems;
        }

        private void CheckStatus()
        {
            bool result = true;
            if (string.IsNullOrEmpty(tbMetaModel.Text))
            {
                result = false;
            }
            if (string.IsNullOrEmpty(tbBaseDataType.Text))
            {
                result = false;
            }
            if (string.IsNullOrEmpty(tbDomainModel.Text))
            {
                result = false;
            }
            if (string.IsNullOrEmpty(tbDTDLNamespace.Text))
            {
                result = false;
            }
            if (string.IsNullOrEmpty(tbDTDLVersion.Text))
            {
                result = false;
            }
            if (string.IsNullOrEmpty(tbGenFolder.Text))
            {
                result = false;
            }
            if (result)
            {
                buttonGenerate.IsEnabled = true;
                buttonSaveConfig.IsEnabled = true;
            }
            else
            {
                buttonGenerate.IsEnabled = false;
            }
        }
        private TreeViewData CreateDirectoryTreeViewItem(DirectoryInfo directoryInfo)
        {
            var children = new List<TreeViewData>();
            var tvData = new TreeViewData { Name = directoryInfo.Name, Children = children };
            foreach (var child in directoryInfo.GetDirectories())
            {
                var childTVData = CreateDirectoryTreeViewItem(child);
                children.Add(childTVData);
            }
            foreach (var child in directoryInfo.GetFiles())
            {
                var childTVData = new TreeViewData() { Name = child.Name, FullPath = child.FullName };
                children.Add(childTVData);
            }
            return tvData;
        }

        private void RefleshGeneratedView()
        {
            Dispatcher.Invoke(() =>
            {
                generatedViewerItems.Clear();
                var di = new DirectoryInfo(tbGenFolder.Text);
                foreach (var child in di.GetDirectories())
                {
                    generatedViewerItems.Add(CreateDirectoryTreeViewItem(child));
                }
                foreach (var child in di.GetFiles())
                {
                    generatedViewerItems.Add(new TreeViewData() { Name = child.Name });
                }
            });
        }

        private void buttonMetaModel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "SQL File|*.sql";
            if (dialog.ShowDialog() == true)
            {
                tbMetaModel.Text = dialog.FileName;
                CheckStatus();
            }
        }

        private void buttonBaseDataType_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "XTUML File|*.xtuml";
            if (dialog.ShowDialog() == true)
            {
                tbBaseDataType.Text = dialog.FileName;
                CheckStatus();
            }
        }

        private void buttonDomainModel_Click(object sender, RoutedEventArgs e)
        {
            if (cbIsFolder.IsChecked == true)
            {
                var folderDialog = new CommonOpenFileDialog();
                folderDialog.IsFolderPicker = true;
                if (folderDialog.ShowDialog()== CommonFileDialogResult.Ok)
                {
                    tbDomainModel.Text = folderDialog.FileName;
                }
            }
            else
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "SQL File|*.sql";
                if (dialog.ShowDialog() == true)
                {
                    tbDomainModel.Text = dialog.FileName;
                }
            }
            CheckStatus();
        }

        private void buttonGenerate_Click(object sender, RoutedEventArgs e)
        {
            var generator = new DTDLGenerator(tbLogger, version);
            var contextParams = generator.GetContext();
            contextParams.SetOptionValue(DTDLGenerator.CPKeyOOAofOOAModelFilePath, (tbMetaModel.Text, !File.Exists(tbMetaModel.Text)));
            contextParams.SetOptionValue(DTDLGenerator.CPKeyBaseDataTypeDefFilePaht, (tbBaseDataType.Text, !File.Exists(tbBaseDataType.Text)));
            contextParams.SetOptionValue(DTDLGenerator.CPKeyDomainModelFilePath, (tbDomainModel.Text, !File.Exists(tbDomainModel.Text)));
            contextParams.SetOptionValue(DTDLGenerator.CPKeyDTDLNameSpace, tbDTDLNamespace.Text);
            contextParams.SetOptionValue(DTDLGenerator.CPKeyDTDLModelVersion, tbDTDLVersion.Text);
            contextParams.SetOptionValue(DTDLGenerator.CPKeyUseKeyLetterAsFileName, cbUseKeyLetter.IsChecked);
            contextParams.SetOptionValue(DTDLGenerator.CPKeySuperSubRelationship, cbRelationshipDef.IsChecked==false);

            var task = new Task(() =>
            {
                generator.ResolveContext();
                generator.LoadMetaModel();
                generator.LoadDomainModels();
                generator.Generate();

                RefleshGeneratedView();
            });
            task.Start();
        }

        private void buttonGenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog()== CommonFileDialogResult.Ok)
            {
                tbGenFolder.Text= dialog.FileName;
            }
        }

        private void buttonLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "JSON File|*.json";
            if (dialog.ShowDialog()== true)
            {
                tbConfig.Text = dialog.FileName;
            }
            if (!string.IsNullOrEmpty(tbConfig.Text))
            {
                using (var reader = new StreamReader(tbConfig.Text))
                {
                    string content = reader.ReadToEnd();
                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratorConfig>(content);
                    tbMetaModel.Text = config.MetaModel;
                    tbBaseDataType.Text = config.BaseDataType;
                    tbDomainModel.Text = config.DomainModel;
                    tbDTDLNamespace.Text = config.DTDLNamespace;
                    tbDTDLVersion.Text = config.DTDLVersion;
                    cbUseKeyLetter.IsChecked = config.UseKeyLetter;
                    cbRelationshipDef.IsChecked = config.SSRelAsExtends;
                }
                buttonGenerate.IsEnabled = true;
            }
        }

        private void buttonSaveConfig_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class TreeViewData
    {
        public string Name { get; set; }
        public IEnumerable<TreeViewData> Children { get; set; }
        public string FullPath { get; set; }
    }

    public class GeneratorConfig
    {
        public string MetaModel { get; set; }
        public string BaseDataType { get; set; }
        public string DomainModel { get; set; }
        public string GenFolder { get; set; }
        public string DTDLNamespace { get; set; }
        public string DTDLVersion { get; set; }
        public bool UseKeyLetter { get; set; }
        public bool SSRelAsExtends { get; set; }
    }

}
