namespace Kit
{
	public class SingletonRaw<T> : SingletonRawBase
		where T : SingletonRaw<T>, new()
	{
		private static T m_Instance = null;
		public static T Instance
		{
			get
			{
				if(m_Instance == null)
				{
					m_Instance = new T();
					m_Instance.Awake();
				}
				return m_Instance;
			}
		}
	}

	public class SingletonRawBase
	{
		protected virtual void Awake() { }
	}
}