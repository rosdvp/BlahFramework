using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TextMixed4
{
	[Test]
	public void Test()
	{
		var a       = typeof(SystemA);
		var b       = typeof(SystemB);
		var c       = typeof(SystemC);
		var systems = new List<Type> { b, c, a };
		
		BlahOrderer.Order(ref systems);
		
		Assert.IsTrue(systems.IndexOf(a) < systems.IndexOf(b));
	}
	

	private struct SignalA : IBlahEntrySignal { }

	[BlahAfterAll]
	private class SystemA
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	[BlahAfterAll]
	private class SystemB
	{
		private IBlahSignalRead<SignalA> _signalA;
	}

	[BlahAfterAll]
	private class SystemC { }
}
}