using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.IO;
namespace WPBackup
{

    internal class FlatBackupFile
    {
        private string _internalPath = "";
        
        /// <summary>
        /// ZipFile internal location.
        /// </summary>
        public string InternalPath
        {
            get
            {
                if (_isFolder)
                    return "dir";
                return _internalPath;
            }
            set
            {
                _internalPath = value;
            }
        }

        private string _deviceSidePath = "";
        /// <summary>
        /// Device-side path.
        /// </summary>
        public string DeviceSidePath
        {
            get
            {
                return _deviceSidePath;
            }
            set
            {
                _deviceSidePath = value;
            }
        }

        private string _pcSidePath = "";
        /// <summary>
        /// PC-side path to file. 
        /// <remarks>
        /// Please, use this property only to implicitly set file path for Write() subroutine (!!!)
        /// </remarks>
        /// </summary>
        public string PcSidePath
        {
            get
            {
                return _pcSidePath;
            }
            set
            {
                _pcSidePath = value;
            }
        }

        private bool _isFolder = false;

        /// <summary>
        /// Returns TRUE if this file can be treated as a folder.
        /// <remarks>
        /// If it is a case, InternalPath and PcSidePath can be ignored!
        /// </remarks>
        /// </summary>
        public bool IsFolder
        {
            get
            {
                return _isFolder;
            }
            set
            {
                _isFolder = value;
            }
        }
    }

    internal class FlatBackup
    {
        private ZipFile _file = null;

        public ZipFile File
        {
            get
            {
                return _file;
            }
        }
        public FlatBackup()
        {
        }

        private SortedList<string, FlatBackupFile> _list = new SortedList<string, FlatBackupFile>();

        public SortedList<string, FlatBackupFile> List
        {
            get
            {
                return _list;
            }
            set
            {
                _list = value;
            }
        }

        public bool Contains(string fileName)
        {
            string fileNameLower = fileName.ToLower();
            foreach (var entry in List)
            {
                if (entry.Value.DeviceSidePath.ToLower() == fileNameLower)
                    return true;
            }
            return false;
        }

        public bool ContainsFolder(string fileName)
        {
            string fileNameLower = fileName.ToLower();
            foreach (var entry in List)
            {
                if (entry.Value.DeviceSidePath.ToLower().StartsWith(fileNameLower))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds or replaces entry.
        /// </summary>
        /// <param name="pcSidePath"></param>
        /// <param name="deviceSidePath"></param>
        public void AddEntry(string pcSidePath, string deviceSidePath, bool isFolder, bool overwrite = false)
        {
            var fbf = new FlatBackupFile();
            fbf.DeviceSidePath = deviceSidePath;
            fbf.PcSidePath = pcSidePath;
            fbf.InternalPath = Guid.NewGuid().ToString();
            fbf.IsFolder = isFolder;
            if (List.ContainsKey(deviceSidePath) && overwrite == true)
                List.Remove(deviceSidePath);
            List.Add(deviceSidePath, fbf);
        }

        /// <summary>
        /// Adds or replaces entry based on existing FlatBackupFile.
        /// </summary>
        /// <param name="flatBackupFile"></param>
        /// <param name="overwrite"></param>
        public void AddEntry(FlatBackupFile flatBackupFile, bool overwrite = false)
        {
            if (List.ContainsKey(flatBackupFile.DeviceSidePath) && overwrite == true)
                List.Remove(flatBackupFile.DeviceSidePath);
            List.Add(flatBackupFile.DeviceSidePath, flatBackupFile);
        }

        public bool Extract(string internalName, Stream stream)
        {
            if (_file != null)
            {
                if (_file[internalName] != null)
                {
                    _file[internalName].Extract(stream);
                    return true;
                }
            }
            return false;
        }

        public bool Extract(FlatBackupFile file, Stream stream)
        {
            if (_file != null)
            {
                if (_file[file.InternalPath] != null)
                {
                    _file[file.InternalPath].Extract(stream);
                    return true;
                }
            }
            return false;
        }

        public bool ExtractFolder(string folderPath, string destPath)
        {
            foreach (var item in List)
            {
                var file = item.Value;
                if (file.DeviceSidePath.ToLower().StartsWith(folderPath.ToLower()))
                {
                    // FILE path starts with required path.
                    // let's cut DeviceSidePath to match destPath.
                    var destRelativePath = file.DeviceSidePath.Substring(folderPath.Length);
                    if (file.IsFolder)
                    {
                        if (!Directory.Exists(destPath + "\\" + destRelativePath))
                            Directory.CreateDirectory(destPath + "\\" + destRelativePath);
                    }
                    else if (_file[file.InternalPath] != null)
                    {
                        var directory = Path.GetDirectoryName(destRelativePath);
                        if (!Directory.Exists(destPath + "\\" + directory))
                            Directory.CreateDirectory(destPath + "\\" + directory);
                        var stream = new FileStream(destPath + "\\" + destRelativePath, FileMode.Create, FileAccess.Write);
                        _file[file.InternalPath].Extract(stream);
                        stream.Close();
                    }
                }
            }
            return true;
        }

        public bool Read(string fileName)
        {
            _file = new ZipFile(fileName);
            if (_file != null)
            {
                
                if (_file["index.wpb"] != null)
                {
                    var stream = new MemoryStream();
                    _file["index.wpb"].Extract(stream);
                    
                    List.Clear();

                    stream.Seek(0, SeekOrigin.Begin);

                    byte[] b = new byte[stream.Length];
                    stream.Read(b, 0, (int)stream.Length);

                    int shift = 0;
                    if (b[0] == 0xFF && b[1] == 0xFE)
                        shift = 2;
                    string[] lines = Encoding.Unicode.GetString(b, shift, b.Length - shift).Trim().Replace("\r", "").Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains("="))
                        {
                            var intName = line.Substring(0, line.IndexOf("="));
                            var fsName = line.Substring(line.IndexOf("=") + 1);
                            var fl = new FlatBackupFile();
                            if (intName == "dir")
                                fl.IsFolder = true;
                            else
                                fl.InternalPath = intName;
                            fl.DeviceSidePath = fsName;

                            List.Add(fsName, fl);
                        }
                    }

                    stream.Close();
                }
                return true;
            }
            return false;
        }

        private void FixInternalNames()
        {
            foreach (var item in _list)
            {
                if (String.IsNullOrEmpty(item.Value.InternalPath))
                {
                    item.Value.InternalPath = Guid.NewGuid().ToString();
                }
            }
        }

        public bool Write(string fileName, string comment = "")
        {
            if (_file != null)
                _file.Dispose();
            _file = new ZipFile();
            if (_file != null)
            {
                _file.UseUnicodeAsNecessary = true;
                _file.UseZip64WhenSaving = Zip64Option.Always;
                _file.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                _file.Comment = BackupProcessor.ToBase64(comment);

                FixInternalNames();
                var openedStreams = new List<Stream>();
                string indexContent = "";
                // adding files to output zip.
                foreach (var item in _list)
                {
                    var file = item.Value;
                    if (!file.IsFolder)
                    {
                        if (file.PcSidePath == null)
                            throw new Exception("PcSidePath isn't set for \r\nInternalPath: " + file.InternalPath + "\r\nDeviceSidePath: " + file.DeviceSidePath);

                        var stream = new FileStream(file.PcSidePath, FileMode.Open, FileAccess.Read);
                        if (stream == null)
                            continue;
                        openedStreams.Add(stream);
                        _file.AddEntry(file.InternalPath, "", stream);
                    }
                    indexContent += (file.IsFolder ? "dir" : file.InternalPath) + "=" + file.DeviceSidePath + "\r\n";
                }

                // adding index file
                byte[] b = Encoding.Unicode.GetBytes(indexContent);
                var indexStream = new MemoryStream();
                if (indexStream == null)
                {
                    throw new Exception("Couldn't open stream for index.wpb file");
                }
                indexStream.Write(b, 0, b.Length);
                indexStream.Seek(0, SeekOrigin.Begin);
                openedStreams.Add(indexStream);
                _file.AddEntry("index.wpb", "", indexStream);

                _file.Save(fileName);


                foreach (var stream in openedStreams)
                {
                    stream.Close();
                }
                return true;
            }
            return false;
        }

        public void Cleanup()
        {
            if (_file != null)
                _file.Dispose();
            _file = null;
        }
    }
}
