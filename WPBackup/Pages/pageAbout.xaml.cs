using System;
using System.Windows;
using System.Windows.Controls;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageAbout.xaml
    /// </summary>
   internal partial class pageAbout : Page
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

        public pageAbout(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
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
                var pg = new pageWelcome(ViewModel);
                ViewModel.Navigate(pg);
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetBackButtonVisibleState)
            {
                e.Processed = true;
                e.Result = true;
            }
        }

    }
}
