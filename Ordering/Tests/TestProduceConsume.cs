using System;
using System.Collections.Generic;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestProduceConsume
{
	[Test]
	public void Test_AllWithFields()
	{
		var systems = new List<Type>
		{
			typeof(SystemC),
			typeof(SystemA),
			typeof(SystemB),
		};
		var expected = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};
		
		BlahOrderer.Order(ref systems);
		
		AssertHelper.AssertEqual(expected, systems);
	}

	[Test]
	public void Test_SomeWithFields()
	{
		var systems = new List<Type>
		{
			typeof(SystemB),
			typeof(SystemD),
			typeof(SystemA),
			typeof(SystemE),
			typeof(SystemC),
		};
		var expectedOrder = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};
		
		BlahOrderer.Order(ref systems);
		
		AssertHelper.AssertOrder(expectedOrder, systems);
	}

	[Test]
	public void Test_ComplexChain()
	{
		var systems = new List<Type>
		{
			typeof(SystemI),
			typeof(SystemH),
			typeof(SystemF),
			typeof(SystemG),
		};
		
		BlahOrderer.Order(ref systems);

		int indexF = systems.IndexOf(typeof(SystemF));
		int indexG = systems.IndexOf(typeof(SystemG));
		int indexH = systems.IndexOf(typeof(SystemH));
		int indexI = systems.IndexOf(typeof(SystemI));
		
		Assert.IsTrue(indexF < indexH, $"F({indexF}) < H({indexH})");
		Assert.IsTrue(indexG < indexH, $"G({indexG}) < H({indexH})");
		Assert.IsTrue(indexH < indexI, $"H({indexH}) < I({indexI})");
	}
	


	private struct SignalA : IBlahEntrySignal { }
	
	private struct SignalB : IBlahEntrySignal { }

	private class SystemA
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	private class SystemB
	{
		private IBlahSignalRead<SignalA> _signalA;
		
		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemC
	{
		private IBlahSignalRead<SignalB> _signalConsumer;
	}

	private class SystemD { }

	private class SystemE { }


	private class SystemF
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	private class SystemG
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}

	private class SystemH
	{
		private IBlahSignalRead<SignalA> _signalA;

		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemI
	{
		private IBlahSignalRead<SignalA> _signalA;
		private IBlahSignalRead<SignalB> _signalB;
	}
}
}