using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void SetCulture(CultureInfo culture)
        {
            Type type = typeof(CultureInfo);

            try
            {
                type.InvokeMember("s_userDefaultCulture",
                                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                                    null,
                                    culture,
                                    new object[] { culture });

                type.InvokeMember("s_userDefaultUICulture",
                                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                                    null,
                                    culture,
                                    new object[] { culture });
            }
            catch { }

            try
            {
                type.InvokeMember("m_userDefaultCulture",
                                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                                    null,
                                    culture,
                                    new object[] { culture });

                type.InvokeMember("m_userDefaultUICulture",
                                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                                    null,
                                    culture,
                                    new object[] { culture });
            }
            catch { }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.Properties["CommandLine"] = e.Args;
            Utils.URLSecurityZoneAPI.InternetSetFeatureEnabled(Utils.URLSecurityZoneAPI.InternetFeaturelist.DISABLE_NAVIGATION_SOUNDS, Utils.URLSecurityZoneAPI.SetFeatureOn.PROCESS, true);
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            var rapi = new OpenNETCF.Desktop.Communication.RAPI();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is DllNotFoundException)
            {
                MessageBox.Show(LocalizedResources.NoSDK, "WPBackup", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Forms.Application.Exit();
                e.Handled = true;
            }
        }
    }
}
