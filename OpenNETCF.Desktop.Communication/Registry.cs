//==========================================================================================
//
//		OpenNETCF.Desktop.Communication.Registry
//		Copyright (c) 2003-2004, OpenNETCF.org
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
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

namespace OpenNETCF.Desktop.Communication
{
	#region CERegistry
	/// <summary>
	/// Supplies the base registry keys that access values and subkeys in the registry on the attached device.
	/// </summary>
	/// <remarks>This class was renamed from its previous name of "Registry" to avoid naming clashes with the desktop equivalent.</remarks>
	/// <seealso cref="T:Microsoft.Win32.Registry">Desktop Registry Class</seealso>
	public sealed class CERegistry
	{
		// Special static RegKey values.

		/// <summary>
		/// Contains the configuration data for the local machine. This field reads the Windows registry base key HKEY_LOCAL_MACHINE.
		/// </summary>
		public static readonly CERegistryKey LocalMachine = new CERegistryKey((uint)RootKeys.LocalMachine, "\\HKEY_LOCAL_MACHINE", true, true);
		/// <summary>
		/// Contains information about the current user preferences. This field reads the Windows registry base key HKEY_CURRENT_USER.
		/// </summary>
		public static readonly CERegistryKey CurrentUser = new CERegistryKey((uint)RootKeys.CurrentUser, "\\HKEY_CURRENT_USER", true, true);
		/// <summary>
		///  Defines the types (or classes) of documents and the properties associated with those types. This field reads the Windows registry base key HKEY_CLASSES_ROOT.
		/// </summary>
		public static readonly CERegistryKey ClassesRoot = new CERegistryKey((uint)RootKeys.ClassesRoot, "\\HKEY_CLASSES_ROOT", true, true);
		/// <summary>
		/// Contains information about the default user configuration. This field reads the Windows registry base key HKEY_USERS.
		/// </summary>
		public static readonly CERegistryKey Users = new CERegistryKey((uint)RootKeys.Users, "\\HKEY_USERS", true, true);
		
	}
	internal enum RootKeys : uint 
	{
		ClassesRoot = 0x80000000, 
		CurrentUser = 0x80000001, 
		LocalMachine = 0x80000002,
		Users = 0x80000003 
	} 
	#endregion


	/// <summary>
	/// Represents a key level node in the device-side Windows registry.
	/// This class is a registry encapsulation.
	/// </summary>
	/// <remarks>This class was renamed from its previous name of "RegistryKey" to avoid naming clashes with the desktop equivalent.
	/// </remarks>
	/// <seealso cref="T:Microsoft.Win32.RegistryKey">Desktop RegistryKey Class</seealso>
	public sealed class CERegistryKey : MarshalByRefObject, IDisposable
	{
		//hkey - handle to a registry key
		private uint m_handle;
		//full name of key
		private string m_name;
		//was key opened as writable
		private bool m_writable;
		//is root key
		private bool m_isroot;

		//error code when all items have been enumerated
		private const int ERROR_NO_MORE_ITEMS = 259;

		#region Constructor
		internal CERegistryKey(uint rootKey, string name, bool writable, bool isroot)
		{
			m_handle = rootKey;
			m_writable = writable;
			m_name = name;
			m_isroot = isroot;
		}
		#endregion


		#region Name
		/// <summary>
		/// Retrieves the name of the key.
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
		}
		#endregion

		#region SubKeyCount
		/// <summary>
		/// Retrieves the count of subkeys at the base level, for the current key.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException"> The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public int SubKeyCount
		{
			get
			{
				//check handle
				if(CheckHKey())
				{
					int subkeycount;
					int valuescount;
					int maxsubkeylen;
					int maxsubkeyclasslen;
					int maxvalnamelen;
					int maxvallen;
					char[] name = new char[256];
					int namelen = name.Length;

					if(RegQueryInfoKey(m_handle, name, ref namelen, 0, out subkeycount, out maxsubkeylen, out maxsubkeyclasslen, out valuescount, out maxvalnamelen, out maxvallen, 0, 0)==0)
					{
						return subkeycount;
					}
					else
					{
						throw new ExternalException("Error retrieving registry properties");
					}
				}
				else
				{
					throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
				}
			}
		}
		#endregion

		#region ValueCount
		/// <summary>
		/// Retrieves the count of values in the key.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException"> The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public int ValueCount
		{
			get
			{
				//check handle
				if(CheckHKey())
				{
					int subkeycount;
					int valuescount;
					int maxsubkeylen;
					int maxsubkeyclasslen;
					int maxvalnamelen;
					int maxvallen;
					char[] name = new char[256];
					int namelen = name.Length;

					if(RegQueryInfoKey(m_handle, name, ref namelen, 0, out subkeycount, out maxsubkeylen, out maxsubkeyclasslen, out valuescount, out maxvalnamelen, out maxvallen, 0, 0)==0)
					{
						return valuescount;
					}
					else
					{
						throw new ExternalException("Error retrieving registry properties");
					}
				}
				else
				{
					throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
				}
			}
		}
		#endregion

		#region ToString
		/// <summary>
		/// Retrieves a string representation of this key.
		/// </summary>
		/// <returns>A string representing the key. If the specified key is invalid (cannot be found) then a null value is returned.</returns>
		/// <exception cref="System.ObjectDisposedException"> The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public override string ToString()
		{
			if(CheckHKey())
			{
				return m_name + " [0x" + m_handle.ToString("X") + "]";
			}
			else
			{
				throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
			}
		}
		#endregion

		#region Close
		/// <summary>
		/// Closes the key and flushes it to storage if the contents have been modified.
		/// </summary>
		/// <remarks>Calling this method on system keys will have no effect, since system keys should never be closed.
		/// This method does nothing if you call it on an instance of <b>RegistryKey</b> that is already closed.</remarks>
		public void Close()
		{
			if(m_isroot)
			{
				//we do not close root keys - because they can not be reopened
				//close will fail silently - no exception is raised
			}
			else
			{
				if(CheckHKey())
				{
					//close the key
					int result = RegCloseKey(m_handle);

					if(result==0)
					{
						//set handle to invalid value
						m_handle = 0;
					}
					else
					{
						//error occured
						throw new ExternalException("Error closing RegistryKey");
					}
				}
			}
		}
		#endregion

		#region Create SubKey
		/// <summary>
		///  Creates a new subkey or opens an existing subkey.
		///  The string subKey is not case-sensitive.
		/// </summary>
		/// <remarks>This class was renamed from its previous name of "RegistryKey" to avoid naming clashes with the desktop equivalent, therefore the return type of this method has also been changed to <see cref="CERegistryKey"/>.</remarks>
		/// <param name="subkey">Name or path of subkey to create or open.</param>
		/// <returns>Returns the subkey, or null if the operation failed.</returns>
		/// <exception cref="System.ArgumentNullException">The specified subkey is null.</exception>
		/// <exception cref="System.ArgumentException">The length of the specified subkey is longer than the maximum length allowed (255 characters).</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="CERegistryKey"/> on which this method is being invoked is closed (closed keys cannot be accessed).</exception>
		public CERegistryKey CreateSubKey(string subkey)
		{
			//check handle is valid
			if(CheckHKey())
			{
				//check subkey is not null
				if(subkey!=null)
				{
					//check subkey length
					if(subkey.Length < 256)
					{
						//handle to new registry key
						uint newhandle = 0;

						//key disposition - did this create a new key or open an existing key
						uint kdisp = 0;

						//create new key
						int result = RegCreateKeyEx(m_handle, subkey, 0, null, 0, 0, IntPtr.Zero, ref newhandle, ref kdisp);

						if(result==0)
						{
							return new CERegistryKey(newhandle, m_name + "\\" + subkey, m_writable, false);
						}
						else
						{
							throw new ExternalException("An error occured creating the registry key.");
						}
					}
					else
					{
						throw new ArgumentException("The length of the specified subkey is longer than the maximum length allowed (255 characters).");
					}
				}
				else
				{
					throw new ArgumentNullException("The specified subkey is null.");
				}
			}
			else
			{
				throw new ObjectDisposedException("The CERegistryKey on which this method is being invoked is closed (closed keys cannot be accessed).");
			}
		}
		#endregion

		#region Open SubKey
		/// <summary>
		/// Retrieves a subkey as read-only.
		/// </summary>
		/// <remarks>This class was renamed from its previous name of "RegistryKey" to avoid naming clashes with the desktop equivalent, therefore the return type of this method has also been changed to <see cref="CERegistryKey"/>.</remarks>
		/// <param name="name">Name or path of subkey to open.</param>
		/// <returns>The subkey requested, or null if the operation failed.</returns>
		public CERegistryKey OpenSubKey(string name)
		{
			return OpenSubKey(name, false);
		}
		/// <summary>
		/// Retrieves a specified subkey.
		/// </summary>
		/// <remarks>This class was renamed from its previous name of "RegistryKey" to avoid naming clashes with the desktop equivalent, therefore the return type of this method has also been changed to <see cref="CERegistryKey"/>.</remarks>
		/// <param name="name">Name or path of subkey to open.</param>
		/// <param name="writable">Set to true if you need write access to the key.</param>
		/// <returns>The subkey requested, or null if the operation failed.</returns>
		/// <exception cref="System.ArgumentNullException">name is null.</exception>
		/// <exception cref="System.ArgumentException">The length of the specified subkey is longer than the maximum length allowed (255 characters).</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="CERegistryKey"/> being manipulated is closed (closed keys cannot be accessed).</exception>
		public CERegistryKey OpenSubKey(string name, bool writable)
		{
			//check handle is valid
			if(CheckHKey())
			{
				//check name is not null
				if(name!=null)
				{
					//check length
					if(name.Length < 256)
					{
						//handle to receive new key
						uint newhandle = 0;

						int result = RegOpenKeyEx(m_handle, name, 0, 0, ref newhandle);

						if(result==0)
						{
							return new CERegistryKey(newhandle, m_name + "\\" + name, writable, false);
						}
						else
						{
							//desktop model return null.
							//throw new ExternalException("An error occured retrieving the registry key");
							return null;
						}
					}
					else
					{
						throw new ArgumentException("The length of the specified subkey is longer than the maximum length allowed (255 characters).");
					}
				}
				else
				{
					throw new ArgumentNullException("name is null.");
				}
			}
			else
			{
				throw new ObjectDisposedException("The CERegistryKey being manipulated is closed (closed keys cannot be accessed).");
			}
		}
		#endregion

		#region Delete SubKey
		/// <summary>
		/// Deletes the specified subkey. The string subkey is not case-sensitive.
		/// </summary>
		/// <param name="subkey">Name of the subkey to delete.</param>
		/// <exception cref="System.ArgumentException">The specified subkey is not a valid reference to a registry key.</exception>
		/// <exception cref="System.ArgumentNullException">The subkey is null.</exception>
		public void DeleteSubKey(string subkey)
		{
			DeleteSubKey(subkey, true);
		}
		/// <summary>
		/// Deletes the specified subkey. The string subkey is not case-sensitive.
		/// </summary>
		/// <param name="subkey">Name of the subkey to delete.</param>
		/// <param name="throwOnMissingSubKey">Indicates whether an exception should be raised if the specified subkey cannot be found.
		/// If this argument is true and the specified subkey does not exist then an exception is raised.
		/// If this argument is false and the specified subkey does not exist, then no action is taken</param>
		/// <exception cref="System.ArgumentException">The specified subkey is not a valid reference to a registry key (and throwOnMissingSubKey is true).</exception>
		/// <exception cref="System.ArgumentNullException">The subkey is null.</exception>
		public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
		{
			if(subkey==null || subkey=="")
			{
				throw new ArgumentNullException("The subkey is null");
			}
			else
			{
				if(CheckHKey())
				{
					//delete the subkey
					int result = RegDeleteKey(m_handle, subkey);

					//if operation failed
					if(result != 0)
					{
						if(throwOnMissingSubKey)
						{
							throw new ArgumentException("The specified subkey is not a valid reference to a registry key");
						}
					}
				}
				else
				{
					//key is closed
					throw new ObjectDisposedException("The CERegistryKey on which this method is being invoked is closed (closed keys cannot be accessed).");
				}
			}
		}
		#endregion

		#region Delete SubKey Tree
		/// <summary>
		///  Deletes a subkey and any child subkeys recursively.
		///  The string subKey is not case-sensitive.
		/// </summary>
		/// <param name="subkey">Subkey to delete.</param>
		/// <exception cref="System.ArgumentNullException">The subkey parameter is null.</exception>
		/// <exception cref="System.ArgumentException">Deletion of a root hive is attempted. 
		/// The subkey parameter does not match a valid registry subkey.</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="CERegistryKey"/> being manipulated is closed (closed keys cannot be accessed).</exception>
		public void DeleteSubKeyTree(string subkey)
		{
			//call delete subkey - this will delete all sub keys autmoatically
			DeleteSubKey(subkey, true);
		}
		#endregion

		#region Get SubKey Names
		/// <summary>
		/// Retrieves an array of strings that contains all the subkey names.
		/// </summary>
		/// <returns>An array of strings that contains the names of the subkeys for the current key.</returns>
		/// <exception cref="System.ObjectDisposedException">The <see cref="CERegistryKey"/> being manipulated is closed (closed keys cannot be accessed).</exception>
		public string[] GetSubKeyNames()
		{
			if(CheckHKey())
			{
				//store the names
				System.Collections.ArrayList subkeynames = new System.Collections.ArrayList();
				int index = 0;
				//buffer to store the name
				char[] keyname = new char[256];
				int keynamelen = keyname.Length;

				//retrieve first key name
				int result = RegEnumKeyEx(m_handle, index, keyname, ref keynamelen, 0, null, 0, 0);

				//enumerate sub keys
				while(result != ERROR_NO_MORE_ITEMS)
				{
					//add the name to the arraylist
					subkeynames.Add(new string(keyname).Substring(0, keynamelen));

					//increment index
					index++;
					//reset length available to max
					keynamelen = keyname.Length;

					//retrieve next key name
					result = RegEnumKeyEx(m_handle, index, keyname, ref keynamelen, 0, null, 0, 0);

				}

				//sort the results
				subkeynames.Sort();
				
				//return a fixed size string array
				return (string[])subkeynames.ToArray(typeof(string));
			}
			else
			{
				throw new ObjectDisposedException("The CERegistryKey being manipulated is closed (closed keys cannot be accessed).");
			}
		}
		#endregion


		#region Get Value
		/// <summary>
		/// Retrieves the data associated with the specified value, or null if the value does not exist.
		/// </summary>
		/// <param name="name">Name of the value to retrieve.</param>
		/// <returns>The data associated with name , or null if the value does not exist.</returns>
		/// <exception cref="System.ArgumentException">The RegistryKey being manipulated does not exist.</exception>
		/// <exception cref="System.ObjectDisposedException">The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public object GetValue(string name)
		{
			return GetValue(name, null);
		}
		/// <summary>
		/// Retrieves the specified value, or the default value you provide if the specified value is not found. 
		/// </summary>
		/// <param name="name">Name of the value to retrieve.</param>
		/// <param name="defaultValue">Value to return if name does not exist.</param>
		/// <returns>The data associated with name, or defaultValue if name is not found.</returns>
		/// <exception cref="System.ArgumentException">The RegistryKey being manipulated does not exist.</exception>
		/// <exception cref="System.ObjectDisposedException">The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public object GetValue(string name, object defaultValue)
		{
			if(CheckHKey())
			{
				KeyType kt = 0;
				//support up to 256 characters
				byte[] buffer;

				//pass in buffer size
				int size = 0;

				//determine validity and get required buffer size
				int result = RegQueryValueEx(m_handle, name, 0, ref kt, null, ref size);

				//catch value name not valid
				if (result == 87)
				{
					return defaultValue;
				}

				//call api again with valid buffer size
				buffer = new byte[size];
				result = RegQueryValueEx(m_handle, name, 0, ref kt, buffer, ref size);

				//return appropriate type of value
				switch(kt)
				{
					case KeyType.Binary:
						//return binary data (byte[])
						return buffer;

					case KeyType.DWord:
                        if (size == 4)
                        {
                            //return value as dword (UInt32)
                            return System.BitConverter.ToUInt32(buffer, 0);
                        }
                        else if (size > 0)
                        {
                            int val = buffer[buffer.Length - 1];
                            for (int i = buffer.Length - 2; i >= 0; i--)
                            {
                                val *= 255 * buffer[i];
                            }
                            return val;
                        }
                            else
                            {
                                return 0;
                            }
					case KeyType.ExpandString:
					case KeyType.String:
						//return value as a string (trailing null removed)
						return System.Text.Encoding.Unicode.GetString(buffer, 0, size).TrimEnd('\0');

					case KeyType.MultiString:
						//get string of value
						string raw = System.Text.Encoding.Unicode.GetString(buffer, 0, size).TrimEnd('\0');
						//return array of substrings between single nulls
						return raw.Split('\0');
					default:
						return defaultValue;
				}

			}
			else
			{
				throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
			}
		}
		#endregion

		#region Set Value
		/// <summary>
		/// Sets the specified value.
		/// </summary>
		/// <param name="name">Name of value to store data in.</param>
		/// <param name="value">Data to store.</param>
		/// <exception cref="System.ArgumentException">The length of the specified value is longer than the maximum length allowed (255 characters).</exception>
		/// <exception cref="System.ArgumentNullException">value is null.</exception>
		/// <exception cref="System.ObjectDisposedException">The RegistryKey being set is closed (closed keys cannot be accessed).</exception>
		public void SetValue(string name, object value)
		{
			if(CheckHKey())
			{
				KeyType type = 0;
				byte[] data;

				switch(value.GetType().ToString())
				{
					case "System.String":
						type = KeyType.String;

						// add null terminator
						data = System.Text.Encoding.Unicode.GetBytes((string)value + '\0');
						
						break;
					case "System.String[]":
						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						foreach (string str in (string[])value)
							sb.Append(str + '\0');
						sb.Append('\0'); // terminated by two null characters
						type = KeyType.MultiString;
						data = System.Text.Encoding.Unicode.GetBytes(sb.ToString());
						break;

					case "System.Byte[]":
						type = KeyType.Binary;
						data = (byte[])value;
						break;
					case "System.Int32":
						type = KeyType.DWord;
						data = BitConverter.GetBytes((int)value);
						break;
					case "System.UInt32":
						type = KeyType.DWord;
						data = BitConverter.GetBytes((uint)value);
						break;
					default:
						throw new ArgumentException("value is not a supported type");
				}

				int size = data.Length;
				
				int result = RegSetValueEx(m_handle, name, 0, type, data, size);


				if(result!=0)
				{
					throw new ExternalException("Error writing to the RegistryKey");
				}
			}
			else
			{
				throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
			}
		}
		#endregion

		#region Delete Value
		/// <summary>
		/// Deletes the specified value from this key.
		/// </summary>
		/// <param name="name">Name of the value to delete.</param>
		public void DeleteValue(string name)
		{
			DeleteValue(name, true);
		}
		/// <summary>
		/// Deletes the specified value from this key.
		/// </summary>
		/// <param name="name">Name of the value to delete.</param>
		/// <param name="throwOnMissingValue">Indicates whether an exception should be raised if the specified value cannot be found.
		/// If this argument is true and the specified value does not exist then an exception is raised.
		/// If this argument is false and the specified value does not exist, then no action is taken</param>
		/// <exception cref="System.ArgumentException">name is not a valid reference to a value (and throwOnMissingValue is true) or name is null</exception>
		/// <exception cref="System.ObjectDisposedException">The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public void DeleteValue(string name, bool throwOnMissingValue)
		{
			if(m_writable)
			{
			
				if(CheckHKey())
				{
					if(name==null)
					{
						throw new ArgumentException("name is null");
					}
					else
					{
						//call api function to delete value
						int result = RegDeleteValue(m_handle, name);

						//check for error in supplied name
						if(result==87)
						{
							//only throw exception if flag is set
							if(throwOnMissingValue)
							{
								throw new ArgumentException("name is not a valid reference to a value (and throwOnMissingValue is true) or name is null");
							}
						}
					}
				}
				else
				{
					throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
				}
			}
			else
			{
				//key is readonly throw exception
				throw new UnauthorizedAccessException("Cannot delete a value from a RegistryKey opened as ReadOnly.");
			}
		}
		#endregion

		#region GetValueNames
		/// <summary>
		/// Retrieves an array of strings that contains all the value names associated with this key.
		/// </summary>
		/// <returns>An array of strings that contains the value names for the current key.</returns>
		/// <exception cref="System.ObjectDisposedException">The RegistryKey being manipulated is closed (closed keys cannot be accessed).</exception>
		public string[] GetValueNames()
		{
			if(CheckHKey())
			{
				//store the names
				ArrayList valuenames = new ArrayList();

				int index = 0;
				//buffer to store the name
				char[] valuename = new char[256];
				int valuenamelen = valuename.Length;
                
				//enumerate sub keys
				while(RegEnumValue(m_handle, index, valuename, ref valuenamelen, 0, 0, null, 0)!=ERROR_NO_MORE_ITEMS)
				{
					//add the name to the arraylist
					valuenames.Add(new string(valuename, 0, valuenamelen));
					//increment index
					index++;
					//reset length available to max
					valuenamelen = valuename.Length;
				}

				//sort the results
				valuenames.Sort();
				
				//return a fixed size string array
				return (string[])valuenames.ToArray(typeof(string));
			}
			else
			{
				throw new ObjectDisposedException("The RegistryKey being manipulated is closed (closed keys cannot be accessed).");
			}
		}
		#endregion


		#region CheckHKey
		//used to check that the handle is a valid open hkey
		private bool CheckHKey()
		{
			if(m_handle==0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		#endregion

		#region IDisposable Members

		/// <summary>
		/// Dispose method
		/// </summary>
		public void Dispose()
		{
			//close and save out data
			this.Close();
		}

		#endregion

		#region Enums
		/// <summary>
		/// Key disposition for RegCreateKey(Ex)
		/// </summary>
		private enum KeyDisposition : int 
		{
			REG_CREATED_NEW_KEY = 1, 
			REG_OPENED_EXISTING_KEY = 2 
		} 

		/// <summary>
		/// Key type for RegCreateKey(Ex)
		/// </summary>
		private enum KeyType : int
		{
			//String
			String = 1,
			ExpandString = 2,
			//binary data (byte[])
			Binary = 3,
			//dword (UInt32)
			DWord = 4,
			//Multi String
			MultiString = 7,
		}
		#endregion

		#region Registry P/Invokes

		[DllImport("rapi.dll", EntryPoint="CeRegOpenKeyEx", CharSet=CharSet.Unicode, SetLastError=true)] 
		private static extern int RegOpenKeyEx(
			uint hKey,
			string lpSubKey,
			int ulOptions,
			int samDesired,
			ref uint phkResult); 

		[DllImport("rapi.dll", EntryPoint="CeRegCreateKeyEx", CharSet=CharSet.Unicode, SetLastError=true)] 
		private static extern int RegCreateKeyEx(
			uint hKey,
			string lpSubKey,
			int lpReserved,
			string lpClass,
			int dwOptions,
			int samDesired,
			IntPtr lpSecurityAttributes,
			ref uint phkResult, 
			ref uint lpdwDisposition); 

		[DllImport("rapi.dll", EntryPoint="CeRegEnumKeyEx", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern int RegEnumKeyEx(
			uint hKey,
			int iIndex, 
			char[] sKeyName,
			ref int iKeyNameLen, 
			int iReservedZero,
			byte[] sClassName,
			int iClassNameLenZero, 
			int iFiletimeZero);

		[DllImport("rapi.dll", EntryPoint="CeRegEnumValue", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern int RegEnumValue(
			uint hKey,
			int iIndex,
			char[] sValueName, 
			ref int iValueNameLen,
			int iReservedZero,
			int iTypeZero, /*should take ref KeyType but we never want to restrict type when enumerating values*/
			byte[] byData,
			int iDataLenZero /*takes ref int but we dont need the value when enumerating the names*/);

		[DllImport("rapi.dll", EntryPoint="CeRegQueryInfoKey", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern int RegQueryInfoKey(
			uint hKey,
			char[] lpClass,
			ref int lpcbClass, 
			int reservedZero,
			out int cSubkey, 
			out int iMaxSubkeyLen,
			out int lpcbMaxSubkeyClassLen,
			out int cValueNames,
			out int iMaxValueNameLen, 
			out int iMaxValueLen,
			int securityDescriptorZero,
			int lastWriteTimeZero);

		[DllImport("rapi.dll", EntryPoint="CeRegQueryValueEx", CharSet=CharSet.Unicode, SetLastError=true)] 
		private static extern int RegQueryValueEx(
			uint hKey,
			string lpValueName,
			int lpReserved, 
			ref KeyType lpType,
			byte[] lpData,
			ref int lpcbData); 

		[DllImport("rapi.dll", EntryPoint="CeRegSetValueEx", CharSet=CharSet.Unicode, SetLastError=true)] 
		private static extern int RegSetValueEx(
			uint hKey,
			string lpValueName,
			int lpReserved, 
			KeyType lpType,
			byte[] lpData,
			int lpcbData); 

		[DllImport("rapi.dll", EntryPoint="CeRegCloseKey", SetLastError=true)] 
		private static extern int RegCloseKey(
			uint hKey);

		[DllImport("rapi.dll", EntryPoint="CeRegDeleteKey", SetLastError=true)]
		private static extern int RegDeleteKey(
			uint hKey,
			string keyName);

		[DllImport("rapi.dll", EntryPoint="CeRegDeleteValue", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern int RegDeleteValue(
			uint hKey,
			string valueName);

		/*[DllImport("rapi.dll", EntryPoint="CeRegFlushKey", SetLastError=true)]
		private static extern int RegFlushKey(
			uint hKey );*/


		#endregion
	}
}
