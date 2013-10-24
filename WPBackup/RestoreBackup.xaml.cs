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
    /// Interaction logic for RestoreBackup.xaml
    /// </summary>
    public partial class RestoreBackup : Window
    {
        public RestoreBackup()
        {
            InitializeComponent();
        }

        public BackupViewModel viewModel
        {
            get
            {
                return this.DataContext as BackupViewModel;
            }
        }
        private BackgroundWorker myBackgroundWorker = null;
        private BackupRestorer _backup = null;
        internal BackupRestorer Backup
        {
            get
            {
                if (_backup == null)
                    _backup = new BackupRestorer();
                return _backup;
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
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
