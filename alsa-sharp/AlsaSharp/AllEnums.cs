using System;

namespace AlsaSharp
{
	public enum AlsaIOType {
		Output = 1,
		Input = 2,
		Duplex = Output | Input,
	}

	public enum AlsaIOMode {
		None = 0,
		NonBlocking = 1,
	}

	[Flags]
	public enum AlsaPortType {
		/** Messages sent from/to this port have device-specific semantics. */
		Specific = 1,
		/** This port understands MIDI messages. */
		MidiGeneric = 1 << 1,
		/** This port is compatible with the General MIDI specification. */
		MidiGM = 1 << 2,

		/** This port is compatible with the Roland GS standard. */
		MidiGS = 1 << 3,
		/** This port is compatible with the Yamaha XG specification. */
		MidiXG = 1 << 4,
		/** This port is compatible with the Roland MT-32. */
		MidiMT32 = 1 << 5,
		/** This port is compatible with the General MIDI 2 specification. */
		MidiGM2 = 1 << 6,
		/** This port understands SND_SEQ_EVENT_SAMPLE_xxx messages
		    (these are not MIDI messages). */
		Synth = 1 << 10,
		/** Instruments can be downloaded to this port
		    (with SND_SEQ_EVENT_INSTR_xxx messages sent directly). */
		DirectSample = 1 << 11,
		/** Instruments can be downloaded to this port
		    (with SND_SEQ_EVENT_INSTR_xxx messages sent directly or through a queue). */
		Sample = 1 << 12,
		/** This port is implemented in hardware. */
		Hardware = 1 << 16,
		/** This port is implemented in software. */
		Software = 1 << 17,
		/** Messages sent to this port will generate sounds. */
		Synthesizer = 1 << 18,
		/** This port may connect to other devices
		    (whose characteristics are not known). */
		Port = 1 << 19,
		/** This port belongs to an application, such as a sequencer or editor. */
		Application = 1 << 20,
	}

	[Flags]
	public enum AlsaPortCapabilities {
		/**< readable from this port */
		Read = 1 << 0,
		/**< writable to this port */
		Write = 1 << 1,
		/**< for synchronization (not implemented) */
		SyncRead = 1 << 2,
		/**< for synchronization (not implemented) */
		SyncWrite = 1 << 3,
		/**< allow read/write duplex */
		Duple = 1 << 4,
		/**< allow read subscription */
		SubsRead = 1 << 5,
		/**< allow write subscription */
		SubsWrite = 1 << 6,
		/**< routing not allowed */
		NoExport = 1 << 7,
	}

	public enum AlsaSequencerType // seq.h (62, 14)
	{
		Hardware = 0,
		SharedMemory = 1,
		Network = 2,
	}

	public enum AlsaClientType
	{
		Kernel,
		User,
	}

	public enum AlsaSubscriptionQueryType
	{
		Read,
		Write,
	}

	public enum AlsaSequencerEventType
	{
		System = 0, 
		Result,
		Note = 5,
		NoteOn,
		NoteOff,
		KeyPress,
		Controller = 10,
		ProgramChange,
		ChannelPressure,
		PitchBend,
		Control14,
		Nprn,
		Rpn,
		SongPos = 20,
		SongSel,
		QFrame,
		TimeSign,
		KeySign,
		Start = 30,
		Continue,
		Stop,
		SetPositionTick,
		SetPositionTime,
		Tempo,
		Clock,
		Tick,
		QueueSkew,
		SyncPosition,
		TuneRequest = 40,
		Reset,
		Sensing,
		Echo = 50,
		Oss,
		ClientStart = 60,
		ClientExit,
		ClientChange,
		PortStart,
		PortExit,
		PortChange,
		PortSubscribed,
		PortUnsubscribed,
		User0 = 90,
		User1,
		User2,
		User3,
		User4,
		User5,
		User6,
		User7,
		User8,
		User9,
		Sysex = 130,
		Bounce,
		UserVar0 = 135,
		UserVar1,
		UserVar2,
		UserVar3,
		UserVar4,
		None = 255
	}
}
