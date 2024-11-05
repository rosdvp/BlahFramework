using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TextMixed2
{
	[Test]
	public void Test([Range(0, 10)] int offset)
	{
		var systems = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemC),
			typeof(SystemB),
		};
		AssertHelper.Shift(systems, offset);
		var expected = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};
		
		BlahOrderer.Order(ref systems);
		
		AssertHelper.AssertOrder(expected, systems);
	}
	

	private struct SignalA : IBlahEntrySignal { }
	
	private struct SignalB : IBlahEntrySignal { }

	private class SystemA
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	[BlahAfterAll]
	private class SystemB
	{
		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemC
	{
		private IBlahSignalRead<SignalA> _signalA;
		private IBlahSignalRead<SignalB> _signalB;
	}
}
}