using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlsaSharp;
using NUnit.Framework;

namespace AlsaSharp.Tests 
{
	[TestFixture]
	public class AlsaMidiTest 
	{
		[Test]
		public void SystemInfo ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Output, AlsaIOMode.NonBlocking)) {
				using (var sys = new AlsaSystemInfo ()) {
					sys.SetContextSequencer (seq);
					Assert.AreEqual (1, sys.CurrentQueueCount, "cur_q");
					Assert.IsTrue (0 < sys.CurrentClientCount, "cur_ch");
					Assert.IsTrue (0 < sys.PortCount, "port");
					Assert.IsTrue (0 < sys.ChannelCount, "ch");
					Assert.IsTrue (sys.CurrentQueueCount < sys.MaxQueueCount, "max_q");
					Assert.IsTrue (sys.CurrentClientCount < sys.MaxClientCount, "max_cli");
				}
			}
		}

		[Test]
		public void GetClient ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking)) {
				var cli = seq.GetClient (AlsaSequencer.ClientSystem);
				foreach (var cpi in cli.GetType ().GetProperties ())
					TextWriter.Null.WriteLine ($"  [{cpi}]\t{cpi.GetValue (cli)}");
			}
		}

		[Test]
		public void GetPort ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking)) {
				var port = seq.GetPort (AlsaSequencer.ClientSystem, AlsaPortInfo.PortSystemAnnouncement);
				foreach (var ppi in port.GetType ().GetProperties ())
					TextWriter.Null.WriteLine ($"    [{ppi}]\t{ppi.GetValue (port)}");
			}
		}

		[Test]
		public void SetClientName ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking, "default")) {
				Assert.AreEqual ("default", seq.Name, "#1");
				seq.SetClientName ("overwritten sequencer name");
				Assert.AreEqual ("default", seq.Name, "#2");
				Assert.AreEqual ("overwritten sequencer name", seq.GetClient (seq.CurrentClientId).Name, "#3");
			}
		}

		[Test]
		public void SetClientNameInvalid ()
		{
			Assert.Throws (typeof (AlsaException), () => {
				var seq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking, "this_is_an_invalid_config_name_and_should_throw_exception");
			}, "should throw ALSA exception");
		}

		[Test]
		public void EnumerateClientsAndPortsPrimitive ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Output, AlsaIOMode.NonBlocking)) {
				var cli = new AlsaClientInfo ();
				cli.Client = -1;
				while (seq.QueryNextClient (cli)) {
					TextWriter.Null.WriteLine ("Client:" + cli.Client);
					foreach (var cpi in cli.GetType ().GetProperties ()) {
						TextWriter.Null.WriteLine ($"  [{cpi}]\t{cpi.GetValue (cli)}");
					}
					var port = new AlsaPortInfo ();
					port.Client = cli.Client;
					port.Port = -1;
					while (seq.QueryNextPort (port)) {
						TextWriter.Null.WriteLine ("  Port:" + port.Id);
						foreach (var ppi in port.GetType ().GetProperties ()) {
							TextWriter.Null.WriteLine ($"    [{ppi}]\t{ppi.GetValue (port)}");
						}
					}
				}
			}
		}

		[Test]
		public void SubscribeUnsubscribePort ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking)) {
				var subs = new AlsaPortSubscription ();
				subs.Sender.Client = AlsaSequencer.ClientSystem;
				subs.Sender.Port = AlsaPortInfo.PortSystemAnnouncement;
				subs.Destination.Client = (byte) seq.CurrentClientId;
				subs.Destination.Port = (byte) seq.CreateSimplePort ("test in port", AlsaPortCapabilities.SubsRead | AlsaPortCapabilities.Read, AlsaPortType.MidiGeneric | AlsaPortType.Application);
				try {
					seq.SubscribePort (subs);
					foreach (var ppi in subs.GetType ().GetProperties ()) {
						TextWriter.Null.WriteLine ($"    [{ppi}]\t{ppi.GetValue (subs)}");
					}
					seq.UnsubscribePort (subs);
				} finally {
					seq.DeleteSimplePort (subs.Destination.Port);
				}
			}
		}

		[Test]
		public void Send ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Output, AlsaIOMode.NonBlocking)) {

				var cinfo = new AlsaClientInfo { Client = -1 };
				int lastClient = -1;
				while (seq.QueryNextClient (cinfo))
					if (cinfo.Name.Contains ("TiMidity"))
						lastClient = cinfo.Client;
				if (lastClient < 0) {
					Console.Error.WriteLine ("TiMidity not found. Not testable.");
					return; // not testable
				}

				int targetPort = 3;
				try {
					seq.GetPort (lastClient, targetPort);
				} catch {
					Console.Error.WriteLine ("TiMidity port #3 not available. Not testable.");
					return; // not testable
				}

				int appPort = seq.CreateSimplePort ("alsa-sharp-test-output", AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport, AlsaPortType.Application | AlsaPortType.MidiGeneric);
				try {
					seq.ConnectTo (appPort, lastClient, targetPort);
					var setup = new byte [] { 0xC0, 0x48, 0xB0, 7, 110, 0xB0, 11, 127 };
					var keyon = new byte [] { 0x90, 0x40, 0x70 };
					var keyoff = new byte [] { 0x80, 0x40, 0x70 };
					seq.Send (appPort, setup, 0, setup.Length);
					seq.Send (appPort, keyon, 0, keyon.Length);
					System.Threading.Thread.Sleep (100);
					seq.Send (appPort, keyoff, 0, keyoff.Length);
					System.Threading.Thread.Sleep (100);
					seq.DisconnectTo (appPort, lastClient, targetPort);
				} finally {
					seq.DeleteSimplePort (appPort);
				}
			}
		}

		[Test]
		public void InputToObserveSystemAnnoucements ()
		{
			bool passed = false;
			var evt = new AlsaSequencerEvent ();
			AlsaSequencer inseq = null;
			int appPort = -1;
			Task task = new TaskFactory ().StartNew (() => {
				using (inseq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.None)) {
					appPort = inseq.CreateSimplePort ("alsa-sharp-test-input", AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport, AlsaPortType.Application | AlsaPortType.MidiGeneric);
					inseq.ConnectFrom (appPort, AlsaSequencer.ClientSystem, AlsaPortInfo.PortSystemAnnouncement);
					try {
						inseq.ResetPoolInput ();
						// ClientStart, PortStart, PortSubscribed, PortUnsubscribed, PortExit
						inseq.Input (evt, appPort);
						Assert.AreEqual (AlsaSequencerEventType.ClientStart, evt.EventType, "evt1");
						inseq.Input (evt, appPort);
						Assert.AreEqual (AlsaSequencerEventType.PortStart, evt.EventType, "evt2");
						inseq.Input (evt, appPort);
						Assert.AreEqual (AlsaSequencerEventType.PortSubscribed, evt.EventType, "evt3");
						passed = true;
					} finally {
						inseq.DisconnectFrom (appPort, AlsaSequencer.ClientSystem, AlsaPortInfo.PortSystemAnnouncement);
						appPort = -1;
					}
				}
			});
			Thread.Sleep (50); // give some time for announcement client to start.

			// create another port, which is a dummy and just subscribes to notify the system to raise an announcement event.
			var cinfo = new AlsaClientInfo { Client = -1 };
			int lastClient = -1;
			var outseq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.NonBlocking);
			while (outseq.QueryNextClient (cinfo))
				if (cinfo.Name.Contains ("Midi Through"))
					lastClient = cinfo.Client;
			if (lastClient < 0) {
				Console.Error.WriteLine ("Midi Through not found. Not testable.");
				return; // not testable
			}
			int targetPort = 0;

			int testPort = outseq.CreateSimplePort ("alsa-sharp-test-output", AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport, AlsaPortType.Application | AlsaPortType.MidiGeneric);
			try {
				outseq.ConnectTo (testPort, lastClient, targetPort);
				outseq.DisconnectTo (testPort, lastClient, targetPort);
				Thread.Sleep (50); // give some time for announcement client to finish.
				Assert.IsTrue (passed, "failed to receive an announcement");
			} finally {
				outseq.DeleteSimplePort (testPort);
			}
		}

		[Test]
		public void Receive ()
		{
			using (var seq = new AlsaSequencer (AlsaIOType.Input, AlsaIOMode.None)) {

				var cinfo = new AlsaClientInfo { Client = -1 };
				int lastClient = -1;
				while (seq.QueryNextClient (cinfo))
					if (cinfo.Name.Contains ("Keystation"))
						lastClient = cinfo.Client;
				if (lastClient < 0) {
					Console.Error.WriteLine ("Keystation not found. Not testable.");
					return; // not testable
				}
				Console.Error.WriteLine ("Press any key on Keystation to continue...");

				int targetPort = 0;

				int appPort = seq.CreateSimplePort ("alsa-sharp-test-input", AlsaPortCapabilities.Write | AlsaPortCapabilities.NoExport, AlsaPortType.Application | AlsaPortType.MidiGeneric);
				try {
					seq.ConnectFrom (appPort, lastClient, targetPort);
					var data = new byte [3];
					var received = seq.Receive (appPort, data, 0, 3);
					Assert.AreEqual (3, received, "received size");
					Assert.AreEqual (0x90, data [0], "received status");
					seq.DisconnectFrom (appPort, lastClient, targetPort);
				} finally {
					seq.DeleteSimplePort (appPort);
				}
			}
		}
	}
}
