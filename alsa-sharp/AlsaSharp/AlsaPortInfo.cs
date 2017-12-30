using System;
using System.Runtime.InteropServices;

namespace AlsaSharp {
	public class AlsaPortInfo : IDisposable {
		public const int PortSystemTimer = 0;
		public const int PortSystemAnnouncement = 1;

		static IntPtr Malloc ()
		{
			unsafe {
				var ptr = IntPtr.Zero;
				var pref = &ptr;
				Natives.snd_seq_port_info_malloc ((IntPtr)pref);
				return ptr;
			}
		}

		static void Free (IntPtr handle)
		{
			if ((IntPtr)handle != IntPtr.Zero)
				Natives.snd_seq_port_info_free (handle);
		}

		public AlsaPortInfo ()
			: this (Malloc (), Free)
		{
		}

		public AlsaPortInfo (IntPtr handle, int port)
		{
			this.handle = handle;
			this.free = _ => {};
		}

		public AlsaPortInfo (IntPtr handle, Action<IntPtr> free)
		{
			this.handle = handle;
			this.free = free;
		}

		Pointer<snd_seq_port_info_t> handle;
		Action<IntPtr> free;

		internal IntPtr Handle => handle;

		public void Dispose ()
		{
			if (name_ptr != IntPtr.Zero)
				Marshal.FreeHGlobal (name_ptr);
			if ((IntPtr)handle != IntPtr.Zero)
				free (handle);
			handle = IntPtr.Zero;
		}

		public AlsaPortInfo Clone ()
		{
			var ret = new AlsaPortInfo ();
			Natives.snd_seq_port_info_copy (ret.Handle, Handle);
			return ret;
		}

		public int Client {
			get => Natives.snd_seq_port_info_get_client (handle);
			set => Natives.snd_seq_port_info_set_client (handle, value);
		}

		public int Port {
			get => Natives.snd_seq_port_info_get_port (handle);
			set => Natives.snd_seq_port_info_set_port (handle, value);
		}

		IntPtr name_ptr;
		public string Name {
			get => Marshal.PtrToStringAnsi (Natives.snd_seq_port_info_get_name (handle));
			set {
				if (name_ptr != IntPtr.Zero)
					Marshal.FreeHGlobal (name_ptr);
				name_ptr = Marshal.StringToHGlobalAnsi (value);
				Natives.snd_seq_port_info_set_name (handle, name_ptr);
			}
		}

		public AlsaPortCapabilities Capabilities {
			get => (AlsaPortCapabilities)Natives.snd_seq_port_info_get_capability (handle);
			set => Natives.snd_seq_port_info_set_capability (handle, (uint)value);
		}

		public AlsaPortType PortType {
			get => (AlsaPortType)Natives.snd_seq_port_info_get_type (handle);
			set => Natives.snd_seq_port_info_set_type (handle, (uint)value);
		}

		public int MidiChannels {
			get => Natives.snd_seq_port_info_get_midi_channels (handle);
			set => Natives.snd_seq_port_info_set_midi_channels (handle, value);
		}

		public int MidiVoices {
			get => Natives.snd_seq_port_info_get_midi_voices (handle);
			set => Natives.snd_seq_port_info_set_midi_voices (handle, value);
		}

		public int SynthVoices {
			get => Natives.snd_seq_port_info_get_synth_voices (handle);
			set => Natives.snd_seq_port_info_set_synth_voices (handle, value);
		}

		public int ReadSubscriptions => Natives.snd_seq_port_info_get_read_use (handle);

		public int WriteSubscriptions => Natives.snd_seq_port_info_get_write_use (handle);

		public bool PortSpecified {
			get => Natives.snd_seq_port_info_get_port_specified (handle) > 0;
			set => Natives.snd_seq_port_info_set_port_specified (handle, value ? 1 : 0);
		}

		public int TimestampQueue {
			get => Natives.snd_seq_port_info_get_timestamp_queue (handle);
			set => Natives.snd_seq_port_info_set_timestamp_queue (handle, value);
		}

		public int TimestampReal {
			get => Natives.snd_seq_port_info_get_timestamp_real (handle);
			set => Natives.snd_seq_port_info_set_timestamp_real (handle, value);
		}

		public bool Timestamping {
			get => Natives.snd_seq_port_info_get_timestamping (handle) != 0;
			set => Natives.snd_seq_port_info_set_timestamping (handle, value ? 1 : 0);
		}

		public string Id => Client.ToString () + '_' + Port.ToString ();

		public string Manufacturer => string.Empty; // FIXME: implement

		public string Version => string.Empty; // FIXME: implement
	}
}
