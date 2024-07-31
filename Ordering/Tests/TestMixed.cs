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
			typeof(SystemC),
			typeof(SystemD),
			typeof(SystemE),
		};
		
		UnityEngine.Random.InitState(100);
		for (var i = 0; i < 10; i++)
		{
			AssertHelper.Randomize(ref systems);
			BlahOrderer.Order(ref systems);
			AssertHelper.AssertEqual(expected, systems);	
		}
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
		private IBlahSignalRead<SignalA> _signalA;
	}

	[BlahAfter(typeof(SystemA))]
	private class SystemC
	{
		private IBlahSignalWrite<SignalB> _signalB;
	}

	[BlahAfter(typeof(SystemC))]
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