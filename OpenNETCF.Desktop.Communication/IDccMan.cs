/*=======================================================================================

	OpenNETCF.Desktop.Communication.IDccMan

	Copyright © 2003, OpenNETCF.org

	This library is free software; you can redistribute it and/or modify it under 
	the terms of the OpenNETCF.org Shared Source License.

	This library is distributed in the hope that it will be useful, but 
	WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
	FITNESS FOR A PARTICULAR PURPOSE. See the OpenNETCF.org Shared Source License 
	for more details.

	You should have received a copy of the OpenNETCF.org Shared Source License 
	along with this library; if not, email licensing@opennetcf.org to request a copy.

	If you wish to contact the OpenNETCF Advisory Board to discuss licensing, please 
	email licensing@opennetcf.org.

	For general enquiries, email enquiries@opennetcf.org or visit our website at:
	http://www.opennetcf.org

=======================================================================================*/

/* from dccole.h
 * 

// {A7B88840-A812-11cf-8011-00A0C90A8F78}
DEFINE_GUID(IID_IDccManSink, 
0xa7b88840, 0xa812, 0x11cf, 0x80, 0x11, 0x0, 0xa0, 0xc9, 0xa, 0x8f, 0x78);
// {A7B88841-A812-11cf-8011-00A0C90A8F78}
DEFINE_GUID(IID_IDccMan, 
0xa7b88841, 0xa812, 0x11cf, 0x80, 0x11, 0x0, 0xa0, 0xc9, 0xa, 0x8f, 0x78);
// {499C0C20-A766-11cf-8011-00A0C90A8F78}
DEFINE_GUID(CLSID_DccMan, 
0x499c0c20, 0xa766, 0x11cf, 0x80, 0x11, 0x0, 0xa0, 0xc9, 0xa, 0x8f, 0x78);

* 
*/

using System;
using System.Runtime.InteropServices;


namespace OpenNETCF.Desktop.Communication
{
	[ComImport]
	[Guid("A7B88841-A812-11cf-8011-00A0C90A8F78")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] 
	internal interface IDccMan
	{
		[PreserveSig()] int Advise([In, MarshalAs(UnmanagedType.Interface)]IDccManSink pDccSink,
					[Out, MarshalAs(UnmanagedType.U4)]out int dwContext);

		void Unadvise([In, MarshalAs(UnmanagedType.U4)]int dwContext);

		void ShowCommSettings(); 
		void AutoconnectEnable();
		void AutoconnectDisable();

		void ConnectNow();			// Active only when Autoconnect is Disabled
		void DisconnectNow();		// Active only when Autoconnect is Disabled
	
		void SetIconDataTransferring();
		void SetIconNoDataTransferring();
		void SetIconError();
	}

	[Guid("A7B88840-A812-11cf-8011-00A0C90A8F78"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] 
	internal interface IDccManSink
	{
		// The order of these methods *must* match the vtable
		// or bad things happen.
		[PreserveSig()] int OnLogIpAddr([In]int dwIpAddr);
		[PreserveSig()] int OnLogTerminated();
		[PreserveSig()] int OnLogActive();
		[PreserveSig()] int OnLogInactive();
		[PreserveSig()] int OnLogAnswered();
		[PreserveSig()] int OnLogListen();
		[PreserveSig()] int OnLogDisconnection();
		[PreserveSig()] int OnLogError();
	}

	[ComImport, Guid("499C0C20-A766-11cf-8011-00A0C90A8F78")] 
	internal class DccMan
	{
	}
	
	[Guid("C6659361-1625-4746-931C-36014B146679")]
	internal class DccManSink : IDccManSink
	{
		public event ActiveHandler Active;
		public event AnswerHandler Answer;
		public event DisconnectHandler Disconnect;
		public event ErrorHandler Error;
		public event InactiveHandler Inactive;
		public event IPAddrHandler IPChange;
		public event ListenHandler Listen;
		public event TerminatedHandler Terminated;

		#region IDccManSink Members

		public int OnLogActive()
		{
			foreach(ActiveHandler ah in Active.GetInvocationList())
			{
				ah();
			}

			System.Diagnostics.Debug.WriteLine("Active");
			return 0;
		}

		public int OnLogAnswered()
		{
			foreach(AnswerHandler ah in Answer.GetInvocationList())
			{
				ah();
			}

			System.Diagnostics.Debug.WriteLine("Answered");
			return 0;
		}

		public int OnLogDisconnection()
		{
			foreach(DisconnectHandler dh in Disconnect.GetInvocationList())
			{
				dh();
			}

			System.Diagnostics.Debug.WriteLine("Disconnect");
			return 0;
		}

		public int OnLogError()
		{
			foreach(ErrorHandler eh in Error.GetInvocationList())
			{
				eh();
			}

			System.Diagnostics.Debug.WriteLine("Error");
			return 0;
		}

		public int OnLogInactive()
		{
			foreach(InactiveHandler ih in Inactive.GetInvocationList())
			{
				ih();
			}

			System.Diagnostics.Debug.WriteLine("Inactive");
			return 0;
		}

		public int OnLogIpAddr(int dwIpAddr)
		{
			foreach(IPAddrHandler ih in IPChange.GetInvocationList())
			{
				ih(dwIpAddr);
			}

			System.Diagnostics.Debug.WriteLine("IP: " + dwIpAddr.ToString());
			return 0;
		}

		public int OnLogListen()
		{
			foreach(ListenHandler lh in Listen.GetInvocationList())
			{
				lh();
			}

			System.Diagnostics.Debug.WriteLine("Listen");
			return 0;
		}

		public int OnLogTerminated()
		{
			foreach(TerminatedHandler th in Terminated.GetInvocationList())
			{
				th();
			}

			System.Diagnostics.Debug.WriteLine("Terminated");
			return 0;
		}

		#endregion
	}

}
