using System;
namespace AlsaSharp {
	public class AlsaSubscriptionQuery {
		static IntPtr Malloc ()
		{
			unsafe {
				var ptr = IntPtr.Zero;
				var pref = &ptr;
				Natives.snd_seq_query_subscribe_malloc ((IntPtr)pref);
				return ptr;
			}
		}

		static void Free (IntPtr handle)
		{
			if ((IntPtr)handle != IntPtr.Zero)
				Natives.snd_seq_query_subscribe_free (handle);
		}

		public AlsaSubscriptionQuery ()
			: this (Malloc (), Free)
		{
		}

		public AlsaSubscriptionQuery (IntPtr handle, Action<IntPtr> free)
		{
			this.handle = handle;
			this.free = free;
		}

		Pointer<snd_seq_query_subscribe_t> handle;
		Action<IntPtr> free;

		internal IntPtr Handle => handle;

		public void Dispose ()
		{
			if ((IntPtr)handle != IntPtr.Zero)
				free (handle);
			handle = IntPtr.Zero;
		}

		public int Client {
			get => Natives.snd_seq_query_subscribe_get_client (handle);
			set => Natives.snd_seq_query_subscribe_set_client (handle, value);
		}

		public int Port {
			get => Natives.snd_seq_query_subscribe_get_port (handle);
			set => Natives.snd_seq_query_subscribe_set_port (handle, value);
		}

		public int Index {
			get => Natives.snd_seq_query_subscribe_get_index (handle);
			set => Natives.snd_seq_query_subscribe_set_index (handle, value);
		}

		public AlsaSubscriptionQueryType Type {
			get => (AlsaSubscriptionQueryType) Natives.snd_seq_query_subscribe_get_type (handle);
			set => Natives.snd_seq_query_subscribe_set_type (handle, (snd_seq_query_subs_type_t) value);
		}

		public AlsaPortSubscription.Address Address => new AlsaPortSubscription.Address (Natives.snd_seq_query_subscribe_get_addr (handle));

		public bool Exclusive => Natives.snd_seq_query_subscribe_get_exclusive (handle) != 0;

		public int Queue => Natives.snd_seq_query_subscribe_get_queue (handle);
	}
}
