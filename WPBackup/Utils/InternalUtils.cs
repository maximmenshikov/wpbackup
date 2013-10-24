using WPBackup.Properties;

namespace WPBackup
{
    internal static class InternalUtils
    {
        private static bool _prepared = false;
        private static string _path = "";

        private const string BackupInfoProvider = "WPBackupInfoProvider.exe";
        private const string BackupApplicationRestorer = "WPBackupApplicationRestorer.exe";
        private const string BackupWlanProvider = "WPBackupWlanProvider.exe";
        private const string BackupAlarmProvider = "WPBackupAlarmProvider.exe";
        private const string BackupPingProvider = "WPBackupPingProvider.exe";
        private const string BackupUnlock = "WPBackupUnlock.exe";
        static InternalUtils()
        {
            Prepare();
        }
        private static void SaveArrayToFile(byte[] array, string fileName)
        {
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);
            System.IO.FileStream fs = null;
            try
            {
                fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
                fs.Write(array, 0, array.Length);
            }
            catch (System.IO.IOException ex)
            {
            }
            finally
            {
                fs.Close();
            }
        }
        public static void Prepare()
        {
            _path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\WPBackup\\Temp";
            if (!System.IO.Directory.Exists(_path))
                System.IO.Directory.CreateDirectory(_path);
            SaveArrayToFile(Resources.WPBackupInfoProvider, System.IO.Path.Combine(_path, BackupInfoProvider));
            SaveArrayToFile(Resources.WPBackupApplicationRestorer, System.IO.Path.Combine(_path, BackupApplicationRestorer));
            SaveArrayToFile(Resources.WPBackupWlanProvider, System.IO.Path.Combine(_path, BackupWlanProvider));
            SaveArrayToFile(Resources.WPBackupAlarmProvider, System.IO.Path.Combine(_path, BackupAlarmProvider));
            SaveArrayToFile(Resources.WPBackupPingProvider, System.IO.Path.Combine(_path, BackupPingProvider));
            SaveArrayToFile(Resources.WPBackupUnlock, System.IO.Path.Combine(_path, BackupUnlock));
            _prepared = true;
        }

        public static string BackupInfoProviderPath()
        {
            if (_prepared == false)
                Prepare();
            return System.IO.Path.Combine(_path, BackupInfoProvider);
        }

        public static string BackupApplicationRestorerPath()
        {
            if (_prepared == false)
                Prepare();
            return System.IO.Path.Combine(_path, BackupApplicationRestorer);
        }

        public static string BackupWlanProviderPath()
        {
            if (_prepared == false)
                Prepare();
            return System.IO.Path.Combine(_path, BackupWlanProvider);
        }

        public static string BackupAlarmProviderPath()
        {
            if (_prepared == false)
                Prepare();
            return System.IO.Path.Combine(_path, BackupAlarmProvider);
        }

        public static string BackupPingProviderPath()
        {
            if (_prepared == false)
                Prepare();
            return System.IO.Path.Combine(_path, BackupPingProvider);
        }

        public static string BackupUnlockPath()
        {
            if (_prepared == false)
                Prepare();
            return System.IO.Path.Combine(_path, BackupUnlock);
        }
    }
}
