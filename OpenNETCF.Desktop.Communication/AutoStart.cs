/*=======================================================================================

	OpenNETCF.Desktop.Communication.AutoStartApps

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
using System;
using System.Collections;

namespace OpenNETCF.Desktop.Communication
{
	/// <summary>
	/// Summary description for AutoStart.
	/// </summary>
	public class AutoStartApps : CollectionBase
	{
		private string		m_key;

		internal AutoStartApps(string RegKey)
		{
			m_key = RegKey;

			string[] vals = null;
			try
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(m_key);

				vals = new string[key.ValueCount];
				string[] names = key.GetValueNames();

				for(int k = 0 ; k < vals.Length ; k++)
				{
					List.Add ((string)key.GetValue(names[k]));
				}

				key.Close();
			}
			catch(Exception)
			{
			}
		}

		/// <summary>
		/// Add a new application to the list of apps to AutoStart
		/// </summary>
		/// <param name="AppPath">Fully qualified path to app</param>
		public void Add(string AppPath)
		{
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(m_key, true);

			// give the value a random name
			key.SetValue(System.Guid.NewGuid().ToString(), AppPath);

			key.Close();
			List.Add(AppPath);
		}

		/// <summary>
		/// Remove an application from the list of apps to AutoStart
		/// </summary>
		/// <param name="AppPath">Fully qualified path to app</param>
		public void Remove(string AppPath)
		{
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(m_key, true);

			string[] vals = key.GetValueNames();

			for(int v = 0 ; v < vals.Length ; v++)
			{
				if(string.Compare((string)key.GetValue(vals[v]), AppPath, true) == 0)
				{
					key.DeleteValue(vals[v], false);
					break;
				}
			}

			key.Close();

			List.Remove(AppPath);
		}

		/// <summary>
		/// Clear all AutoStart apps
		/// </summary>
		public new void Clear()
		{
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(m_key, true);

			string[] vals = key.GetValueNames();

			for(int v = 0 ; v < vals.Length ; v++)
			{
				key.DeleteValue(vals[v], false);
			}

			key.Close();
			List.Clear();
		}

		/// <summary>
		/// Remove an application from the list of apps to AutoStart
		/// </summary>
		/// <param name="Index">Index of item to remove</param>
		public new void RemoveAt(int Index)
		{
			string toremove = (string)List[Index];

			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(m_key, true);

			string[] vals = key.GetValueNames();

			for(int v = 0 ; v < vals.Length ; v++)
			{
				if(string.Compare((string)key.GetValue(vals[v]), toremove, true) == 0)
				{
					key.DeleteValue(vals[v], false);
					break;
				}
			}

			key.Close();

			List.RemoveAt(Index);
		}

		/// <summary>
		/// Indexer
		/// </summary>
		public string this[int index]
		{
			get
			{
				return (string)List[index];
			}
		}
	}
}
