//==========================================================================================
//
//		OpenNETCF.Desktop.Communication.SYSTEMTIME
//		Copyright (c) 2003, OpenNETCF.org
//
//		This library is free software; you can redistribute it and/or modify it under 
//		the terms of the OpenNETCF.org Shared Source License.
//
//		This library is distributed in the hope that it will be useful, but 
//		WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
//		FITNESS FOR A PARTICULAR PURPOSE. See the OpenNETCF.org Shared Source License 
//		for more details.
//
//		You should have received a copy of the OpenNETCF.org Shared Source License 
//		along with this library; if not, email licensing@opennetcf.org to request a copy.
//
//		If you wish to contact the OpenNETCF Advisory Board to discuss licensing, please 
//		email licensing@opennetcf.org.
//
//		For general enquiries, email enquiries@opennetcf.org or visit our website at:
//		http://www.opennetcf.org
//
//==========================================================================================
using System;
using System.Runtime.InteropServices;

namespace OpenNETCF.Desktop.Communication
{
	internal class SYSTEMTIME
	{
		protected byte[] flatStruct = new byte[ 16 ];

		#region Flat structure offset constants
		protected const	int wYearOffset = 0;
		protected const ushort wMonthOffset = 2; 
		protected const ushort wDayOfWeekOffset = 4; 
		protected const ushort wDayOffset = 6; 
		protected const ushort wHourOffset = 8; 
		protected const ushort wMinuteOffset = 10; 
		protected const ushort wSecondOffset = 12; 
		protected const ushort wMillisecondsOffset = 14; 
		#endregion

		// Construct a SYSTEMTIME from a byte array.  This is
		// used when setting a time zone, which contains two
		// embedded SYSTEMTIME structures.
		public SYSTEMTIME( byte[] bytes ) : this( bytes, 0 )
		{
		}

		// Construct a SYSTEMTIME from a portion of a byte array.  
		// This is used when setting a time zone, which contains 
		// two embedded SYSTEMTIME structures.
		public SYSTEMTIME( byte[] bytes, int offset )
		{
			// Dump the byte array into our array.
			Buffer.BlockCopy( bytes, offset, flatStruct, 0, flatStruct.Length );
		}

		/// <summary>
		/// Initializes a new SYSTEMTIME object with the specified parameters.
		/// </summary>
		/// <param name="year">Year</param>
		/// <param name="month">Month</param>
		/// <param name="day">Day</param>
		/// <param name="hour">Hour</param>
		/// <param name="minute">Minute</param>
		/// <param name="second">Second</param>
		public SYSTEMTIME(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second)
		{
			wYear = year;
			wMonth = month;
			wDayOfWeek = 0;
			wDay = day;
			wHour = hour;
			wMinute = minute;
			wSecond = second;
			wMilliseconds = 0;
		}

		// Method to extract marshal-compatible 'structure' from
		// the class.
		public byte[] ToByteArray()
		{
			return flatStruct;
		}

//----------------
		public SYSTEMTIME(DateTime dt) : this((ushort)dt.Year, (ushort)dt.Month, (ushort)dt.Day, (ushort)dt.Hour, (ushort)dt.Minute, (ushort)dt.Second)
		{
		}

		public SYSTEMTIME() : this(new byte[16])
		{
		}

		public static implicit operator DateTime(SYSTEMTIME st)
		{
			return new DateTime(st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
		}

		public static implicit operator SYSTEMTIME(long FileTime)
		{
			byte[] bytes = new byte[16];

			FileTimeToSystemTimePC(ref FileTime, bytes);

			SYSTEMTIME st = new SYSTEMTIME(bytes);
			return st;
		}

		public static implicit operator long(SYSTEMTIME st)
		{
			byte[] bytes = new byte[16];
			bytes = st.ToByteArray();
			long ft = new long();
			SystemTimeToFileTimePC(bytes, ref ft);

			return ft;
		}

		public SYSTEMTIME FileTimeToSystemTime(long FileTime)
		{
			SYSTEMTIME st = new SYSTEMTIME();

			switch(System.Environment.OSVersion.Platform)
			{
				case PlatformID.WinCE:
					FileTimeToSystemTimeCE(ref FileTime, st.flatStruct);
					break;
				default:
					FileTimeToSystemTimePC(ref FileTime, st.flatStruct);
					break;
			}

			return st;
		}

		public long SystemTimeToFileTime(SYSTEMTIME SystemTime)
		{
			long ft = new long();

			switch(System.Environment.OSVersion.Platform)
			{
				case PlatformID.WinCE:
					SystemTimeToFileTimeCE(SystemTime.flatStruct, ref ft);
					break;
				default:
					SystemTimeToFileTimePC(SystemTime.flatStruct, ref ft);
					break;
			}

			return ft;
		}

		[DllImport("kernel32.dll", EntryPoint="FileTimeToSystemTime", SetLastError=true)]
		internal static extern int FileTimeToSystemTimePC( 
			ref long lpFileTime, 
			byte[] lpSystemTime ); 

		[DllImport("kernel32.dll", EntryPoint="SystemTimeToFileTime", SetLastError=true)]
		internal static extern int SystemTimeToFileTimePC( 
			byte[] lpSystemTime,
			ref long lpFileTime); 
		
		[DllImport("coredll.dll", EntryPoint="FileTimeToSystemTime", SetLastError=true)]
		internal static extern int FileTimeToSystemTimeCE( 
			ref long lpFileTime, 
			byte[] lpSystemTime ); 

		[DllImport("coredll.dll", EntryPoint="SystemTimeToFileTime", SetLastError=true)]
		internal static extern int SystemTimeToFileTimeCE( 
			byte[] lpSystemTime,
			ref long lpFileTime); 
		//----------------

		public static implicit operator byte[]( SYSTEMTIME st )
		{
			return st.ToByteArray();
		}

		public ushort wYear
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wYearOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wYearOffset, 2 );
			}
		}

		public ushort wMonth
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wMonthOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wMonthOffset, 2 );
			}
		}

		public ushort wDayOfWeek
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wDayOfWeekOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wDayOfWeekOffset, 2 );
			}
		}

		public ushort wDay
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wDayOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wDayOffset, 2 );
			}
		}

		public ushort wHour
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wHourOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wHourOffset, 2 );
			}
		}

		public ushort wMinute
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wMinuteOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wMinuteOffset, 2 );
			}
		}

		public ushort wSecond
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wSecondOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wSecondOffset, 2 );
			}
		}

		public ushort wMilliseconds
		{
			get
			{
				return BitConverter.ToUInt16( flatStruct, wMillisecondsOffset );
			}
			set
			{
				byte[]	bytes = BitConverter.GetBytes( value );
				Buffer.BlockCopy( bytes, 0, flatStruct, wMillisecondsOffset, 2 );
			}
		}
	}
}
