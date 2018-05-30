using System;
using System.Runtime.InteropServices;

namespace AlsaSharp {
	public class AlsaException : Exception {

		static string GetErrorMessage (int errorCode)
		{
			var ptr = Natives.snd_strerror (errorCode);
			return ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi (ptr) : null;
		}

		public AlsaException (int errorCode, Exception innerException = null)
			: this ($"ALSA exception (error code = {errorCode}) : {GetErrorMessage (errorCode)}", innerException)
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
