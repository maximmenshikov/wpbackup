using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using System.IO;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   internal partial class MainWindow : Window
    {
        public MainViewModel viewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        void FixFileAssociations()
        {
            try
            {
                var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(".wpbackup", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (key != null)
                {
                    key.SetValue(null, "wpbackupfile");
                    key.Close();
                    key = null;
                }
                key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("wpbackupfile", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (key != null)
                {
                    key.SetValue(null, "WPBackup file");
                    key.Close();
                    key = null;
                }
                key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("wpbackupfile\\shell\\open\\command", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (key != null)
                {
                    key.SetValue(null, "\"" + Assembly.GetExecutingAssembly().Location + "\"" + " " + "\"%1\"");
                    key.Close();
                    key = null;
                }
            }
            catch (Exception ex)
            {
                // error while editing file associations. Rights elevation required?
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            FixFileAssociations();

            try
            {
                var test = new Microsoft.SmartDevice.Connectivity.SmartDeviceException();
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizedResources.NoSDK, "WPBackup", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Forms.Application.Exit();
                return;
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            viewModel.OnModeChanged += new EventHandler(viewModel_OnModeChanged);
            viewModel.OnChange("WorkMode");
            viewModel.OnNavigate += new EventHandler<MainViewModel.NavigateEventArgs>(viewModel_OnNavigate);
            viewModel.Navigate(new pageWelcome(viewModel));
            if (Application.Current.Properties.Contains("CommandLine"))
            {
                foreach (var cl in (Application.Current.Properties["CommandLine"] as string[])) 
                {
                    if (System.IO.File.Exists(cl))
                    {
                        var pg = new pageRestoreSelectFile(viewModel);
                        pg.SetFilePath(cl);
                        viewModel.Navigate(pg);
                        break;
                    }
                }
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var sw = new System.IO.StreamWriter(System.Windows.Forms.Application.StartupPath + "\\exceptionlog.txt", true);
            sw.WriteLine(e.ExceptionObject.ToString());
            sw.Close();
        }

        
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }
        

        private void viewModel_OnModeChanged(object sender, EventArgs e)
        {

        }

        private void viewModel_OnNavigate(object sender, MainViewModel.NavigateEventArgs e)
        {
            
            if (e.Page.GetType().ToString().Contains("Welcome"))
                lblAbout.Visibility = Visibility.Visible;
            else
                lblAbout.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(e.Page);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            RapiComm.RAPI.Connect(false);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            viewModel.NavigateBack();
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            viewModel.NavigateForward();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (viewModel.IsBusy)
            {
                e.Cancel = true;
            }
            else
            {
                RapiComm.RAPI.Dispose();
            }
        }


        private void lblAbout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.Navigate(new pageAbout(viewModel));
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (RapiComm.IsConnected)
            {
                var tempPath = System.Windows.Forms.Application.StartupPath + "\\Test3";
                var list = RapiComm.CopyDirectoryFromDeviceFLAT("\\My Documents", tempPath, "");

                var fb = new FlatBackup();
                foreach (var item in list.Values)
                {
                    fb.AddEntry(item, true);
                    //fb.List.Add((item.IsFolder ? "dir" : item.InternalPath), item);
                    Console.WriteLine((item.IsFolder ? "dir" : item.InternalPath) + "=" + item.DeviceSidePath);
                }
                fb.Write(System.Windows.Forms.Application.StartupPath + "\\test.zip", "");
            }
            else
            {
                var fb = new FlatBackup();
                fb.Read(System.Windows.Forms.Application.StartupPath + "\\test.zip");
                var tempPath = "C:\\Temp\\WPBackup";
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                foreach (var item in fb.List)
                {
                    var file = item.Value;
                    Console.WriteLine("\"" + file.InternalPath + "|||" + file.DeviceSidePath + "\"");
                    if (file.IsFolder)
                    {
                        if (!Directory.Exists(tempPath + "\\" + file.DeviceSidePath))
                            Directory.CreateDirectory(tempPath + "\\" + file.DeviceSidePath);
                    }
                    else if (fb.File[file.InternalPath] != null)
                    {
                        var directory = Path.GetDirectoryName(file.DeviceSidePath);
                        if (!Directory.Exists(tempPath + "\\" + directory))
                            Directory.CreateDirectory(tempPath + "\\" + directory);
                        var stream = new FileStream(tempPath + "\\" + file.DeviceSidePath, FileMode.Create, FileAccess.Write);
                        fb.Extract(file, stream);
                        stream.Close();
                        file.PcSidePath = tempPath + "\\" + file.DeviceSidePath;
                    }
                }
                fb.AddEntry(System.Windows.Forms.Application.StartupPath + "\\PresentationCore.dll", "\\Applications\\PresentationCore.dll", false, true);
                fb.AddEntry("", "\\TestFolder", true, true);
                fb.Write(System.Windows.Forms.Application.StartupPath + "\\test.zip", "");
            }
        }

    }
}