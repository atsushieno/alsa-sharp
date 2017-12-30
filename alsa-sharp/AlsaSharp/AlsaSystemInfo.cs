using System;

namespace AlsaSharp {
	public class AlsaSystemInfo : IDisposable {
		public AlsaSystemInfo ()
		{
			IntPtr ptr = IntPtr.Zero;
			unsafe {
				var pref = &ptr;
				Natives.snd_seq_system_info_malloc ((IntPtr)pref);
				handle = ptr;
			}
		}

		public void SetContextSequencer (AlsaSequencer seq)
		{
			Natives.snd_seq_system_info (seq.SequencerHandle, handle);
		}

		public int MaxQueueCount => Natives.snd_seq_system_info_get_queues (handle);
		public int MaxClientCount => Natives.snd_seq_system_info_get_clients (handle);
		public int PortCount => Natives.snd_seq_system_info_get_ports (handle);
		public int ChannelCount => Natives.snd_seq_system_info_get_channels (handle);
		public int CurrentQueueCount => Natives.snd_seq_system_info_get_cur_queues (handle);
		public int CurrentClientCount => Natives.snd_seq_system_info_get_cur_clients (handle);

		Pointer<snd_seq_system_info_t> handle;

		public void Dispose ()
		{
			if ((IntPtr)handle != IntPtr.Zero)
				Natives.snd_seq_system_info_free (handle);
			handle = IntPtr.Zero;
		}
	}
}
