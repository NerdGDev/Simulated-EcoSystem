using Debug = UnityEngine.Debug;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

namespace Kit
{
	/// <summary>
	/// Calculate current & next session
	/// and trigger EVENT_SessionChanged when change detected.
	/// </summary>
	/// <remarks>you need to update the current time by yourself.</remarks>
	/// <example>m_TimeSession.current = System.DateTime.UtcNow;</example>
	public class TimeSession
	{
		/// <summary>The different between 2 different time, and how much session across,
		/// Warning can not handle back in time issue.</summary>
		/// <param name="from">Older time reference</param>
		/// <param name="to">Newer time reference</param>
		/// <param name="diff">can only handle the different smaller then int.MaxValue, return -1 for any backward time travel.</param>
		public delegate void TimeCrossSession(DateTime from, DateTime to, int diff);
		public event TimeCrossSession EVENT_SessionChanged;

		#region variables, getter, constructor
		private DateTime m_Current;
		/// <summary>The current time in record, update this may trigger <see cref="EVENT_SessionChanged"/>.</summary>
		public DateTime current { get { return m_Current; } set { TriggerEventIfCrossSession(value); } }
		/// <summary>Define the start session based on giving time.</summary>
		public DateTime timeAnchor { get; private set; }
		/// <summary>The offset from last midnight, used to define <see cref="timeAnchor"/></summary>
		private TimeSpan m_Offset;
		/// <summary>Next time anchor that will trigger <see cref="EVENT_SessionChanged"/></summary>
		public DateTime nextSession { get; private set; }
		/// <summary>Last time anchor that triggered event.</summary>
		public DateTime lastSession { get; private set; }
		/// <summary>Define how long of each session.</summary>
		public TimeSpan oneSession { get; private set; }
		/// <summary>an index number calculate by the number of session cross since <see cref="timeAnchor"/> to <see cref="current"/></summary>
		public int index { get; private set; }

		public TimeSession(DateTime _current, TimeSpan _oneSession, TimeSpan _offset = default(TimeSpan))
		{
			ReInit(_current, _oneSession, _offset);
		}
		#endregion

		#region API
		public void ReInit(DateTime _current, TimeSpan _OneSession, TimeSpan _offset = default(TimeSpan))
		{
			if (_OneSession.TotalDays > 1)
				throw new System.InvalidOperationException("Can not more then one day.");
			if (_OneSession <= TimeSpan.Zero)
				throw new System.InvalidOperationException("You need a positive timespan more than Zero.");
			if (_offset == default(TimeSpan))
				_offset = TimeSpan.Zero;

			m_Current = _current;
			m_Offset = _offset;
			oneSession = _OneSession;

			CalculateSession();
		}
		
		public static readonly DateTime SECOND_DAY_OF_CREATION = DateTime.MinValue.AddDays(1f);
		/// <summary>The time anchor set to UTC 00:00, locate the previous midnight based on giving time.</summary>
		/// <param name="utc"></param>
		/// <param name="offset">time offset will added in to the result</param>
		/// <returns></returns>
		public static DateTime PrevMidNight(DateTime utc, TimeSpan offset = default(TimeSpan))
		{
			// Check if it less then one day, just return the min value for the answer.
			if (utc.Ticks <= SECOND_DAY_OF_CREATION.Ticks)
				return DateTime.MinValue;

			if (offset == default(TimeSpan))
				offset = TimeSpan.Zero;

			DateTime midnight = utc.Date.Add(offset);
			if ((utc - midnight).TotalDays >= 1)
				midnight = midnight.AddDays(1);
			return midnight;
		}
		#endregion

		/// <summary>Update current time, and try to trigger event.</summary>
		/// <param name="newCurrent"></param>
		private void TriggerEventIfCrossSession(DateTime newCurrent)
		{
			if (newCurrent == m_Current)
				return;

			DateTime oldCurrent = m_Current;
			m_Current = newCurrent;
			if (lastSession < newCurrent && newCurrent < nextSession)
			{
				// Debug.Log("Within session");
			}
			else if (newCurrent >= nextSession)
			{
				// Debug.Log("Pass session detected.");
				int diff = 0;
				DateTime oldNextSession = nextSession;
				CalculateSession();
				while (m_Current > oldNextSession && diff < int.MaxValue)
				{
					oldNextSession = oldNextSession.Add(oneSession);
					diff++;
				}

				Debug.LogFormat("Session updated: <color=green>Diff {0}</color>\n\rLast Current {1:G}\n\rCurrent {2:G}, \n\n\rLast Session {3:G}\n\rNext Session {4:G}\n\n\rOne Session {5:c}\n", diff, oldCurrent, m_Current, lastSession, nextSession, oneSession);
				if (EVENT_SessionChanged != null)
					EVENT_SessionChanged(oldCurrent, m_Current, diff);

			}
			// The latest time(newCurrent) are smaller than the record that we had, back to the future.
			else // if (newCurrent < oldCurrent)
			{
				CalculateSession();
				if (newCurrent < lastSession)
					Debug.LogErrorFormat("Time traveler detected := Session Jumpped.\ncurrent : {0:G} -> {1:G},\nLast session : {2:G}\nNext Session : {3:G}\nOne session : {4:c}", m_Current, newCurrent, lastSession, nextSession, oneSession);
				else
					Debug.LogWarningFormat("Time traveler detected := within session.\ncurrent : {0:G} -> {1:G},\nLast session : {2:G}\nNext Session : {3:G}\nOne session : {4:c}", m_Current, newCurrent, lastSession, nextSession, oneSession);

				if (EVENT_SessionChanged != null)
					EVENT_SessionChanged(oldCurrent, m_Current, -1);
			}
		}

		/// <summary>Get next/last session & session index, based on all giving values.</summary>
		private void CalculateSession()
		{
			timeAnchor = PrevMidNight(m_Current, m_Offset);
			nextSession = timeAnchor.Add(oneSession);
			index = 0;

			while (m_Current > nextSession)
			{
				nextSession = nextSession.Add(oneSession);
				index++;
			}
			lastSession = nextSession.Add(-oneSession);
		}

		public override string ToString()
		{
			return string.Format("Time session :=\n\rOne session: {0:c}\n\rTime Anchor : {1:G}\n\rCurrent Time : {2:G}\n\rLast session: {3:G}\n\rNext session: {4:G}\n\rSession Index : {5}\n\n",
				oneSession, timeAnchor, m_Current, lastSession, nextSession, index);
		}
	}
}