/*=======================================================================================
	OpenNETCF.Desktop.Communication.PerformanceStatistics
	OpenNETCF.Desktop.Communication.PerformanceStatistic

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
	/// A collection of performance statistics for managed applications
	/// <seealso cref="CFPerformanceMonitor"/>
	/// </summary>
	public class PerformanceStatistics : IEnumerable
	{
		private ArrayList items;

		internal PerformanceStatistics()
		{
			items = new ArrayList();
		}

		internal void Add(PerformanceStatistic stat)
		{
			items.Add(stat);
		}

		internal void Clear()
		{
			items.Clear();
		}

		/// <summary>
		/// Number of statistices in the collection
		/// </summary>
		public int Count
		{
			get {return items.Count;}
		}

		#region IEnumerable Members
		/// <summary>
		///  Gets an enumerator for iterating the PerformanceStatistics collection
		///  <seealso cref="StatisticEnumerator"/>
		/// </summary>
		/// <returns>A StatisticEnumerator</returns>
		public StatisticEnumerator GetEnumerator()
		{
			return new StatisticEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}		
		#endregion

		#region --- StatisticEnumerator ---
		/// <summary>
		/// The enumerator for the PerformanceStatistics class
		/// <seealso cref="PerformanceStatistics"/> 
		/// </summary>
		public class StatisticEnumerator : IEnumerator
		{
			int index;
			PerformanceStatistics m_stats;

			internal StatisticEnumerator(PerformanceStatistics stats) 
			{
				m_stats = stats;
				index = -1;
			}

			/// <summary>
			/// Reset
			/// </summary>
			public void Reset() 
			{
				index = -1;
			}

			/// <summary>
			/// MoveNext
			/// </summary>
			/// <returns></returns>
			public bool MoveNext() 
			{
				index++;
				return(index < m_stats.items.Count);
			}

			/// <summary>
			/// The current PerformanceStatistic pointed to
			/// </summary>
			public PerformanceStatistic Current 
			{
				get 
				{
					return((PerformanceStatistic)m_stats.items[index]);
				}
			}

			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}		
		}
		#endregion

	}

	/// <summary>
	/// A single statistic for a profiled application
	/// <seealso cref="CFPerformanceMonitor"/>
	/// </summary>
	public class PerformanceStatistic
	{
		private string	m_name;
		private int		m_value;
		private int		m_sampleCount;
		private int		m_mean;
		private int		m_min;
		private int		m_max;

		internal PerformanceStatistic(string name, int val, int sampleCount, int mean, int min, int max)
		{
			m_name = name;
			m_value = val;
			m_sampleCount = sampleCount;
			m_mean = mean;
			m_min = min;
			m_max = max;
		}

		/// <summary>
		/// The statistic name
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// The statistic value
		/// </summary>
		public int Value
		{
			get { return m_value; }
		}

		/// <summary>
		/// The number of samples used to determine the Min, Max and Mean
		/// </summary>
		public int SampleCount
		{
			get { return m_sampleCount; }
		}

		/// <summary>
		/// Average statistic value
		/// </summary>
		public int Mean
		{
			get { return m_mean; }
		}

		/// <summary>
		/// Minimum statistic value
		/// </summary>
		public int Min
		{
			get { return m_min; }
		}

		/// <summary>
		/// Maximum statistic value
		/// </summary>
		public int Max
		{
			get { return m_max; }
		}

	}
}
