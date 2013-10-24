using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageRestoreSelectFile.xaml
    /// </summary>
   internal partial class pageRestoreSelectFile : Page
    {
        public MainViewModel ViewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public pageRestoreSelectFile(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            if (ViewModel.TempFile != null)
            {
                txtComment.Text = ViewModel.TempFile.File.Comment;
                txtFilePath.Text = ViewModel.TempFilePath;
            }
            else
            {
                txtComment.Text = "";
                txtFilePath.Text = "";
            }
            Refresh();
        }
       
        void ViewModel_NavigationHandler(object sender, MainViewModel.PageNavigationEventArgs e)
        {
            if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.Forward && ViewModel.TempFile != null)
            {
                ViewModel.CurrentFile = ViewModel.TempFile;
                ViewModel.CurrentFilePath = ViewModel.TempFilePath;

                e.Processed = true;
                
                var actions = new Dictionary<string, BackupProcessor.ActionSetting>();
                foreach (var key in BackupProcessor.RestoreTestActions.Keys)
                {
                    var val = BackupProcessor.RestoreTestActions[key];
                    var param = new BackupProcessor.TestActionParameter();
                    param.File = ViewModel.CurrentFile;
                    val(param);
                    actions.Add(key, new BackupProcessor.ActionSetting(param.IsAvailable, param.IsAvailable));
                }
                ViewModel.ActionSettings = actions;
                var page = new pageRestoreList(ViewModel);
                ViewModel.Navigate(page);
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetForwardButtonVisibleState)
            {
                e.Processed = true;
                if (System.IO.File.Exists(txtFilePath.Text) && ViewModel.TempFile != null && RapiComm.IsConnected)
                    e.Result = true;
                else
                    e.Result = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigationHandler += new EventHandler<MainViewModel.PageNavigationEventArgs>(ViewModel_NavigationHandler);
            ViewModel.OnTempFileLoaded += new EventHandler(ViewModel_OnTempFileLoaded);
            ViewModel.Refresh();
        }

        void Refresh()
        {
            ViewModel.Refresh();
            txtComment.Text = (ViewModel.TempFile != null) ? BackupProcessor.FromBase64(ViewModel.TempFile.File.Comment) : "";
        }
        void ViewModel_OnTempFileLoaded(object sender, EventArgs e)
        {
            Refresh();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = LocalizedResources.OpenBackup;
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ofd.Filter = LocalizedResources.WPBackupFiles + "|*.wpbackup";

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WP7 Backups";
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            ofd.InitialDirectory = folderPath;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFilePath.Text = ofd.FileName;
            }
        }

        public void SetFilePath(string path)
        {
            txtFilePath.Text = path;
        }
        private void txtFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (System.IO.File.Exists(txtFilePath.Text))
                ViewModel.LoadBackupToTemp(txtFilePath.Text);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.KillLoadBackupThread();
        }



    }
}
