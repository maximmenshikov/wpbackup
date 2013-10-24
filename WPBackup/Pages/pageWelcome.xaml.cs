using System.Windows;
using System.Windows.Controls;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageWelcome.xaml
    /// </summary>
    internal partial class pageWelcome : Page
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

        public pageWelcome(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void CleanupViewModel()
        {
            ViewModel.ActionSettings = null;
            ViewModel.CurrentFile = null;
            ViewModel.CurrentFilePath = null;
            ViewModel.OutputFilePath = null;
            ViewModel.TempFile = null;
            ViewModel.TempFilePath = null;
            ViewModel.Comment = null;
        }
        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.BackupMode == false)
            {
                CleanupViewModel();
            }
            ViewModel.BackupMode = true;
            ViewModel.Navigate(new pageBackupList(ViewModel));
            ViewModel.IsOperationCancelled = false;
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.BackupMode == true)
            {
                CleanupViewModel();
            }
            ViewModel.BackupMode = false;
            ViewModel.Navigate(new pageRestoreSelectFile(ViewModel));
            ViewModel.IsOperationCancelled = false;
        }
    }
}
