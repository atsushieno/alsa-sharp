using System;

namespace AlsaSharp {
	public class AlsaException : Exception {
		public AlsaException (int errorCode, Exception innerException = null)
			: this ($"ALSA exception (error code = {errorCode})", innerException)
		{
		}

		public AlsaException (string message)
			: base (message)
		{
		}

		public AlsaException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
