using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.XPath;
using Microsoft.SmartDevice.Connectivity;

namespace XapToPhone
{

    internal class XapDeployer
    {

        internal enum DeployBehaviourType
        {
            SkipApplication,
            ForceUninstall,
            UpdateApplication
        }

        internal enum DeployResult
        {
            NoConnect,
            NotSupported,
            DeployError,
            Success
        }

        internal enum DeployState
        {
            WaitingForUserInput,
            NotFound,
            InvalidXap,
            Deploying,
            DeployError,
            Success
        }

        internal class Application
        {

            internal class AppInfo
            {

                [CompilerGenerated]
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string _Author;
                [CompilerGenerated]
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string _Description;
                [CompilerGenerated]
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string _Genre;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                [CompilerGenerated]
                private Guid _Guid;
                [CompilerGenerated]
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string _IconPath;
                [CompilerGenerated]
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Version _PlatformVersion;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                [CompilerGenerated]
                private string _Publisher;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                [CompilerGenerated]
                private string _TempIconFileName;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                [CompilerGenerated]
                private string _Title;
                [CompilerGenerated]
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string _Version;

                public string Author
                {
                    get
                    {
                        return _Author;
                    }
                    set
                    {
                        _Author = value;
                    }
                }

                public string Description
                {
                    get
                    {
                        return _Description;
                    }
                    set
                    {
                        _Description = value;
                    }
                }

                public string Genre
                {
                    get
                    {
                        return _Genre;
                    }
                    set
                    {
                        _Genre = value;
                    }
                }

                public Guid Guid
                {
                    get
                    {
                        return _Guid;
                    }
                    set
                    {
                        _Guid = value;
                    }
                }

                public string IconPath
                {
                    get
                    {
                        return _IconPath;
                    }
                    set
                    {
                        _IconPath = value;
                    }
                }

                public Version PlatformVersion
                {
                    get
                    {
                        return _PlatformVersion;
                    }
                    set
                    {
                        _PlatformVersion = value;
                    }
                }

                public string Publisher
                {
                    get
                    {
                        return _Publisher;
                    }
                    set
                    {
                        _Publisher = value;
                    }
                }

                public string TempIconFileName
                {
                    get
                    {
                        return _TempIconFileName;
                    }
                    set
                    {
                        _TempIconFileName = value;
                    }
                }

                public string Title
                {
                    get
                    {
                        return _Title;
                    }
                    set
                    {
                        _Title = value;
                    }
                }

                public string Version
                {
                    get
                    {
                        return _Version;
                    }
                    set
                    {
                        _Version = value;
                    }
                }

                public AppInfo()
                {
                    string s = null;
                    Title = s;
                    Version version = null;
                    PlatformVersion = version;
                    s = null;
                    Version = s;
                    s = null;
                    Author = s;
                    s = null;
                    Publisher = s;
                    s = null;
                    Description = s;
                    s = null;
                    Genre = s;
                    Guid = Guid.Empty;
                    s = null;
                    IconPath = s;
                    s = null;
                    TempIconFileName = s;
                }

            } // class AppInfo

            private const string DefaultAppIcon1 = "ApplicationIcon.png";
            private const string WMAppManifestFile = "WMAppManifest.xml";

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [CompilerGenerated]
            private XapDeployer.Application.AppInfo _Info;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [CompilerGenerated]
            private bool _IsValid;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [CompilerGenerated]
            private string _XapPath;

            public XapDeployer.Application.AppInfo Info
            {
                get
                {
                    return _Info;
                }
                set
                {
                    _Info = value;
                }
            }

            public bool IsValid
            {
                get
                {
                    return _IsValid;
                }
                set
                {
                    _IsValid = value;
                }
            }

            public string XapPath
            {
                get
                {
                    return _XapPath;
                }
                set
                {
                    _XapPath = value;
                }
            }

            internal Application(string nXapPath)
            {
                string s = null;
                XapPath = s;
                bool flag = false;
                IsValid = flag;
                XapDeployer.Application.AppInfo appInfo = new XapDeployer.Application.AppInfo();
                Info = appInfo;
                XapPath = nXapPath;
            }

            internal XapDeployer.DeployState Deploy(XapDeployer.DeviceInfoClass device, XapDeployer.DeployBehaviourType DeployBehaviourType, ref Exception Exception)
            {
                XapDeployer.DeployState Deploy;

                Exception = null;
                bool flag = !File.Exists(XapPath);
                if (flag)
                {
                    Deploy = XapDeployer.DeployState.NotFound;
                }
                else
                {
                    flag = !IsValid;
                    if (flag)
                    {
                        Deploy = XapDeployer.DeployState.InvalidXap;
                    }
                    else
                    {
                        string tmpXapFile = Path.GetTempFileName();
                        File.Copy(XapPath, tmpXapFile, true);
                        File.SetAttributes(tmpXapFile, FileAttributes.Normal);
                        XapDeployer.DeviceInfoClass devinfo = device;
                        XapDeployer.DeployResult Result = XapDeployer.InstallApplication(devinfo, Info.Guid, Info.PlatformVersion, "NormalApp", Info.TempIconFileName, tmpXapFile, DeployBehaviourType, ref Exception);
                        File.Delete(tmpXapFile);
                        flag = (Result == XapDeployer.DeployResult.DeployError) | (Result == XapDeployer.DeployResult.NoConnect) | (Result == XapDeployer.DeployResult.NotSupported);
                        if (flag)
                            Deploy = XapDeployer.DeployState.DeployError;
                        else
                            Deploy = XapDeployer.DeployState.Success;
                    }
                }
                return Deploy;
            }

            internal string ExtractIconFile()
            {
                bool flag;

                Info.TempIconFileName = null;
                try
                {
                    FileStream fileStream1 = new FileStream(XapPath, FileMode.Open, FileAccess.Read);
                    try
                    {
                        XapDeployer.ZipArchive zipArchive1 = new XapDeployer.ZipArchive(fileStream1);
                        Stream stream1 = zipArchive1.GetFileStream(Info.IconPath);
                        flag = stream1 == null;
                        if (flag)
                        {
                            Assembly assembly1 = Assembly.GetExecutingAssembly();
                            stream1 = assembly1.GetManifestResourceStream("ApplicationIcon.png");
                            flag = stream1 == null;
                            if (flag)
                            {
                                return null;
                            }
                        }
                        string TempFileName = Path.GetTempFileName();
                        FileStream fileStream2 = new FileStream(TempFileName, FileMode.Create);
                        try
                        {
                            stream1.CopyTo(fileStream2);
                        }
                        finally
                        {
                            flag = fileStream2 != null;
                            if (flag)
                                fileStream2.Dispose();
                        }
                        Info.TempIconFileName = TempFileName;
                        return TempFileName;
                    }
                    finally
                    {
                        flag = fileStream1 != null;
                        if (flag)
                            fileStream1.Dispose();
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
                return null;
            }

            public static AppInfo ReadManifestFromStream(Stream stream1)
            {
                AppInfo inf = new AppInfo();
                XPathDocument doc1 = new XPathDocument(stream1);

                if (doc1 != null)
                {
                    XPathNavigator nav1 = doc1.CreateNavigator();
                    if (nav1 != null)
                    {
                        nav1.MoveToFirstChild();
                        while (nav1.Name != "Deployment")
                        {
                            if (!nav1.MoveToNext())
                                break;
                        }
                        if (nav1.Name != "Deployment")
                        {
                            return null;
                        }
                        inf.PlatformVersion = new Version(nav1.GetAttribute("AppPlatformVersion", String.Empty));
                        XPathNavigator nav2 = nav1.SelectSingleNode("App");
                        if (nav2 != null)
                        {
                            inf.Title = nav2.GetAttribute("Title", String.Empty);
                            inf.Version = nav2.GetAttribute("Version", String.Empty);
                            inf.Author = nav2.GetAttribute("Author", String.Empty);
                            inf.Publisher = nav2.GetAttribute("Publisher", String.Empty);
                            inf.Description = nav2.GetAttribute("Description", String.Empty);
                            inf.Genre = nav2.GetAttribute("Genre", String.Empty);
                            Guid guid2 = new Guid(nav2.GetAttribute("ProductID", String.Empty));
                            inf.Guid = guid2;
                            if (inf.Title.StartsWith("@"))
                                inf.Title = inf.Description;
                            XPathNavigator nav3 = nav2.SelectSingleNode("IconPath");
                            if (nav3 != null)
                                inf.IconPath = nav3.Value;
                            else
                                inf.IconPath = null;
                            return inf;
                        }
                    }
                    doc1 = null;
                }
                return null;
            }
            public AppInfo ReadManifestFromZip(string filePath)
            {
                AppInfo inf = new AppInfo();
                FileStream fileStream1 = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                try
                {
                    XapDeployer.ZipArchive zipArchive1 = new XapDeployer.ZipArchive(fileStream1);
                    Stream stream1 = zipArchive1.GetFileStream("WMAppManifest.xml");
                    try
                    {
                        return ReadManifestFromStream(stream1);
                    }
                    finally
                    {
                        if (stream1 != null)
                            stream1.Dispose();
                    }
                }
                finally
                {
                    if (fileStream1 != null)
                        fileStream1.Dispose();
                }
                return null;
            }

            internal bool ReadManifest()
            {
                bool flag1;
                bool result = false;
                try
                {
                    Info = ReadManifestFromZip(XapPath);
                    if (Info.Guid != Guid.Empty)
                        result = true;
                }
                catch (Exception e)
                {
                }
                if (result)
                {
                    if (ExtractIconFile() == null)
                        result = false;
                }
                IsValid = result;
                return result;
            }

        } // class Application

        internal class DeviceInfoClass
        {

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [CompilerGenerated]
            private string _DeviceId;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [CompilerGenerated]
            private string _DeviceName;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [CompilerGenerated]
            private string _PlatformId;

            public string DeviceId
            {
                get
                {
                    return _DeviceId;
                }
                set
                {
                    _DeviceId = value;
                }
            }

            public string DeviceName
            {
                get
                {
                    return _DeviceName;
                }
                set
                {
                    _DeviceName = value;
                }
            }

            public string PlatformId
            {
                get
                {
                    return _PlatformId;
                }
                set
                {
                    _PlatformId = value;
                }
            }

            internal DeviceInfoClass()
            {
            }

            internal DeviceInfoClass(string nPlatformId, string nDeviceId, string nDeviceName)
            {
                PlatformId = nPlatformId;
                DeviceId = nDeviceId;
                DeviceName = nDeviceName;
            }

            public override string ToString()
            {
                return DeviceName;
            }

        } // class DeviceInfoClass

        private class ZipArchive
        {

            private List<XapDeployer.ZipArchiveFile> fileList;
            private FileStream stream;

            internal ZipArchive(FileStream stream)
            {
                fileList = new List<XapDeployer.ZipArchiveFile>();
                this.stream = stream;
                XapDeployer.ZipArchiveFile zipArchiveFile1 = XapDeployer.ZipArchiveFile.ReadHeader(stream);
                while (zipArchiveFile1 != null)
                {
                    fileList.Add(zipArchiveFile1);
                    zipArchiveFile1 = XapDeployer.ZipArchiveFile.ReadHeader(stream);
                }
            }

            internal Stream GetFileStream(string filename)
            {
                Stream GetFileStream;

                List<XapDeployer.ZipArchiveFile>.Enumerator enumerator1 = fileList.GetEnumerator();
                try
                {
                    while (enumerator1.MoveNext())
                    {
                        XapDeployer.ZipArchiveFile zipArchiveFile1 = enumerator1.Current;
                        if (String.Compare(zipArchiveFile1.Name, filename, true) == 0)
                        {
                            Stream stream1 = zipArchiveFile1.GetUncompressedStream(stream);
                            GetFileStream = stream1;
                            return GetFileStream;
                        }
                    }
                }
                finally
                {
                    enumerator1.Dispose();
                }
                return null;
            }

        } // class ZipArchive

        private class ZipArchiveFile
        {

            [CompilerGenerated]
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _Name;
            private ushort compressMethod;
            private long dataPosition;

            internal string Name
            {
                get
                {
                    return _Name;
                }
                set
                {
                    _Name = value;
                }
            }

            internal ZipArchiveFile()
            {
            }

            internal Stream GetUncompressedStream(Stream zipStream)
            {
                zipStream.Seek(dataPosition, SeekOrigin.Begin);
                ushort ush = compressMethod;
                bool flag = ush == 0;
                if (flag)
                {
                    return zipStream;
                }
                else
                {
                    flag = ush == 8;
                    if (flag)
                    {
                        return new DeflateStream(zipStream, CompressionMode.Decompress);
                    }
                }
                return null;
            }

            internal static XapDeployer.ZipArchiveFile ReadHeader(FileStream stream)
            {
                XapDeployer.ZipArchiveFile ReadHeader;

                BinaryReader binaryReader1 = new BinaryReader(stream);
                uint ui1 = binaryReader1.ReadUInt32();
                bool flag = checked((int)ui1) != 67324752;
                if (flag)
                {
                    ReadHeader = null;
                }
                else
                {
                    XapDeployer.ZipArchiveFile zipArchiveFile1 = new XapDeployer.ZipArchiveFile();
                    binaryReader1.ReadUInt16();
                    binaryReader1.ReadUInt16();
                    zipArchiveFile1.compressMethod = binaryReader1.ReadUInt16();
                    binaryReader1.ReadUInt16();
                    binaryReader1.ReadUInt16();
                    binaryReader1.ReadUInt32();
                    uint ui2 = binaryReader1.ReadUInt32();
                    binaryReader1.ReadUInt32();
                    ushort ush1 = binaryReader1.ReadUInt16();
                    ushort ush2 = binaryReader1.ReadUInt16();
                    byte[] bArr1 = new byte[checked(ush1 + 1)];
                    binaryReader1.Read(bArr1, 0, ush1);
                    zipArchiveFile1.Name = Encoding.UTF8.GetString(bArr1);
                    binaryReader1.ReadBytes(ush2);
                    zipArchiveFile1.dataPosition = binaryReader1.BaseStream.Position;
                    binaryReader1.BaseStream.Seek(ui2, SeekOrigin.Current);
                    ReadHeader = zipArchiveFile1;
                }
                return ReadHeader;
            }

        } // class ZipArchiveFile

        [DebuggerNonUserCode]
        public XapDeployer()
        {
        }

        internal static XapDeployer.DeviceInfoClass[] GetDevices()
        {
            List<XapDeployer.DeviceInfoClass> list1 = new List<XapDeployer.DeviceInfoClass>();
            DatastoreManager datastoreManager1 = new DatastoreManager(CultureInfo.CurrentUICulture.LCID);
            IEnumerator<Platform> ienumerator1 = datastoreManager1.GetPlatforms().GetEnumerator();
            while (ienumerator1.MoveNext())
            {
                Platform platform1 = ienumerator1.Current;
                IEnumerator<Device> ienumerator2 = platform1.GetDevices().GetEnumerator();
                while (ienumerator2.MoveNext())
                {
                    Device device1 = ienumerator2.Current;
                    XapDeployer.DeviceInfoClass dev = new XapDeployer.DeviceInfoClass(platform1.Id.ToString(), device1.Id.ToString(), device1.Name);
                    list1.Add(dev);
                }
            }
            return list1.ToArray();
        }

        internal static XapDeployer.DeployResult InstallApplication(XapDeployer.DeviceInfoClass DeviceInfoClass, Guid appGuid, Version appVersion, string applicationGenre, string iconFile, string xapFile, XapDeployer.DeployBehaviourType DeployBehaviourType, ref Exception Exception)
        {
            Exception = null;
            try
            {
                DatastoreManager datastoreManager1 = new DatastoreManager(CultureInfo.CurrentUICulture.LCID);
                if (datastoreManager1 != null)
                {
                    Platform platform1 = datastoreManager1.GetPlatform(new ObjectId(DeviceInfoClass.PlatformId));
                    if (platform1 != null)
                    {
                        Device device1 = platform1.GetDevice(new ObjectId(DeviceInfoClass.DeviceId));
                        if (device1 != null)
                        {
                            device1.Connect();
                            SystemInfo systemInfo1 = device1.GetSystemInfo();
                            Version version1 = new Version(systemInfo1.OSMajor, systemInfo1.OSMinor);
                            bool flag1 = appVersion.CompareTo(version1) > 0;
                            if (flag1)
                            {
                                device1.Disconnect();
                                return XapDeployer.DeployResult.NotSupported;
                            }
                            flag1 = device1.IsApplicationInstalled(appGuid);
                            if (flag1)
                            {
                                bool flag2 = DeployBehaviourType == XapDeployer.DeployBehaviourType.SkipApplication;
                                if (flag2)
                                {
                                    device1.Disconnect();
                                    return XapDeployer.DeployResult.Success;
                                }
                                else
                                {
                                    RemoteApplication remoteApplication1 = device1.GetApplication(appGuid);
                                    flag2 = DeployBehaviourType == XapDeployer.DeployBehaviourType.ForceUninstall;
                                    if (flag2)
                                    {
                                        remoteApplication1.Uninstall();
                                    }
                                    else
                                    {
                                        flag2 = DeployBehaviourType == XapDeployer.DeployBehaviourType.UpdateApplication;
                                        if (flag2)
                                        {
                                            remoteApplication1.UpdateApplication(applicationGenre, iconFile, xapFile);
                                            device1.Disconnect();
                                            return XapDeployer.DeployResult.Success;
                                        }
                                    }
                                }
                            }
                            device1.InstallApplication(appGuid, appGuid, applicationGenre, iconFile, xapFile);
                            device1.Disconnect();
                            return XapDeployer.DeployResult.Success;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return XapDeployer.DeployResult.DeployError;
            }
            return XapDeployer.DeployResult.DeployError;
        }

    } // class XapDeployer

}
