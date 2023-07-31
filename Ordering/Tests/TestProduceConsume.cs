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
		var expected = new[]
		{
			typeof(SystemF),
			typeof(SystemG),
			typeof(SystemH),
			typeof(SystemI),
		};
		
		BlahOrderer.Order(ref systems);
		
		AssertHelper.AssertEqual(expected, systems);
	}
	


	private struct SignalA : IBlahEntrySignal { }
	
	private struct SignalB : IBlahEntrySignal { }

	private class SystemA
	{
		private IBlahSignalProducer<SignalA> _signalA;
	}

	private class SystemB
	{
		private IBlahSignalConsumer<SignalA> _signalA;
		
		private IBlahSignalProducer<SignalB> _signalB;
	}

	private class SystemC
	{
		private IBlahSignalConsumer<SignalB> _signalConsumer;
	}

	private class SystemD { }

	private class SystemE { }


	private class SystemF
	{
		private IBlahSignalProducer<SignalA> _signalA;
	}

	private class SystemG
	{
		private IBlahSignalProducer<SignalA> _signalA;
	}

	private class SystemH
	{
		private IBlahSignalConsumer<SignalA> _signalA;

		private IBlahSignalProducer<SignalB> _signalB;
	}

	private class SystemI
	{
		private IBlahSignalConsumer<SignalA> _signalA;
		private IBlahSignalConsumer<SignalB> _signalB;
	}
}
}