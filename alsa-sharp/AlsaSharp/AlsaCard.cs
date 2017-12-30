using System;
using System.Runtime.InteropServices;

namespace AlsaSharp {
	public class AlsaCard {
		public static string GetCardName (int card)
		{
			unsafe {
				IntPtr ptr = IntPtr.Zero;
				var pref = &ptr;
				Natives.snd_card_get_name (card, (IntPtr)pref);
				return Marshal.PtrToStringAnsi (ptr);
			}
		}
	}
}
