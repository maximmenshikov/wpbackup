using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenNETCF.Desktop.Communication
{
	#region ------ delegate definitions ------
	/// <summary>
	/// ActiveHandler delegate
	/// </summary>
	public delegate void ActiveHandler();
	/// <summary>
	/// AnswerHandler delegate
	/// </summary>
	public delegate void AnswerHandler();
	/// <summary>
	/// DisconnectHandler delegate
	/// </summary>
	public delegate void DisconnectHandler();
	/// <summary>
	/// ErrorHandler delegate
	/// </summary>
	public delegate void ErrorHandler();
	/// <summary>
	/// InactiveHandler delegate
	/// </summary>
	public delegate void InactiveHandler();
	/// <summary>
	/// IPAddrHandler delegate
	/// </summary>
	public delegate void IPAddrHandler(int IP);
	/// <summary>
	/// ListenHandler delegate
	/// </summary>
	public delegate void ListenHandler();
	/// <summary>
	/// TerminatedHandler delegate
	/// </summary>
	public delegate void TerminatedHandler();
	#endregion


	/// <summary>
	/// This class wraps functions exposed by Microsoft ActiveSync
	/// </summary>
	public class ActiveSync
	{
		#region ------ event definitions ------
		/// <summary>
		/// Indicates that a connection is established between the client application and the connection manager.
		/// </summary>
		public event ActiveHandler Active;

		/// <summary>
		/// Indicates that the Windows CE connection manager has detected the communications interface.
		/// </summary>
		public event AnswerHandler Answer;

		/// <summary>
		/// Indicates that the connection manager has terminated the connection between the desktop computer and the Windows CE–based device
		/// </summary>
		public event DisconnectHandler Disconnect;

		/// <summary>
		/// Indicates that the connection manager failed to start communications between the desktop computer and the Windows CE–based device.
		/// </summary>
		public event ErrorHandler Error;

		/// <summary>
		/// Indicates a disconnection, or disconnected state, between the desktop computer and the Windows CE–based device. 
		/// </summary>
		public event InactiveHandler Inactive;

		/// <summary>
		/// Indicates that an Internet Protocol (IP) address has been established for communication between the desktop computer and the Windows CE–based device.
		/// </summary>
		public event IPAddrHandler IPChange;

		/// <summary>
		/// Indicates that a connection is waiting to be established between the desktop computer and the Windows CE–based device.
		/// </summary>
		public event ListenHandler Listen;

		/// <summary>
		/// Indicates that the Windows CE connection manager has been shut down.
		/// </summary>
		public event TerminatedHandler Terminated;
		#endregion

		/// <summary>
		/// ActiveSync Icon
		/// </summary>
		public enum ActiveSyncIcon
		{
			/// <summary>
			/// Shown when data is transferring
			/// </summary>
			DataTransferring,
			/// <summary>
			/// Shown when no data is transferring (idle)
			/// </summary>
			NoDataTransferring,
			/// <summary>
			/// Shown when an ActiveSync error occurs
			/// </summary>
			Error
		}

		private DccMan			dccMan;
		private IDccMan			idccMan;
		private IDccManSink		idccSink;
		private DccManSink		dccSink;
		private int				dccContext = 0;
		private AutoStartApps	connectapps = new AutoStartApps(@"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnConnect");
		private AutoStartApps	disconnectapps = new AutoStartApps(@"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnDisconnect");

		[DllImport("kernel32", SetLastError=true)]
		static extern unsafe uint GetWindowsDirectory(byte* lpBuffer,uint uSize);

		internal void BeginListen()
		{
			// call advise
			idccMan.Advise(idccSink, out dccContext);
		}

		internal void EndListen()
		{
			// due to the threading model of IDccMan, we cannot Unadvise
			// before the CF's finalizer thread kills the actual COM
			// object, so calling Unadvise from a dtor throws an exception

			// currently this library leaves the objects to be cleaned by the system
			// I don't like it, but I see no workaround at this point

			// unhook the IDccManSink
			// idccMan.Unadvise(dccContext);
		}

		internal ActiveSync()
		{
			try
			{
				// call CoCreateInstance
				dccMan = new DccMan();
				dccSink = new DccManSink();

				// wire all the events
				dccSink.Active += new ActiveHandler(dccSink_Active);
				dccSink.Answer += new AnswerHandler(dccSink_Answer);
				dccSink.Disconnect += new DisconnectHandler(dccSink_Disconnect);
				dccSink.Error += new ErrorHandler(dccSink_Error);
				dccSink.Inactive += new InactiveHandler(dccSink_Inactive);
				dccSink.IPChange += new IPAddrHandler(dccSink_IPChange);
				dccSink.Listen += new ListenHandler(dccSink_Listen);
				dccSink.Terminated += new TerminatedHandler(dccSink_Terminated);

				// QI both
				idccMan = (IDccMan)dccMan;
				idccSink = (IDccManSink)dccSink;
			}
			catch(Exception)
			{
				throw new RAPIException("Unable to create ActiveSync object.  Make sure ActiveSync is installed");
			}
		}

		/// <summary>
		/// When true, all CE devices connecting to the host will connect as guest without prompting for a partnership
		/// </summary>
		public bool ConnectAsGuestOnly
		{
			set
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows CE Services", true);
				key.SetValue("GuestOnly", value ? 1 : 0);
				key.Close();
			}
			get
			{
				bool val = false;
				try
				{
					Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows CE Services");
					val = (int)key.GetValue("GuestOnly", 0) == 1;
					key.Close();
				}
				catch(Exception)
				{
					// key didn't exist, so return false
				}
				return val;
			}
		}

		/// <summary>
		/// Gets or Sets an array of paths for applications that will be automatically run on the PC when a CE device is connected to the host PC
		/// </summary>
		public AutoStartApps AutoStartOnConnect
		{
			set
			{
				connectapps = value;
//				SetAutoRun(@"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnConnect", value);
			}
			get
			{
				return connectapps;
//				return GetAutoRun(@"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnConnect");
			}
		}

		/// <summary>
		/// Gets or Sets an array of paths for applications that will be automatically run on the PC when a CE device is connected to the host PC
		/// </summary>
		public AutoStartApps AutoStartOnDisconnect
		{
			set
			{
				disconnectapps = value;
//				SetAutoRun(@"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnDisconnect", value);
			}
			get
			{
				return disconnectapps;
//				return GetAutoRun(@"SOFTWARE\Microsoft\Windows CE Services\AutoStartOnDisconnect");
			}
		}

		/// <summary>
		/// Display the ActiveSync Settings dialog
		/// </summary>
		public void ShowCommSettings()
		{
			idccMan.ShowCommSettings();
		}

		/// <summary>
		/// Force an ActiveSync connection.  Autoconnect must be disabled.
		/// </summary>
		public void ConnectNow()
		{
			idccMan.ConnectNow();
		}

		/// <summary>
		/// Force an ActiveSync disconnect.  Autoconnect must be disabled.
		/// </summary>
		public void DisconnectNow()
		{
			idccMan.DisconnectNow();
		}

		/// <summary>
		/// Sets the ActiveSync icon
		/// </summary>
		public ActiveSyncIcon Icon
		{
			set
			{
				switch(value)
				{
					case ActiveSyncIcon.DataTransferring:
						idccMan.SetIconDataTransferring();
						break;
					case ActiveSyncIcon.NoDataTransferring:
						idccMan.SetIconNoDataTransferring();
						break;
					case ActiveSyncIcon.Error:
						idccMan.SetIconError();
						break;
				}
			}
		}

		/// <summary>
		/// Sets whether ActiveSync should automatically connect or not
		/// </summary>
		public bool AutoConnect
		{
			set
			{
				if(value)
				{
					idccMan.AutoconnectEnable();
				}
				else
				{
					idccMan.AutoconnectDisable();
				}
			}
			
		}

		/// <summary>
		/// Returns the version of Microsoft ActiveSync currently running
		/// </summary>
		public string Version
		{
			get
			{
				// determine what version of ActiveSync is installed
				byte[] buff = new byte[512]; 
				uint length = 0;
				unsafe 
				{
					fixed(byte *pbuff=buff)
					{
						length = GetWindowsDirectory(pbuff,512);
					}
				}
				ASCIIEncoding ae = new ASCIIEncoding();
				string dllpath = ae.GetString(buff, 0, (int)length) + "\\System32\\rapi.dll";

				// get the RAPI version info
				System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(dllpath);
				return info.FileVersion;
			}
		}

		/// <summary>
		/// Converts an integer into a dotted-string-notation IP address
		/// </summary>
		/// <param name="IP">innteger IP</param>
		/// <returns>Dotted string IP</returns>
		public static string IntToDottedIP(int IP)
		{
			uint ip = (uint)IP;
			byte part = 0;
			string outip = "";

			for(byte i = 0 ; i < 4 ; i++)
			{
				part = (byte)((ip & (0xFF << (i * (byte)8))) >> (i * (byte)8));
				outip += part.ToString();
				if(i < 3)
				{
					outip += ".";
				}
			}

			return outip;
		}

		#region ----- Event sink/relay -----
		private void dccSink_Active()
		{
			if(Active != null)
			{
				foreach(ActiveHandler ah in Active.GetInvocationList())
				{
					ah();
				}
			}
		}

		private void dccSink_Answer()
		{
			if(Answer != null)
			{
				foreach(AnswerHandler ah in Answer.GetInvocationList())
				{
					ah();
				}
			}
		}

		private void dccSink_Disconnect()
		{
			if(Disconnect != null)
			{
				foreach(DisconnectHandler dh in Disconnect.GetInvocationList())
				{
					dh();
				}
			}
		}

		private void dccSink_Error()
		{
			if(Error != null)
			{
				foreach(ErrorHandler eh in Error.GetInvocationList())
				{
					eh();
				}
			}
		}

		private void dccSink_Inactive()
		{
			if(Inactive != null)
			{
				foreach(InactiveHandler ih in Inactive.GetInvocationList())
				{
					ih();
				}
			}
		}

		private void dccSink_IPChange(int IP)
		{
			if(IPChange != null)
			{
				foreach(IPAddrHandler ih in IPChange.GetInvocationList())
				{
					ih(IP);
				}
			}
		}

		private void dccSink_Listen()
		{
			if(Listen != null)
			{
				foreach(ListenHandler lh in Listen.GetInvocationList())
				{
					lh();
				}
			}
		}

		private void dccSink_Terminated()
		{
			if(Terminated != null)
			{
				foreach(TerminatedHandler th in Terminated.GetInvocationList())
				{
					th();
				}
			}
		}
		#endregion
	}
}
