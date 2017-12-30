using System;
using System.Runtime.InteropServices;

namespace AlsaSharp {
	public class AlsaPortSubscription
	{
		public class Address
		{
			public Address (IntPtr handle)
			{
				this.handle = handle;
			}

			IntPtr handle;

			public IntPtr Handle => handle;

			public byte Client {
				get => Marshal.ReadByte (handle, 0);
				set => Marshal.WriteByte (handle, 0, value);
			}

			public byte Port {
				get => Marshal.ReadByte (handle, 1);
				set => Marshal.WriteByte (handle, 1, value);
			}

			public override string ToString()
			{
				return $"ch{Client}_port{Port}";
			}
		}
		
		static IntPtr Malloc ()
		{
			unsafe {
				var ptr = IntPtr.Zero;
				var pref = &ptr;
				Natives.snd_seq_port_subscribe_malloc ((IntPtr)pref);
				return ptr;
			}
		}

		static void Free (IntPtr handle)
		{
			if ((IntPtr)handle != IntPtr.Zero)
				Natives.snd_seq_port_subscribe_free (handle);
		}

		public AlsaPortSubscription ()
			: this (Malloc (), Free)
		{
		}

		public AlsaPortSubscription (IntPtr handle, Action<IntPtr> free)
		{
			this.handle = handle;
			this.free = free;
		}

		Pointer<snd_seq_port_subscribe_t> handle;
		Action<IntPtr> free;

		internal IntPtr Handle => handle;

		public void Dispose ()
		{
			if ((IntPtr)handle != IntPtr.Zero)
				free (handle);
			handle = IntPtr.Zero;
		}

		public Address Sender {
			get => new Address (Natives.snd_seq_port_subscribe_get_sender (handle));
			set => Natives.snd_seq_port_subscribe_set_sender (handle, value.Handle);
		}

		public Address Destination {
			get => new Address (Natives.snd_seq_port_subscribe_get_dest (handle));
			set => Natives.snd_seq_port_subscribe_set_dest (handle, value.Handle);
		}

		public int Queue {
			get => Natives.snd_seq_port_subscribe_get_queue (handle);
			set => Natives.snd_seq_port_subscribe_set_queue (handle, value);
		}

		public bool Exclusive {
			get => Natives.snd_seq_port_subscribe_get_exclusive (handle) != 0;
			set => Natives.snd_seq_port_subscribe_set_exclusive (handle, value ? 1: 0);
		}

		public bool UpdateTime {
			get => Natives.snd_seq_port_subscribe_get_time_update (handle) != 0;
			set => Natives.snd_seq_port_subscribe_set_time_update (handle, value ? 1 : 0);
		}

		public bool IsRealTimeUpdateMode {
			get => Natives.snd_seq_port_subscribe_get_time_real (handle) != 0;
			set => Natives.snd_seq_port_subscribe_set_time_real (handle, value ? 1 : 0);
		}
	}
}
