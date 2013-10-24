using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Forms;

namespace WPBackup
{
    /// <summary>
    /// Interaction logic for CreateBackup.xaml
    /// </summary>
    public partial class CreateBackup : Window
    {
        
        public BackupViewModel viewModel
        {
            get
            {
                return this.DataContext as BackupViewModel;
            }
        }
        private BackgroundWorker myBackgroundWorker = null;
        private BackupMaker _backup = null;
        internal BackupMaker Backup
        {
            get
            {
                if (_backup == null)
                    _backup = new BackupMaker();
                return _backup;
            }
        }
        public CreateBackup()
        {
            InitializeComponent();
            _backup = null;
        }

        
        private void btnMake_Click(object sender, RoutedEventArgs e)
        {

            Backup.BackupContacts = (bool)chkContacts.IsChecked;
            Backup.BackupSms = (bool)chkSms.IsChecked;
            Backup.BackupMms = (bool)chkMms.IsChecked;
            Backup.BackupSchedule = (bool)chkSchedule.IsChecked;
            Backup.BackupDocuments = (bool)chkDocuments.IsChecked;
            Backup.BackupBingMapsCache = (bool)chkBingMapsCache.IsChecked;
            Backup.BackupUserDict = (bool)chkUserDict.IsChecked;
            Backup.BackupApplications = (bool)chkApplications.IsChecked;
            //Backup.BackupZune = (bool)chkZune.IsChecked;

            Backup.TempFolderPath = "";
            Backup.DeviceTempFolderPath = "";
            Backup.OutputBackupFilePath = "";

            /*
            if (Backup.BackupZune)
            {
                var browser = new FolderBrowserDialog();
                if (browser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                Backup.TempFolderPath = browser.SelectedPath;
            }
            else
            {*/
                var ffd = new SaveFileDialog();
                ffd.Title = "Choose a location for backup file";
                ffd.InitialDirectory = "C:\\";
                ffd.Filter = "*.backup|*.backup";
                if (ffd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                Backup.OutputBackupFilePath = ffd.FileName;
            //}
            viewModel.IsRunning = true;
            myBackgroundWorker = new BackgroundWorker();
            var bgw = myBackgroundWorker;
            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;
            bgw.RunWorkerAsync();
        }

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            viewModel.IsRunning = false;
            if (Backup.OutputBackupFilePath != "")
            {
                var zip = new Ionic.Zip.ZipFile();
                zip.UseUnicodeAsNecessary = true;
                //zip.AlternateEncoding = Encoding.UTF8;
                //zip.AlternateEncodingUsage = Ionic.Zip.ZipOption.AsNecessary;
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                zip.AddDirectory(Backup.TempFolderPath, null);
                zip.Save(Backup.OutputBackupFilePath);
                //zip.Dispose();
            }
        }

        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            int stageCount = (int)Backup.PerformStage(int.MaxValue, true);
            double percent = 100 / stageCount;
            for (int i = 0; i < stageCount; i++)
            {
                if (worker.CancellationPending)
                    break;
                Backup.PerformStage(i);
                worker.ReportProgress((int)(percent * i));
            }
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (myBackgroundWorker != null)
                myBackgroundWorker.CancelAsync();
        }
    }
}
