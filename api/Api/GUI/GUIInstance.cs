using System;
using SynthesisAPI.Runtime;

namespace Api.GUI
{
	public abstract class GUIInstance : IDisposable
	{
		private  bool _disposed    = false;
		private  bool _initialized = false;
		internal int  _id;

		internal void SetId(int id)
		{
			if (!_initialized)
			{
				_id = id;
				_initialized = true;
			}
			else
			{
				throw new Exception("Object already initialized");
			}
		}

		internal bool IsInitialized => _initialized;
		internal bool IsDisposed => _disposed;


		public void Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				ApiProvider.GetGUIManager().Free(_id);
			}
		}
	}
}