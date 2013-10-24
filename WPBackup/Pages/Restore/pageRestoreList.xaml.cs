using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for pageRestoreList.xaml
    /// </summary>
   internal partial class pageRestoreList : Page
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


        internal void ApplyActionSettings(Dictionary<string, BackupProcessor.ActionSetting> dict)
        {
            if (dict != null)
            {
                foreach (var child in stkPanel.Children)
                {
                    if (child is CheckBox)
                    {
                        var cb = child as CheckBox;
                        if (dict.ContainsKey(cb.Tag as string))
                        {
                            cb.IsChecked = dict[cb.Tag as string].IsEnabled;
                            cb.IsEnabled = dict[cb.Tag as string].IsAvailable;
                        }
                        else
                        {
                            cb.IsChecked = false;
                            cb.IsEnabled = false;
                        }
                    }
                }
            }
        }

        public pageRestoreList(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigationHandler += new EventHandler<MainViewModel.PageNavigationEventArgs>(ViewModel_NavigationHandler);
            ApplyActionSettings(ViewModel.ActionSettings);
            ViewModel.Refresh();
        }

        private bool IsAnyChecked()
        {
            bool isAnyChecked = chkApplications.IsChecked == true |
                    chkBingMapsCache.IsChecked == true |
                    chkDocuments.IsChecked == true |
                    chkContactsSmsMmsSchedule.IsChecked == true |
                    chkUserDict.IsChecked == true |
                    chkWifiNetworks.IsChecked == true |
                    chkAlarms.IsChecked == true |
                    chkCameraSequenceNumber.IsChecked == true |
                    chkLockscreenTimeout.IsChecked == true;
            return isAnyChecked;
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Refresh();
        }

        private void AddToDict(Dictionary<string, BackupProcessor.ActionSetting> dict, CheckBox cb)
        {
            if (!dict.ContainsKey(cb.Tag as string))
                dict.Add(cb.Tag as string, new BackupProcessor.ActionSetting((cb.IsEnabled == true) ? true : false, (cb.IsChecked == true) ? true : false));
        }

        private void ViewModel_NavigationHandler(object sender, MainViewModel.PageNavigationEventArgs e)
        {
            if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.GetForwardButtonVisibleState)
            {
                e.Processed = true;
                e.Result = ViewModel.IsConnected && IsAnyChecked();
            }
            else if (e.Type == MainViewModel.PageNavigationEventArgs.EventType.Forward)
            {
                e.Processed = true;
                var dict = new Dictionary<string, BackupProcessor.ActionSetting>();
                AddToDict(dict, chkApplications);
                AddToDict(dict, chkBingMapsCache);
                AddToDict(dict, chkDocuments);
                AddToDict(dict, chkContactsSmsMmsSchedule);
                AddToDict(dict, chkUserDict);
                AddToDict(dict, chkWifiNetworks);
                AddToDict(dict, chkAlarms);
                AddToDict(dict, chkCameraSequenceNumber);
                AddToDict(dict, chkLockscreenTimeout);
                ViewModel.ActionSettings = dict;

                if (chkApplications.IsChecked == true)
                {
                    try
                    {
                        if (System.IO.Directory.Exists(BackupProcessor.AppInfoRepositoryPath))
                            System.IO.Directory.Delete(BackupProcessor.AppInfoRepositoryPath, true);
                    }
                    catch (Exception ex)
                    {
                    }
                    System.IO.Directory.CreateDirectory(BackupProcessor.AppInfoRepositoryPath);
                    if (ViewModel.CurrentFile.Contains("\\AppInfo\\ApplicationInfo.txt"))
                    {
                        ViewModel.CurrentFile.ExtractFolder("\\AppInfo", BackupProcessor.AppInfoRepositoryPath);

                    }
                    /*
                    foreach (var entry in ViewModel.CurrentFile.List)
                    {
                        
                        if (entry.FileName.StartsWith("AppInfo/"))
                        {
                            if (!entry.IsDirectory)
                            {
                                string fileName = BackupProcessor.AppInfoRepositoryPath + "\\";
                                fileName += entry.FileName.Replace("AppInfo/", "").Replace("/", "\\");

                                string folderPath = fileName.Substring(0, fileName.LastIndexOf("\\"));
                                if (System.IO.Directory.Exists(folderPath) == false)
                                    System.IO.Directory.CreateDirectory(folderPath);
                                entry.Extract(BackupProcessor.AppInfoRepositoryPath + "\\..", Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    */

                    var page = new pageSelectApps(ViewModel);
                    page.IsBackupMode = false;
                    page.LoadAppsIntoView(BackupProcessor.AppInfoRepositoryPath, new Action<List<ApplicationInfo>>(delegate(List<ApplicationInfo> list)
                    {
                        foreach (var info in list)
                        {
                            /* we must copy only External (sideloaded) apps as we won't be able to restore all the rest */
                            info.IsDistributivePresent = info.RealIsDistributivePresent(ViewModel);
                            info.IsDataPresent = info.RealIsDataPresent(ViewModel);

                            info.IsDataChecked = info.IsDataPresent;
                            info.IsDistributiveChecked = info.IsDistributivePresent;
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
                                info.Icon = BitmapUtils.LoadImage(path);
                            }
                            else
                            {
                                info.Icon = null;
                            }
                        }
                    }));
                    ViewModel.Navigate(page);
                }
                else
                {
                    ViewModel.Navigate(new pageRestoration(ViewModel));
                }
            }
        }

    }
}
