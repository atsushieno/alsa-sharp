using System;
using System.Runtime.InteropServices;

namespace AlsaSharp {
	public class AlsaClientInfo : IDisposable {
		static IntPtr Malloc ()
		{
			unsafe {
				var ptr = IntPtr.Zero;
				var pref = &ptr;
				Natives.snd_seq_client_info_malloc ((IntPtr)pref);
				return ptr;
			}
		}

		static void Free (IntPtr handle)
		{
			if ((IntPtr)handle != IntPtr.Zero)
				Natives.snd_seq_client_info_free (handle);
		}

		public AlsaClientInfo ()
			: this (Malloc (), Free)
		{
		}

		public AlsaClientInfo (IntPtr handle, Action<IntPtr> free)
		{
			this.handle = handle;
			this.free = free;
		}

		Pointer<snd_seq_client_info_t> handle;
		Action<IntPtr> free;

		internal IntPtr Handle => handle;

		public void Dispose ()
		{
			if ((IntPtr)handle != IntPtr.Zero) {
				Natives.snd_seq_client_info_free (handle);
				handle = IntPtr.Zero;
			}
		}

		public int Client {
			get => Natives.snd_seq_client_info_get_client (handle);
			set => Natives.snd_seq_client_info_set_client (handle, value);
		}
		public AlsaClientType ClientType => (AlsaClientType)Natives.snd_seq_client_info_get_type (handle);
		IntPtr name_ptr;
		public string Name {
			get => Marshal.PtrToStringAnsi (Natives.snd_seq_client_info_get_name (handle));
			set {
				if (name_ptr != IntPtr.Zero)
					Marshal.FreeHGlobal (name_ptr);
				name_ptr = Marshal.StringToHGlobalAnsi (value);
				Natives.snd_seq_client_info_set_name (handle, name_ptr);
			}
		}
		public int BroadcastFilter {
			get => Natives.snd_seq_client_info_get_broadcast_filter (handle);
			set => Natives.snd_seq_client_info_set_broadcast_filter (handle, value);
		}
		public int ErrorBounce {
			get => Natives.snd_seq_client_info_get_error_bounce (handle);
			set => Natives.snd_seq_client_info_set_error_bounce (handle, value);
		}
		public int Card => Natives.snd_seq_client_info_get_card (handle);
		public int Pid => Natives.snd_seq_client_info_get_pid (handle);
		public int PortCount => Natives.snd_seq_client_info_get_num_ports (handle);
		public int EventLostCount => Natives.snd_seq_client_info_get_event_lost (handle);

		public void ClearEventFilter () => Natives.snd_seq_client_info_event_filter_clear (handle);
		public void AddEventFilter (int eventType) => Natives.snd_seq_client_info_event_filter_add (handle, eventType);
		public void DeleteEventFilter (int eventType) => Natives.snd_seq_client_info_event_filter_del (handle, eventType);
		public bool IsEventFiltered (int eventType) => Natives.snd_seq_client_info_event_filter_check (handle, eventType) > 0;
	}
}
