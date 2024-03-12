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
	public void Test()
	{
		var systems = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemC),
			typeof(SystemB),
		};
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
		private IBlahSignalProducer<SignalA> _signalA;
	}

	[BlahAfterAll]
	private class SystemB
	{
		private IBlahSignalProducer<SignalB> _signalB;
	}

	private class SystemC
	{
		private IBlahSignalConsumer<SignalA> _signalA;
		private IBlahSignalConsumer<SignalB> _signalB;
	}
}
}