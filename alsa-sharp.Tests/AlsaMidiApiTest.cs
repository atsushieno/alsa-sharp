using System;
using System.Linq;
using NUnit.Framework;

namespace AlsaSharp.Tests 
{
	public class AlsaMidiApiTest 
	{
		[Test]
		public void EnumeratePorts ()
		{
			var api = new AlsaMidiApi ();
			foreach (var port in api.EnumerateAvailableInputPorts ())
				Console.Error.WriteLine ("Input: " + port.Id + " : " + port.Name);
			foreach (var port in api.EnumerateAvailableOutputPorts ())
				Console.Error.WriteLine ("Output: " + port.Id + " : " + port.Name);
		}

		[Test]
		public void CreateInputConnectedPort ()
		{
			var api = new AlsaMidiApi ();
			var input = api.CreateInputConnectedPort (api.EnumerateAvailableInputPorts ().Last ());
			input.Dispose ();
		}

		[Test]
		public void CreateOutputConnectedPort ()
		{
			var api = new AlsaMidiApi ();
			var output = api.CreateOutputConnectedPort (api.EnumerateAvailableOutputPorts ().Last ());
			output.Dispose ();
		}
	}
}
