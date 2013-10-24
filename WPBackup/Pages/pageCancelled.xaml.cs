using System;
using System.Windows;
using System.Windows.Controls;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageCancelled.xaml
    /// </summary>
   internal partial class pageCancelled : Page
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

        public pageCancelled(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private bool _isBackupMode = false;
        public bool IsBackupMode
        {
            get
            {
                return _isBackupMode;
            }
            set
            {
                _isBackupMode = value;
                if (value)
                    Comment.Text = LocalizedResources.BackupCancelled;
                else
                    Comment.Text = LocalizedResources.RestoreCancelled;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigationHandler += new EventHandler<MainViewModel.PageNavigationEventArgs>(ViewModel_NavigationHandler);
            ViewModel.Refresh();
        }

        void ViewModel_NavigationHandler(object sender, MainViewModel.PageNavigationEventArgs e)
        {
            if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.Back)
            {
                e.Processed = true;

            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetBackButtonVisibleState)
            {
                e.Processed = true;
                e.Result = false;
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.Forward)
            {
                e.Processed = true;
                ViewModel.Navigate(new pageWelcome(ViewModel));
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetForwardButtonVisibleState)
            {
                e.Processed = true;
                e.Result = true;
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetForwardButtonText)
            {
                e.Processed = true;
                e.Result = LocalizedResources.MainMenu;
            }
        }

    }
}
