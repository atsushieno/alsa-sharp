using System;
using System.Collections.Generic;

namespace AlsaSharp {
	public class AlsaMidiApi {
		// FIXME: can SYNTH be excluded? Chromium does not use it but RtMidi does.
		const AlsaPortType midi_port_type = AlsaPortType.MidiGeneric | AlsaPortType.Application;

		AlsaSequencer input, output, system_announcement;

		public AlsaMidiApi ()
		{
			input = new AlsaSequencer (AlsaIOType.Duplex, AlsaIOMode.NonBlocking);
			input_client_id = input.CurrentClientId;
			output = new AlsaSequencer (AlsaIOType.Output, AlsaIOMode.NonBlocking);
			output_client_id = output.CurrentClientId;
			system_announcement = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking);
		}
		int input_client_id, output_client_id;

		public AlsaSequencer Input => input;
		public AlsaSequencer Output => output;

		readonly AlsaPortCapabilities input_requirements = AlsaPortCapabilities.Read | AlsaPortCapabilities.SubsRead;
		readonly AlsaPortCapabilities output_requirements = AlsaPortCapabilities.Write | AlsaPortCapabilities.SubsWrite;
		readonly AlsaPortCapabilities output_connected_cap = AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport;
		readonly AlsaPortCapabilities input_connected_cap = AlsaPortCapabilities.Read | AlsaPortCapabilities.NoExport;

		IEnumerable<AlsaPortInfo> EnumerateMatchingPorts (AlsaSequencer seq, AlsaPortCapabilities cap)
		{
			var cinfo = new AlsaClientInfo { Client = -1 };
			while (seq.QueryNextClient (cinfo)) {
				var pinfo = new AlsaPortInfo { Client = cinfo.Client, Port = -1 };
				while (seq.QueryNextPort (pinfo))
					if ((pinfo.PortType & midi_port_type) != 0 &&
					    (pinfo.Capabilities & cap) == cap)
						yield return pinfo.Clone ();
			}
		}

		public IEnumerable<AlsaPortInfo> EnumerateAvailableInputPorts ()
		{
			return EnumerateMatchingPorts (input, input_requirements);
		}

		public IEnumerable<AlsaPortInfo> EnumerateAvailableOutputPorts ()
		{
			return EnumerateMatchingPorts (output, output_requirements);
		}

		// [input device port] --> [RETURNED PORT] --> app handles messages
		public AlsaPortInfo CreateInputConnectedPort (AlsaPortInfo pinfo, string portName = "alsa-sharp input")
		{
			var portId = input.CreateSimplePort (portName, input_connected_cap, midi_port_type);
			var sub = new AlsaPortSubscription ();
			sub.Destination.Client = (byte)input_client_id;
			sub.Destination.Port = (byte)portId;
			sub.Sender.Client = (byte)pinfo.Client;
			sub.Sender.Port = (byte)pinfo.Port;
			input.SubscribePort (sub);
			return input.GetPort (sub.Destination.Client, sub.Destination.Port);
		}

		// app generates messages --> [RETURNED PORT] --> [output device port]
		public AlsaPortInfo CreateOutputConnectedPort (AlsaPortInfo pinfo, string portName = "alsa-sharp output")
		{
			var portId = output.CreateSimplePort (portName, output_connected_cap, midi_port_type);
			var sub = new AlsaPortSubscription ();
			sub.Sender.Client = (byte)output_client_id;
			sub.Sender.Port = (byte)portId;
			sub.Destination.Client = (byte)pinfo.Client;
			sub.Destination.Port = (byte)pinfo.Port;
			output.SubscribePort (sub);
			return output.GetPort (sub.Sender.Client, sub.Sender.Port);
		}
	}
}
