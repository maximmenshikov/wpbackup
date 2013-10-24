using System.IO;

namespace WPBackup
{
    internal static class IoUtils
    {
        public static void CopyDirectory(string srcPath, string destPath)
        {
            if (!System.IO.Directory.Exists(destPath))
                System.IO.Directory.CreateDirectory(destPath);
            var di = new DirectoryInfo(srcPath);
            foreach (var file in di.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;

                string destFileName = Path.Combine(destPath, file.Name);
                if (File.Exists(destFileName))
                {
                    var fiDest = new FileInfo(destFileName);
                    fiDest.Attributes = FileAttributes.Normal;
                    fiDest = null;
                }
                System.IO.File.Copy(file.FullName, destFileName, true);
            }

            foreach (var dir in di.GetDirectories())
                CopyDirectory(dir.FullName, Path.Combine(destPath, dir.Name));
        }
    }
}
