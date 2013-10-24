using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Ionic.Zip;

namespace WPBackup
{
    internal class MainViewModel : BaseViewModel
    {

        public MainViewModel()
        {
            RapiComm.Ping();

            RapiComm.OnConnected+=new EventHandler(RapiComm_OnConnected);
            RapiComm.OnDisconnected +=new EventHandler(RapiComm_OnDisconnected);
            RapiComm.OnConnectingStateChanged += new EventHandler(RapiComm_OnConnectingStateChanged);
            //RapiComm.On
        }

        void RapiComm_OnConnectingStateChanged(object sender, EventArgs e)
        {
            IsConnecting = RapiComm.IsConnecting;
        }


        public event EventHandler OnModeChanged;

        private bool _isConnected = false;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnChange("IsConnected");
                    OnChange("IsConnectedText");
                    OnChange("IsConnectedVisibility");
                    OnChange("IsConnectedReverseVisibility");
                    OnChange("ShowConnectButton");
                    this.Refresh();
                }
            }
        }

        public string IsConnectedText
        {
            get
            {
                if (IsConnected)
                    return LocalizedResources.Connected;
                else
                    return LocalizedResources.NotConnected;
            }
        }

        public Visibility IsConnectedVisibility
        {
            get
            {
                return IsConnected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility IsConnectedReverseVisibility
        {
            get
            {
                return !IsConnected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool _isConnecting;
        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }
            set
            {
                _isConnecting = value;
                OnChange("IsConnecting");
                OnChange("IsConnectingVisibility");
                OnChange("ShowConnectButton");
            }
        }

        public Visibility IsConnectingVisibility
        {
            get
            {
                return _isConnecting ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ShowConnectButton
        {
            get
            {
                return (IsConnected | IsConnecting) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private void RapiComm_OnConnected(object sender, EventArgs e)
        {
            IsConnected = true;
        }

        private void RapiComm_OnDisconnected(object sender, EventArgs e)
        {
            IsConnected = false;
        }

        #region "pageBackupFile"

        private bool _isLoadingBackup = false;
        public bool IsLoadingBackup
        {
            get
            {
                return _isLoadingBackup;
            }
            set
            {
                _isLoadingBackup = value;
                OnChange("IsLoadingBackup");
                OnChange("IsLoadingBackupVisibility");
            }
        }

        public Visibility IsLoadingBackupVisibility
        {
            get
            {
                return _isLoadingBackup ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility IsLoadingBackupReverseVisibility
        {
            get
            {
                return !_isLoadingBackup ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string _tempFilePath = null;
        public string TempFilePath
        {
            get
            {
                return _tempFilePath;
            }
            set
            {
                _tempFilePath = value;
                OnChange("TempFilePath");
            }
        }
        private FlatBackup _tempFile = null;
        public FlatBackup TempFile
        {
            get
            {
                return _tempFile;
            }
            set
            {
                _tempFile = value;
                OnChange("TempFile");
            }
        }

        #endregion

        private FlatBackup _currentFile = null;
        public FlatBackup CurrentFile
        {
            get
            {
                return _currentFile;
            }
            set
            {
                _currentFile = value;
                OnChange("CurrentFile");
            }
        }

        private string _currentFilePath = null;
        public string CurrentFilePath
        {
            get
            {
                return _currentFilePath;
            }
            set
            {
                _currentFilePath = value;
                OnChange("CurrentFilePath");
            }
        }

        public event EventHandler OnTempFileLoaded;
        private Object _loadBackupSync = new Object();
        private Thread _loadBackupThread = null;

        private void LoadBackupThread(object param)
        {
            lock (_loadBackupSync)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    IsLoadingBackup = true;
                }));
                _tempFile = null;
                _tempFilePath = null;
                //Thread.Sleep(5000);
                try
                {

                    string fileName = param as string;
                    if (ZipFile.IsZipFile(fileName))
                    {
                        //ZipFile file = new ZipFile(fileName);
                        FlatBackup file = new FlatBackup();
                        file.Read(fileName);
                        _tempFile = file;
                        _tempFilePath = fileName;
                    }
                }
                catch (Exception ex)
                {
                    _tempFile = null;
                    _tempFilePath = null;
                }
                
                Application.Current.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    if (OnTempFileLoaded != null)
                        OnTempFileLoaded(this, new EventArgs());
                    IsLoadingBackup = false;
                }));
            }
        }

        public void LoadBackupToTemp(string fileName)
        {
            _loadBackupThread = new Thread(LoadBackupThread);
            _loadBackupThread.Start(fileName);
        }

        public void KillLoadBackupThread()
        {
            if (_loadBackupThread != null)
            {
                if (_loadBackupThread.IsAlive)
                    _loadBackupThread.Abort();
            }
        }
        #region "Navigation"
        public class NavigateEventArgs : EventArgs
        {
            public Page Page;

            public NavigateEventArgs(Page page)
            {
                Page = page;
            }
        }
        public event EventHandler<NavigateEventArgs> OnNavigate;

        public class PageDesc
        {
            public bool ShowBack
            {
                get
                {
                    if (_previousPage == null)
                        return false;
                    return true;
                }
            }

            public bool ShowForward
            {
                get
                {
                    if (_nextPage == null)
                        return false;
                    return true;
                }
            }

            private string _previousPage = null;
            public string PreviousPage
            {
                get
                {
                    return _previousPage;
                }
            }

            private string _nextPage = null;
            public string NextPage
            {
                get
                {
                    return _nextPage;
                }
            }

            private string _pageName = "";
            public string PageName
            {
                get
                {
                    return _pageName;
                }
            }

            public PageDesc(string pageName, string previousPage, string nextPage)
            {
                _pageName = pageName;
                _previousPage = previousPage;
                _nextPage = nextPage;
            }
        }

        public class PageNavigationEventArgs : EventArgs
        {
            public enum EventType
            {
                Back = 0,
                Forward = 1,
                GetBackButtonText = 2,
                GetForwardButtonText = 3,
                GetBackButtonVisibleState = 4,
                GetForwardButtonVisibleState = 5
            }
            public EventType Type;
            public bool Processed = false;

            public Object Result = null;

            public PageNavigationEventArgs(EventType type)
            {
                Type = type;
            }
        }

        public event EventHandler<PageNavigationEventArgs> NavigationHandler;

        public List<PageDesc> NavigationModels = new List<PageDesc>() {
            new PageDesc("pageConnect", null, null),
            new PageDesc("pageWelcome", null, null),
            /* Backup */
            new PageDesc("pageBackupList", "pageWelcome", null),
            new PageDesc("pageBackupCreation", "pageBackupList", null),

            /* Restore */
            new PageDesc("pageRestoreSelectFile", "pageWelcome", null),
            new PageDesc("pageRestoreList", "pageRestoreSelectFile", null),
            new PageDesc("pageRestoration", "pageRestoreList", null)
        };

        private PageDesc GetDesc(string name)
        {
            if (name == null)
                return null;
            foreach (var model in NavigationModels)
            {
                if (model.PageName.ToLower() == name.ToLower())
                    return model;
            }
            return null;
        }

        private string _currentPage;
        public string CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                OnChange("CurrentPage");
            }
        }

        public string BackButtonText
        {
            get
            {
                if (NavigationHandler != null)
                {
                    var e = new PageNavigationEventArgs(PageNavigationEventArgs.EventType.GetBackButtonText);
                    NavigationHandler(this, e);
                    if (e.Processed)
                    {
                        string text = (string)e.Result;
                        return text;
                    }
                }
                return LocalizedResources.Back;
            }
        }

        public Visibility BackButtonVisibility
        {
            get
            {
                if (NavigationHandler != null)
                {
                    var e = new PageNavigationEventArgs(PageNavigationEventArgs.EventType.GetBackButtonVisibleState);
                    NavigationHandler(this, e);
                    if (e.Processed)
                    {
                        bool visible = (bool)e.Result;
                        return visible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                var desc = GetDesc(_currentPage);
                if (desc == null)
                    return Visibility.Collapsed;
                return desc.ShowBack ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string ForwardButtonText
        {
            get
            {
                if (NavigationHandler != null)
                {
                    var e = new PageNavigationEventArgs(PageNavigationEventArgs.EventType.GetForwardButtonText);
                    NavigationHandler(this, e);
                    if (e.Processed)
                    {
                        string text = (string)e.Result;
                        return text;
                    }
                }
                return LocalizedResources.Forward;
            }
        }

        public Visibility ForwardButtonVisibility
        {
            get
            {
                bool processed = false;
                if (NavigationHandler != null)
                {
                    var e = new PageNavigationEventArgs(PageNavigationEventArgs.EventType.GetForwardButtonVisibleState);
                    NavigationHandler(this, e);
                    if (e.Processed)
                    {
                        bool visible = (bool)e.Result;
                        return visible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                var desc = GetDesc(_currentPage);
                if (desc == null)
                    return Visibility.Collapsed;
                return desc.ShowForward ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void NavigateBack()
        {
            bool processed = false;
            if (NavigationHandler != null)
            {
                var e = new PageNavigationEventArgs(PageNavigationEventArgs.EventType.Back);
                NavigationHandler(this, e);
                processed = e.Processed;
            }
            if (processed == false)
            {
                var desc = GetDesc(_currentPage);
                if (desc == null)
                    return;
                if (desc.PreviousPage != null)
                    Navigate(PageFactory.Create(desc.PreviousPage, this));
            }
        }

        public void NavigateForward()
        {
            bool processed = false;
            if (NavigationHandler != null)
            {
                var e = new PageNavigationEventArgs(PageNavigationEventArgs.EventType.Forward);
                NavigationHandler(this, e);
                processed = e.Processed;
            }
            if (processed == false)
            {
                var desc = GetDesc(_currentPage);
                if (desc == null)
                    return;
                if (desc.PreviousPage != null)
                    Navigate(PageFactory.Create(desc.PreviousPage, this));
            }
        }

        public void Navigate(Page page)
        {
            CurrentPage = page.GetType().ToString().Replace("WPBackup.", "");
            if (OnNavigate != null)
            {
                NavigationHandler = null;
                OnNavigate(this, new NavigateEventArgs(page));
                Refresh();
            }
        }

        public void Refresh()
        {
            OnChange("BackButtonVisibility");
            OnChange("ForwardButtonVisibility");
            OnChange("BackButtonText");
            OnChange("ForwardButtonText");
        }

        private string _progressText = null;
        public string ProgressText
        {
            get
            {
                return _progressText;
            }
            set
            {
                _progressText = value;
                OnChange("ProgressText");
            }
        }

        private int _progressValue;
        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                OnChange("ProgressValue");
                OnChange("ProgressValueText");
            }
        }

        public string ProgressValueText
        {
            get
            {
                return _progressValue.ToString() + "%";
            }
        }

        private string _outputFilePath;
        public string OutputFilePath
        {
            get
            {
                return _outputFilePath;
            }
            set
            {
                _outputFilePath = value;
                OnChange("OutputFilePath");
            }
        }

        private Dictionary<string, BackupProcessor.ActionSetting> _actionSettings;
        internal Dictionary<string, BackupProcessor.ActionSetting> ActionSettings
        {
            get
            {
                return _actionSettings;
            }
            set
            {
                _actionSettings = value;
                OnChange("ActionSettings");
            }
        }

        private bool? _backupMode = null;
        public bool? BackupMode
        {
            get
            {
                return _backupMode;
            }
            set
            {
                _backupMode = value;
                OnChange("BackupMode");
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnChange("IsBusy");
            }
        }

        private string _comment = "";
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
                OnChange("Comment");
            }
        }

        private bool _isOperationCancelled = false;
        public bool IsOperationCancelled
        {
            get
            {
                return _isOperationCancelled;
            }
            set
            {
                _isOperationCancelled = value;
                OnChange("IsOperationCancelled");
                OnChange("IsOperationCancelledVisibility");
            }
        }

        public Visibility IsOperationCancelledVisibility
        {
            get
            {
                return _isOperationCancelled ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
        #endregion


}
