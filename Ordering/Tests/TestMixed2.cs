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