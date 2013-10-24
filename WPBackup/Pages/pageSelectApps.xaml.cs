using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageSelectApps.xaml
    /// </summary>
   internal partial class pageSelectApps : Page
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

        private bool _isWorking = false;
        public bool IsWorking
        {
            get
            {
                return _isWorking;
            }
            set
            {
                _isWorking = value;
                progressBar1.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                ViewModel.Refresh();
            }
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
                {
                    lblSubHeader.Text = LocalizedResources.SelectAppsBackup;
                }
                else
                {
                    lblSubHeader.Text = LocalizedResources.SelectAppsRestore;
                }
            }
        }
        public pageSelectApps(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            progressBar1.Visibility = Visibility.Collapsed;
            
        }

        private class LoadAppsIntoViewParam
        {
            public string srcPath;
            public Action<List<ApplicationInfo>> action;
            public ApplicationInfoParser parser;
            public MainViewModel ViewModel;
        }

        private Thread _loadAppsThread = null;


        private void LoadAppsIntoViewThread(object p)
        {
            var param = p as LoadAppsIntoViewParam;
            var parser = param.parser;
            //DebugShow(0);
            if (IsBackupMode)
            {
                BackupProcessor.GetApplicationInfo(param.srcPath);
            }
            //DebugShow(1);
            parser.LoadFromFile(param.srcPath + "\\ApplicationInfo.txt", IsBackupMode ? null : param.ViewModel);
            //DebugShow(2);
            parser.Applications.Sort(new ApplicationInfoSorter());
            //DebugShow(3);
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                
                if (param.action != null)
                    param.action(parser.Applications);
                listApplications.ItemsSource = parser.Applications;
                IsWorking = false;
            }));
            //DebugShow(4);
        }

        public void LoadAppsIntoView(string srcPath, Action<List<ApplicationInfo>> action = null)
        {
            IsWorking = true;
            var parser = new ApplicationInfoParser();
            var param = new LoadAppsIntoViewParam();
            param.srcPath = srcPath;
            param.action = action;
            param.parser = parser;
            param.ViewModel = ViewModel;
            _loadAppsThread = new Thread(LoadAppsIntoViewThread);
            _loadAppsThread.Start(param);
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
                
                if (IsBackupMode)
                    ViewModel.Navigate(new pageBackupList(ViewModel));
                else
                    ViewModel.Navigate(new pageRestoreList(ViewModel));
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.Forward)
            {
                e.Processed = true;
                if (ViewModel.ActionSettings != null)
                {
                    if (ViewModel.ActionSettings.ContainsKey("Applications"))
                        ViewModel.ActionSettings["Applications"].UserParam = listApplications.ItemsSource as List<ApplicationInfo>;
                }
                if (IsBackupMode)
                    ViewModel.Navigate(new pageBackupComment(ViewModel));
                else
                    ViewModel.Navigate(new pageRestoration(ViewModel));
                
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetBackButtonVisibleState)
            {
                e.Processed = true;
                e.Result = true;
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetForwardButtonVisibleState)
            {
                e.Processed = true;
                e.Result = !IsWorking;
            }
        }
        private static void DebugShow(string test)
        {
            /*
            var sw = new System.IO.StreamWriter(System.Windows.Forms.Application.StartupPath + "\\log.txt",
                true);
            sw.WriteLine(test);
            sw.Close();
             * */
        }
        public static pageSelectApps GetBackupList(MainViewModel ViewModel)
        {
            var page = new pageSelectApps(ViewModel);
            page.IsBackupMode = true;
            page.LoadAppsIntoView(BackupProcessor.AppInfoRepositoryPath, new Action<List<ApplicationInfo>>(delegate(List<ApplicationInfo> list)
            {
                foreach (var info in list)
                {
                    try
                    {
                        DebugShow("---app: " + info.Title + " " + info.Guid);

                        /* we must copy only External (sideloaded) apps as we won't be able to restore all the rest */
                        info.IsDistributivePresent = true;
                        info.IsDataPresent = info.RealIsDataPresent();

                        info.IsDataChecked = true;
                        info.IsDistributiveChecked = true;


                        string path = BackupProcessor.AppInfoRepositoryPath + "\\Icons\\" + info.Guid;
                        if (System.IO.File.Exists(path + ".png"))
                            path += ".png";
                        else if (System.IO.File.Exists(path + ".gif"))
                            path += ".gif";
                        else if (System.IO.File.Exists(path + ".jpg"))
                            path += ".jpg";
                        else if (System.IO.File.Exists(path + ".jpeg"))
                            path += ".jpeg";
                        else if (System.IO.File.Exists(path + ".bmp"))
                            path += ".bmp";

                        if (System.IO.File.Exists(path))
                        {
                            DebugShow("Load Image " + path);

                            info.Icon = BitmapUtils.LoadImage(path);
                        }
                        else
                        {
                            info.Icon = null;
                            DebugShow("No Image");
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }));
            return page;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_loadAppsThread != null)
            {
                if (_loadAppsThread.IsAlive)
                {
                    _loadAppsThread.Abort();
                }
            }
        }
    }
}
