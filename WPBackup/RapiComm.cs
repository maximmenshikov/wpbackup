using System;
using System.IO;
using System.Threading;
using System.Windows;
using OpenNETCF.Desktop.Communication;
using System.Collections.Generic;
using WPBackup;

internal static class RapiComm
{

    public static event EventHandler OnConnected;
    public static event EventHandler OnDisconnected;
    public static event EventHandler OnConnectingStateChanged;

    private static OpenNETCF.Desktop.Communication.RAPI _rapi = null;

    public static string LastError = "";

    #region "Normal routines"
    public static bool CopyDirectoryFromDevice(string src, string dest, string exceptFolders = "")
    {
        LastError = "";
        src = src.TrimEnd('\\');
        dest = dest.TrimEnd('\\');
        try
        {
            var list = RapiComm.RAPI.EnumFiles(src + "\\*");
            if (list == null)
            {
                return false;
            }
            Directory.CreateDirectory(dest);
            foreach (FileInformation item in list)
            {
                if (item.FileName != "." && item.FileName != "..")
                {
                    if ((item.dwFileAttributes & (int)FileAttributes.Directory) > 0)
                    {
                        if (exceptFolders.Contains("(" + item.FileName + ")") == false)
                        {
                            CopyDirectoryFromDevice(src + "\\" + item.FileName, dest + "\\" + item.FileName);
                        }
                    }
                    else
                    {
                        RAPI.CopyFileFromDevice(src + "\\" + item.FileName, dest + "\\" + item.FileName, true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LastError = ex.ToString();
            return false;
        }
        return true;
    }

    public static bool CopyDirectoryToDevice(string src, string dest, string exceptFolders = "")
    {
        LastError = "";
        src = src.TrimEnd('\\');
        dest = dest.TrimEnd('\\');
        try
        {
            RAPI.CreateDeviceDirectory(dest);
            var files = Directory.GetFiles(src);
            foreach (var file in files)
            {
                string shortName = file;
                if (shortName.Contains("\\"))
                    shortName = shortName.Substring(shortName.LastIndexOf("\\") + 1);
                RAPI.CopyFileToDevice(file, dest + "\\" + shortName, true);
            }
            var folders = Directory.GetDirectories(src);
            foreach (var folder in folders)
            {
                string shortName = folder;
                if (shortName.Contains("\\"))
                    shortName = shortName.Substring(shortName.LastIndexOf("\\") + 1);
                RAPI.CreateDeviceDirectory(dest + "\\" + shortName);
                CopyDirectoryToDevice(folder, dest + "\\" + shortName);
            }
        }
        catch (Exception ex)
        {
            LastError = ex.ToString();
            Console.WriteLine(ex.ToString() + "\n");
            return false;
        }
        return true;
    }
    #endregion

    #region "FLAT routines"
    /// <summary>
    /// Copies files and directories to PC using flat structure.
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    /// <param name="exceptFolders"></param>
    /// <returns>A list of generated associations "PcSideFile - DeviceSideFile"</returns>
    public static SortedList<string, FlatBackupFile> CopyDirectoryFromDeviceFLAT(string src, string dest, string deviceSidePath, string exceptFolders = "")
    {
        if (deviceSidePath == null)
            deviceSidePath = src;
        LastError = "";
        src = src.TrimEnd('\\');
        dest = dest.TrimEnd('\\');

        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        var retlist = new SortedList<string, FlatBackupFile>();
        try
        {
            var list = RapiComm.RAPI.EnumFiles(src + "\\*");
            if (list == null || list.Count == 0)
            {
                // if list is empty, let's add it this directory to be (at least) 
                // able to restore folder structure later.
                var fbf = new FlatBackupFile();
                fbf.IsFolder = true;
                fbf.DeviceSidePath = deviceSidePath;
                retlist.Add(fbf.DeviceSidePath, fbf);
                return retlist;
            }

            foreach (FileInformation item in list)
            {
                if (item.FileName != "." && item.FileName != "..")
                {
                    if ((item.dwFileAttributes & (int)FileAttributes.Directory) > 0)
                    {
                        if (exceptFolders.Contains("(" + item.FileName + ")") == false)
                        {
                            var newlist = CopyDirectoryFromDeviceFLAT(src + "\\" + item.FileName, dest, deviceSidePath + "\\" + item.FileName);
                            foreach (var newitem in newlist)
                                retlist.Add(newitem.Value.DeviceSidePath, newitem.Value);

                        }
                    }
                    else
                    {
                        var srcFullName = src + "\\" + item.FileName;
                        var destShortFileName = Guid.NewGuid().ToString();
                        var destFullName = dest + "\\" + destShortFileName;
                        RAPI.CopyFileFromDevice(srcFullName, destFullName, true);

                        var fbf = new FlatBackupFile();
                        fbf.DeviceSidePath = deviceSidePath + "\\" + item.FileName; //srcFullName;
                        fbf.InternalPath = destShortFileName;
                        fbf.PcSidePath = destFullName;
                        retlist.Add(fbf.DeviceSidePath, fbf);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LastError = ex.ToString();
        }
        return retlist;
    }

    public static FlatBackupFile CopyFileFromDeviceFLAT(string src, string destFolder)
    {
        var fbf = new FlatBackupFile();

        string mangledName = Guid.NewGuid().ToString();
        string destPath = destFolder + "\\" + mangledName;
        if (RAPI.CopyFileFromDevice(src, destPath, true) == true)
        {
            fbf.DeviceSidePath = src;
            fbf.PcSidePath = destPath;
            fbf.InternalPath = mangledName;
            return fbf;
        }
        return null;
    }
    public static bool CopyDirectoryToDeviceFLAT(SortedList<string, FlatBackupFile> list, string folderPath, string dest, string exceptFolders = "")
    {
        LastError = "";
        
        try
        {
            RAPI.CreateDeviceDirectory(dest);
            foreach (var item in list)
            {
                var file = item.Value;
                if (file.DeviceSidePath.ToLower().StartsWith(folderPath.ToLower()))
                {
                    if (String.IsNullOrEmpty(dest))
                        dest = folderPath;
                    else
                        dest = dest.TrimEnd('\\');
                    if (file.IsFolder)
                    {
                        RAPI.CreateDeviceDirectory(dest);
                    }
                    else
                    {
                        folderPath = folderPath.TrimEnd('\\');
                        var relativePath = file.DeviceSidePath.Substring(folderPath.Length);
                        RAPI.CopyFileToDevice(file.PcSidePath, dest + relativePath, true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LastError = ex.ToString();
            Console.WriteLine(ex.ToString() + "\n");
            return false;
        }
        return true;
    }

    public static bool CopyFileToDeviceFLAT(SortedList<string, FlatBackupFile> list, string fileName, string destFileName = null)
    {
        string temp = fileName.ToLower();
        foreach (var item in list)
        {
            var file = item.Value;
            if (file.DeviceSidePath.ToLower().StartsWith(temp))
            {
                if (destFileName == null)
                    destFileName = file.DeviceSidePath;
                RAPI.CopyFileToDevice(file.PcSidePath, destFileName, true);
                return true;
            }
        }
        return false;
    }
    #endregion

    public static OpenNETCF.Desktop.Communication.RAPI RAPI
    {
        get
        {
            return _rapi;
        }
    }

    public static void Ping()
    {

    }

    private static bool _isConnected = false;

    public static bool IsConnected
    {
        get
        {
            return _isConnected;
        }
        set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                Application.Current.Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        if (value == true)
                        {
                            if (OnConnected != null)
                                OnConnected(null, new EventArgs());
                        }
                        else
                        {
                            if (OnDisconnected != null)
                                OnDisconnected(null, new EventArgs());
                        }
                    }));
            }
        }
    }

    private static bool _isConnecting = false;
    public static bool IsConnecting
    {
        get
        {
            return _isConnecting;
        }
        set
        {
            if (_isConnecting != value)
            {
                _isConnecting = value;
                if (OnConnectingStateChanged != null)
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            OnConnectingStateChanged(null, new EventArgs());
                        }
                        ));
                    }
                }
            }
        }
    }

    static RapiComm()
    {
        _rapi = new OpenNETCF.Desktop.Communication.RAPI();
        _rapi.RAPIConnected += new OpenNETCF.Desktop.Communication.RAPIConnectedHandler(Connection_RAPIConnected);
        _rapi.RAPIDisconnected += new OpenNETCF.Desktop.Communication.RAPIConnectedHandler(Connection_RAPIDisconnected);
        _rapi.RAPIConnectingStateChanged += new EventHandler(Connection_RAPIConnecting);
    }

    private static void Connection_RAPIConnecting(object sender, EventArgs e)
    {
        IsConnecting = _rapi.IsConnecting;
    }
    private static void Connection_RAPIConnected()
    {
        IsConnected = true;
    }

    private static void Connection_RAPIDisconnected()
    {
        IsConnected = false;
    }

    public static bool RunAndWait(string file, string args, bool wait = false)
    {
        if (file.IndexOf("\\") != -1)
        {
            string path = Path.GetDirectoryName(file);
            string shortName = Path.GetFileName(file);
            string shortNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            RapiComm.RAPI.DeleteDeviceFile(path + "\\" + shortNameWithoutExt + ".mon");
            IntPtr hProcess = RapiComm.RAPI.CreateProcess(file, args);
            if (hProcess != IntPtr.Zero)
            {
                if (wait)
                {
                    while (RapiComm.RAPI.GetDeviceFileAttributes(path + "\\" + shortNameWithoutExt + ".mon") == RAPI.RAPIFileAttributes.InvalidFileAttributes)
                    {
                        Thread.Sleep(1000);
                    }
                }
                return true;
            }
        }
        return false;
    }
}
