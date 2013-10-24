using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace WPBackup
{
    internal static class BitmapUtils
    {
        public static BitmapImage LoadImage(string path)
        {
            try
            {
                var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                fs.Seek(0, System.IO.SeekOrigin.Begin);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                fs.Close();

                var ms = new System.IO.MemoryStream(bytes);
                ms.Position = 0;
                var image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
