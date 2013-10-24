using System;
using System.Windows;
using System.Windows.Controls;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageError.xaml
    /// </summary>
   internal partial class pageError : Page
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

        public pageError(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public void SetErrorDescText(string desc)
        {
            ErrorDescBox.Text = desc;
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
