using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TextMixed
{
	[Test]
	public void Test()
	{
		var systems = new List<Type>
		{
			typeof(SystemE),
			typeof(SystemD),
			typeof(SystemC),
			typeof(SystemB),
			typeof(SystemA)
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

	[BlahBefore(typeof(SystemC))]
	private class SystemB
	{
		private IBlahSignalProducer<SignalA> _signalA;
	}

	[BlahAfter(typeof(SystemA))]
	private class SystemC
	{
		private IBlahSignalProducer<SignalA> _signalA;
		private IBlahSignalProducer<SignalB> _signalB;
	}

	private class SystemD
	{
		private IBlahSignalConsumer<SignalA> _signalA;

		private IBlahSignalProducer<SignalB> _signalB;
	}

	private class SystemE
	{
		private IBlahSignalConsumer<SignalB> _signalB;
	}
}
}