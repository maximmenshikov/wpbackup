using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageRestoration.xaml
    /// </summary>
   internal partial class pageRestoration : Page
    {
     private Thread _myThread = null;
        public Thread MyThread
        {
            get
            {
                return _myThread;
            }
            set
            {
                _myThread = value;
            }
        }

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


        private object _cancellationPendingLock = new object();
        public object CancellationPendingLock
        {
            get
            {
                return _cancellationPendingLock;
            }
        }

        private bool _cancellationPending = false;
        /// <summary>
        /// Is backup thread cancellation pending?
        /// </summary>
        /// <remarks>
        /// Use "lock (CancellationPendingLock)"!
        /// </remarks>
        public bool CancellationPending
        {
            get
            {
                return _cancellationPending;
            }
            set
            {
                _cancellationPending = value;
            }
        }

        private void RestoreThread(object p)
        {
            var vm = p as MainViewModel;
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                vm.IsBusy = true;
            }));
            var proc = new BackupProcessor();
            var param = new BackupProcessor.BackupActionParameter();
            param.OutputFilePath = vm.OutputFilePath;
            param.DeviceTempFolderPath = BackupProcessor.DeviceTempFolderPath;

            param.TempFolderPath = BackupProcessor.TempFolderPath;
            param.SourceFilePath = vm.CurrentFilePath;
            param.SourceFolderPath = BackupProcessor.SourceFolderPath;

            param.RegistryStore = new RegistryStore();
            proc.ActionSettings = vm.ActionSettings;
            Action<BackupProcessor.CancellationStateCheck> cancellationCheck = new Action<BackupProcessor.CancellationStateCheck>(delegate(BackupProcessor.CancellationStateCheck check)
            {
                lock (CancellationPendingLock)
                {
                    check.isCancelled = CancellationPending;
                }
            });

            param.CheckCancellationState = cancellationCheck;
            int count = 0, currentAction = 0;
            for (int i = 0; i < BackupProcessor.RestoreActions.Count; ++i)
            {
                var action = BackupProcessor.RestoreActions[i];
                if (String.IsNullOrEmpty(action.SettingName) || (proc.ActionSettings.ContainsKey(action.SettingName) && proc.ActionSettings[action.SettingName].IsEnabled))
                {
                    count++;
                }
            }
            bool cancelled = false;
            string errorText = null;
            for (int i = 0; i < BackupProcessor.RestoreActions.Count; ++i)
            {
                lock (CancellationPendingLock)
                {
                    if (CancellationPending)
                    {
                        cancelled = true;
                        break;
                    }
                }
               
                var action = BackupProcessor.RestoreActions[i];
                if (String.IsNullOrEmpty(action.SettingName) || (proc.ActionSettings.ContainsKey(action.SettingName) && proc.ActionSettings[action.SettingName].IsEnabled))
                {
                    param.IsCancelledByAction = false;
                    param.Error = null;
                    if (proc.ActionSettings.ContainsKey(action.SettingName))
                        param.Setting = proc.ActionSettings[action.SettingName];
                    else
                        param.Setting = null;
                    this.Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        vm.ProgressText = action.Name;
                        double val = 100 / count * currentAction;
                        val = Math.Floor(val);
                        vm.ProgressValue = (int)val;
                    }));
                    try
                    {
                        action.Execute(param);
                        if (param.IsCancelledByAction)
                        {
                            cancelled = true;
                            break;
                        }
                        if (param.Error != null)
                        {
                            errorText = param.Error;
                            break;
                        }
                    }
                    catch (OpenNETCF.Desktop.Communication.RAPIException ex)
                    {
                        errorText = ex.ToString();
                        break;
                    }
                    currentAction++;
                }
                
            }
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                vm.ProgressText = LocalizedResources.Finished;
                vm.ProgressValue = 100;
                if (errorText != null)
                {
                    var pg = new pageError(ViewModel);
                    pg.SetErrorDescText(errorText);
                    ViewModel.Navigate(pg);
                }
                else
                {
                    if (!cancelled)
                    {
                        var pg = new pageSuccess(ViewModel);
                        pg.IsBackupMode = false;
                        ViewModel.Navigate(pg);
                    }
                    else
                    {
                        var pg = new pageCancelled(ViewModel);
                        pg.IsBackupMode = false;
                        ViewModel.Navigate(pg);
                    }
                }
                vm.IsBusy = false;
            }));
        }

        void ViewModel_NavigationHandler(object sender, MainViewModel.PageNavigationEventArgs e)
        {
            e.Processed = true;
            switch (e.Type)
            {
                case MainViewModel.PageNavigationEventArgs.EventType.GetForwardButtonVisibleState:
                    e.Result = false;
                    break;
                case MainViewModel.PageNavigationEventArgs.EventType.Back:
                    if (MessageBox.Show(LocalizedResources.CancelRestoreText, "WPBackup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        lock (CancellationPendingLock)
                        {
                            CancellationPending = true;
                        }
                        ViewModel.IsOperationCancelled = true;
                    }
                    break;
                case MainViewModel.PageNavigationEventArgs.EventType.GetBackButtonVisibleState:
                    e.Result = true;
                    break;
                case MainViewModel.PageNavigationEventArgs.EventType.GetBackButtonText:
                    e.Result = LocalizedResources.Cancel;
                    break;
                default:
                    e.Processed = false;
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigationHandler += new EventHandler<MainViewModel.PageNavigationEventArgs>(ViewModel_NavigationHandler);
            ViewModel.Refresh();
        }


        public pageRestoration(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            MyThread = new Thread(RestoreThread);
            MyThread.Start(ViewModel);
        }
    }
}
