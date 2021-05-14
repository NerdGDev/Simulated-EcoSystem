using UnityEngine;

namespace Kit
{
	public sealed class DisposObject : MonoBehaviour, System.IDisposable
	{
		public static DisposObject Create(string _name, HideFlags _hideFlags = HideFlags.None)
		{
			GameObject obj = new GameObject(_name);
			obj.hideFlags = _hideFlags;
			DisposObject self = obj.AddComponent<DisposObject>();
			return self;
		}

		private void OnDestroy()
		{
			Dispose();
		}

		private void OnApplicationQuit()
		{
			Dispose();
		}

		#region IDisposable Support
		private bool IsDisposed = false; // To detect redundant calls

		private void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					Destroy(gameObject);
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				IsDisposed = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		//~DisposObject()
		//{
		//	 Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//	Dispose(false);
		//}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

	}
}