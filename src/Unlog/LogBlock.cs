using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlog
{
	public class LogBlock : IDisposable
	{
		private bool _WasDisposed = false;

		public bool Keep = true;

		public void Dispose ()
		{
			if (!_WasDisposed) {
				_WasDisposed = true;
				Log.DoLeave (Keep);
			}
		}
	}
}
