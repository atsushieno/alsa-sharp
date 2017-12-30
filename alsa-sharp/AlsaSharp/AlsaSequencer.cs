using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AlsaSharp
{

	public class AlsaSequencer : IDisposable {
		public const int ClientSystem = 0;

		public AlsaSequencer (AlsaIOType ioType, AlsaIOMode ioMode, string driverName = "default")
		{
			unsafe {
				var ptr = IntPtr.Zero;
				var pref = &ptr;
				driver_name_handle = Marshal.StringToHGlobalAnsi (driverName);
				var err = Natives.snd_seq_open ((IntPtr)pref, driver_name_handle, (int)ioType, (int)ioMode);
				if (err != 0)
					throw new AlsaException (err);
				seq = ptr;
			}
			io_type = ioType;
		}

		IntPtr seq;
		IntPtr driver_name_handle;
		AlsaIOType io_type;

		internal IntPtr SequencerHandle => seq;

		public void Dispose ()
		{
			if (midi_event_parser != IntPtr.Zero) {
				Natives.snd_midi_event_free (midi_event_parser);
				midi_event_parser = IntPtr.Zero;
			}
			if (driver_name_handle != IntPtr.Zero) {
				Marshal.FreeHGlobal (driver_name_handle);
				driver_name_handle = IntPtr.Zero;
			}
			if (port_name_handle != IntPtr.Zero) {
				Marshal.FreeHGlobal (port_name_handle);
				port_name_handle = IntPtr.Zero;
			}
			if (name_handle != IntPtr.Zero) {
				Marshal.FreeHGlobal (name_handle);
				name_handle = IntPtr.Zero;
			}
			if (seq != IntPtr.Zero) {
				Natives.snd_seq_close (seq);
				seq = IntPtr.Zero;
			}
		}

		public AlsaIOType IOType => io_type;

		public string Name => Marshal.PtrToStringAnsi (Natives.snd_seq_name (seq));

		public AlsaSequencerType SequencerType => (AlsaSequencerType)Natives.snd_seq_type (seq);

		public void SetNonBlockingMode (bool toNonBlockingMode)
		{
			Natives.snd_seq_nonblock (seq, toNonBlockingMode ? 1 : 0);
		}

		public int CurrentClientId => Natives.snd_seq_client_id (seq);

		public int InputBufferSize {
			get => (int)Natives.snd_seq_get_input_buffer_size (seq);
			set => Natives.snd_seq_set_input_buffer_size (seq, (uint)value);
		}
		public int OutputBufferSize {
			get => (int)Natives.snd_seq_get_output_buffer_size (seq);
			set => Natives.snd_seq_set_output_buffer_size (seq, (uint)value);
		}

		public AlsaPortType TargetPortType => AlsaPortType.MidiGeneric | AlsaPortType.Synth | AlsaPortType.Application;

		public bool QueryNextClient (AlsaClientInfo client)
		{
			int ret = Natives.snd_seq_query_next_client (seq, client.Handle);
			return ret >= 0;
		}

		public bool QueryNextPort (AlsaPortInfo port)
		{
			int ret = Natives.snd_seq_query_next_port (seq, port.Handle);
			return ret >= 0;
		}

		public AlsaClientInfo GetClient (int client)
		{
			unsafe {
				var cinfo = new AlsaClientInfo ();
				int ret = Natives.snd_seq_get_any_client_info (seq, client, cinfo.Handle);
				if (ret != 0)
					throw new AlsaException (ret);
				return cinfo;
			}
		}

		public AlsaPortInfo GetPort (int client, int port)
		{
			unsafe {
				var pinfo = new AlsaPortInfo ();
				var err = Natives.snd_seq_get_any_port_info (seq, client, port, pinfo.Handle);
				if (err != 0)
					throw new AlsaException (err);
				return pinfo;
			}
		}

		IntPtr port_name_handle;
		public int CreateSimplePort (string name, AlsaPortCapabilities caps, AlsaPortType type)
		{
			if (port_name_handle != IntPtr.Zero)
				Marshal.FreeHGlobal (port_name_handle);
			port_name_handle = name == null ? IntPtr.Zero : Marshal.StringToHGlobalAnsi (name);
			return Natives.snd_seq_create_simple_port (seq, port_name_handle, (uint)caps, (uint)type);
		}

		public void DeleteSimplePort (int port)
		{
			int ret = Natives.snd_seq_delete_simple_port (seq, port);
			if (ret != 0)
				throw new AlsaException (ret);
		}

		IntPtr name_handle;
		public void SetClientName (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));
			unsafe {
				if (name_handle != IntPtr.Zero)
					Marshal.FreeHGlobal (name_handle);
				name_handle = Marshal.StringToHGlobalAnsi (name);
				Natives.snd_seq_set_client_name (seq, name_handle);
			}
		}

		#region Subscription

		public void SubscribePort (AlsaPortSubscription subs)
		{
			unsafe {
				int err = Natives.snd_seq_subscribe_port (seq, subs.Handle);
				if (err != 0)
					throw new AlsaException (err);
			}
		}

		public void UnsubscribePort (AlsaPortSubscription sub)
		{
			Natives.snd_seq_unsubscribe_port (seq, sub.Handle);
		}

		public bool QueryPortSubscribers (AlsaSubscriptionQuery query)
		{
			var ret = Natives.snd_seq_query_port_subscribers (seq, query.Handle);
			return ret == 0;
		}

		// simplified SubscribePort()
		public void ConnectFrom (int portToReceive, int sourceClient, int sourcePort)
		{
			var err = Natives.snd_seq_connect_from (seq, portToReceive, sourceClient, sourcePort);
			if (err != 0)
				throw new AlsaException (err);
		}

		// simplified SubscribePort()
		public void ConnectTo (int portToSendFrom, int destinationClient, int destinationPort)
		{
			var err = Natives.snd_seq_connect_to (seq, portToSendFrom, destinationClient, destinationPort);
			if (err != 0)
				throw new AlsaException (err);
		}

		// simplified UnsubscribePort()
		public void DisconnectFrom (int portToReceive, int sourceClient, int sourcePort)
		{
			var err = Natives.snd_seq_disconnect_from (seq, portToReceive, sourceClient, sourcePort);
			if (err != 0)
				throw new AlsaException (err);
		}

		// simplified UnsubscribePort()
		public void DisconnectTo (int portToSendFrom, int destinationClient, int destinationPort)
		{
			var err = Natives.snd_seq_disconnect_to (seq, portToSendFrom, destinationClient, destinationPort);
			if (err != 0)
				throw new AlsaException (err);
		}

		#endregion // Subscription

		public void ResetPoolInput ()
		{
			Natives.snd_seq_reset_pool_input (seq);
		}

		public void ResetPoolOutput ()
		{
			Natives.snd_seq_reset_pool_output (seq);
		}

		#region Events

		static readonly int seq_evt_size = Marshal.SizeOf (typeof (snd_seq_event_t));
		static readonly int seq_evt_off_source_port = (int)Marshal.OffsetOf (typeof (snd_seq_event_t), "source") + (int)Marshal.OffsetOf (typeof (snd_seq_addr_t), "port");
		static readonly int seq_evt_off_dest_client = (int)Marshal.OffsetOf (typeof (snd_seq_event_t), "dest") + (int)Marshal.OffsetOf (typeof (snd_seq_addr_t), "client");
		static readonly int seq_evt_off_dest_port = (int)Marshal.OffsetOf (typeof (snd_seq_event_t), "dest") + (int)Marshal.OffsetOf (typeof (snd_seq_addr_t), "port");
		static readonly int seq_evt_off_queue = (int)Marshal.OffsetOf (typeof (snd_seq_event_t), "queue");

		public const byte AddressUnknown = 253;
		public const byte AddressSubscribers = 254;
		public const byte AddressBroadcast = 255;

		public const byte QueueDirect = 253;

		public void Output (int port, byte [] data, int index, int count)
		{
			unsafe {
				fixed (byte *ptr = data)
					Output (port, ptr, index, count);
			}
		}

		const int midi_event_buffer_size = 256;
		byte [] event_buffer = new byte [midi_event_buffer_size];
		IntPtr midi_event_parser;

		// FIXME: should this be moved to AlsaMidiApi? It's a bit too high level.
		public unsafe void Output (int port, byte *data, int index, int count)
		{
			if (midi_event_parser == IntPtr.Zero) {
				var ptr = midi_event_parser;
				var pref = &ptr;
				Natives.snd_midi_event_new (midi_event_buffer_size, (IntPtr) pref);
				midi_event_parser = ptr;
			}
			fixed (byte* ev = event_buffer) {
				for (int i = index; i < index + count; i++) {
					int ret = Natives.snd_midi_event_encode_byte (midi_event_parser, data [i], (IntPtr) ev);
					if (ret < 0)
						throw new AlsaException (ret);
					if (ret == 1) {
						Marshal.WriteByte ((IntPtr)ev, seq_evt_off_source_port, (byte)port);
						Marshal.WriteByte ((IntPtr)ev, seq_evt_off_dest_client, AddressSubscribers);
						Marshal.WriteByte ((IntPtr)ev, seq_evt_off_dest_port, AddressUnknown);
						Marshal.WriteByte ((IntPtr)ev, seq_evt_off_queue, QueueDirect);
						Natives.snd_seq_event_output_direct (seq, (IntPtr)ev);
					}
				}
			}
		}

		public void Input (AlsaSequencerEvent result, int port)
		{
			unsafe {
				IntPtr evt = IntPtr.Zero;
				var eref = &evt;
				int ret = Natives.snd_seq_event_input (seq, (IntPtr)eref);
				if (ret < 0)
					throw new AlsaException (ret);
				Marshal.PtrToStructure (evt, result);
			}
		}

		#endregion
	}


	// This is a class for temporary managed class to make it possible to unmarshal via PtrToStructure.
	[StructLayout (LayoutKind.Sequential)]
	public class AlsaSequencerEvent
	{
		byte type;
		byte flags;
		byte tag;
		byte queue;
		snd_seq_timestamp_t time;
		snd_seq_addr_t source;
		snd_seq_addr_t dest;
		// FIXME: some of the struct members are arrays with SizeConsts, but the runtime (either mono or CoreCLR) does not accept them.
		// Therefore it is commented out, but that will result in inconsistent sizing between managed and unmanaged.
		//anonymous_type_3 data;

		public AlsaSequencerEventType EventType => (AlsaSequencerEventType) type;
	}
}
