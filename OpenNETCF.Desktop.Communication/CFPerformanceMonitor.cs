/*=======================================================================================
	OpenNETCF.Desktop.Communication.CFPerformanceMonitor

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
using System.IO;

namespace OpenNETCF.Desktop.Communication
{
	/// <summary>
	/// This class can be used to generate and capture performance statistics
	/// from .NET Compact Framework applications run on a connected device.
	/// </summary>
	public class CFPerformanceMonitor
	{
		PerformanceStatistics	m_stats;
		RAPI					m_rapi;
		static string			m_perfkey = @"SOFTWARE\Microsoft\.NETCompactFramework\PerfMonitor";

		internal CFPerformanceMonitor(RAPI rapi)
		{
			m_stats = new PerformanceStatistics();
			m_rapi = rapi;
		}

		/// <summary>
		/// This method informs the device to begin profiling all managed applications
		/// </summary>
		public void EnableProfiling()
		{
			m_rapi.CheckConnection();

			CERegistryKey key = CERegistry.LocalMachine.CreateSubKey(m_perfkey);
			key.SetValue("Counters", 1);
		}

		/// <summary>
		/// This method informs the device to stop profiling all managed applications
		/// </summary>
		public void DisableProfiling()
		{
			m_rapi.CheckConnection();

			CERegistryKey key = CERegistry.LocalMachine.CreateSubKey(m_perfkey);
			key.SetValue("Counters", 0);
		}

		/// <summary>
		/// Retrieves the statistics for the last profiled managed application
		/// <seealso cref="PerformanceStatistics"/>
		/// </summary>
		/// <returns>Profile statistics</returns>
		public PerformanceStatistics GetCurrentStatistics()
		{
			m_rapi.CheckConnection();
			GetStats();

			return m_stats;
		}

		private void GetStats()
		{
			string localpath = System.Windows.Forms.Application.StartupPath + "\\mscoree.stat";
			string line;

			m_rapi.CopyFileFromDevice("\\mscoree.stat", localpath, true);

			StreamReader reader = System.IO.File.OpenText(localpath);

			line = reader.ReadLine();

			while(line != null)
			{
				// there are a couple blank lines
				if(line.Length > 1)
				{
					// skip the "header" line
					if(line.Substring(0, 7) != "counter")
					{
						// get name
						string name = line.Substring(0, 47).Trim();
						int val =		Convert.ToInt32(line.Substring(46, 11).Trim(), 10);
						int samples =	Convert.ToInt32(line.Substring(57, 9).Trim(), 10);
						int mean =		Convert.ToInt32(line.Substring(66, 9).Trim(), 10);
						int min =		Convert.ToInt32(line.Substring(75, 9).Trim(), 10);
						int max =		Convert.ToInt32(line.Substring(84, 9).Trim(), 10);

						m_stats.Add(new PerformanceStatistic(name, val, samples, mean, min, max));
					}
				}
				line = reader.ReadLine();
			}
			reader.Close();
			File.Delete(localpath);
		}
	}
}
