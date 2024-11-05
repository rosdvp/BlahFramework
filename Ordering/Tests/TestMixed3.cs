using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TextMixed3
{
	[Test]
	public void Test([Range(0, 10)] int offset)
	{
		var a       = typeof(SystemA);
		var b       = typeof(SystemB);
		var c       = typeof(SystemC);
		var d       = typeof(SystemD);
		var systems = new List<Type> { d, c, b, a };
		AssertHelper.Shift(systems, offset);
		
		BlahOrderer.Order(ref systems);
		
		Assert.IsTrue(systems.IndexOf(a) < systems.IndexOf(b));
		Assert.IsTrue(systems.IndexOf(c) < systems.IndexOf(d));
	}
	

	private struct SignalA : IBlahEntrySignal { }
	
	private struct SignalB : IBlahEntrySignal { }

	[BlahAfterAll]
	private class SystemA
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	private class SystemB
	{
		private IBlahSignalRead<SignalA> _signalA;
	}

	[BlahAfterAll]
	private class SystemC
	{
		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemD
	{
		private IBlahSignalRead<SignalB> _signalB;
	}
}
}