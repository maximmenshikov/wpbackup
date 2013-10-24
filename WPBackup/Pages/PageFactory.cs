using System.Windows.Controls;

namespace WPBackup
{
    internal static class PageFactory
    {

        public static Page Create(string uriOrName, MainViewModel viewModel)
        {
            if (uriOrName.Contains("pageBackupList"))
                return new pageBackupList(viewModel);
            else if (uriOrName.Contains("pageRestoreList"))
                return new pageRestoreList(viewModel);
            else if (uriOrName.Contains("pageWelcome"))
                return new pageWelcome(viewModel);
            else if (uriOrName.Contains("pageRestoreSelectFile"))
                return new pageRestoreSelectFile(viewModel);
            else if (uriOrName.Contains("pageBackupCreation"))
                return new pageBackupCreation(viewModel);
            else if (uriOrName.Contains("pageRestoration"))
                return new pageBackupCreation(viewModel);
            return null;
        }
    }
}
