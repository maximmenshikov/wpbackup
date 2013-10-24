using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenNETCF.Desktop.Communication
{
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct FileInformation //CE_FIND_DATA
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public UInt32 dwFileAttributes;
        [FieldOffset(4)]
        public FILETIME ftCreationTime;
        [FieldOffset(12)]
        public FILETIME ftLastAccessTime;
        [FieldOffset(20)]
        public FILETIME ftLastWriteTime;
        [FieldOffset(28), MarshalAs(UnmanagedType.U4)]
        public UInt32 nFileSizeHigh;
        [FieldOffset(32), MarshalAs(UnmanagedType.U4)]
        public UInt32 nFileSizeLow;
        [FieldOffset(36), MarshalAs(UnmanagedType.U4)]
        public UInt32 dwOID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260), FieldOffset(40)]
        public string FileName;
    };

    /*
    /// <summary> 
    /// This structure describes a file found by the FindFirstFile or FindNextFile. 
    /// </summary> 
    /// <seealso cref="M:OpenNETCF.Desktop.Communication.RAPI.EnumFiles(System.String@)"/> 
    public class FileInformation //WIN32_FIND_DATA
    {
        private byte[] data;

        public FileInformation()
        {
            data = new byte[592];
        }
        
		/// <summary>
		/// Byte representation of FileInformation
		/// </summary>
		public static implicit operator byte[](FileInformation fi)
		{
			return fi.data;
		}
        /// <summary>
        /// File attributes of the file found.
        /// </summary>
        public int FileAttributes
        {
            get
            {
                return BitConverter.ToInt32(data, 0);
            }
        }

        /// <summary>
        /// UTC time at which the file was created.
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                long time = BitConverter.ToInt64(data, 4);
                return DateTime.FromFileTime(time);
            }
        }

        /// <summary>
        /// UTC time at which the file was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                long time = BitConverter.ToInt64(data, 12);
                return DateTime.FromFileTime(time);
            }
        }

        /// <summary>
        /// UTC time at which the file was modified.
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                long time = BitConverter.ToInt64(data, 20);
                return DateTime.FromFileTime(time);
            }
        }

        /// <summary>
        /// Size, in bytes, of file
        /// </summary>
        public long FileSize
        {
            get
            {
                return BitConverter.ToInt32(data, 28) + (BitConverter.ToInt32(data, 32) << 32);
            }
        }

        /// <summary>
        /// Full name of the file
        /// </summary>
        public string FileName
        {
            get
            {
                return Encoding.Unicode.GetString(data, 40, 256).TrimEnd('\0');
            }
        }
    }*/
}
