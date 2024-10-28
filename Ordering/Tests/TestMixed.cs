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
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
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
		private IBlahSignalWrite<SignalA> _signalA;
	}

	[BlahBefore(typeof(SystemC))]
	private class SystemB
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	[BlahAfter(typeof(SystemA))]
	private class SystemC
	{
		private IBlahSignalWrite<SignalA> _signalA;
		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemD
	{
		private IBlahSignalRead<SignalA> _signalA;

		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemE
	{
		private IBlahSignalRead<SignalB> _signalB;
	}
}
}