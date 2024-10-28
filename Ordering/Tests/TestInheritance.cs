using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TextInheritance
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
			typeof(SystemE)
		};
		
		BlahOrderer.Order(ref systems);
		
		AssertHelper.AssertEqual(expected, systems);
	}
	

	private struct SignalA : IBlahEntrySignal { }
	
	private struct SignalB : IBlahEntrySignal { }

	private struct SignalC : IBlahEntrySignal { }


	private class SystemA
	{
		private IBlahSignalWrite<SignalA> _signalA;
	}


	private class SystemBBase { }

	private class SystemB : SystemBBase
	{
		private IBlahSignalRead<SignalA> _signalA;
		
		private IBlahSignalWrite<SignalB> _signalB;
	}

	private class SystemCBase
	{
		private IBlahSignalRead<SignalB> _signalC;
	}
	
	private class SystemC : SystemCBase
	{
		private IBlahSignalWrite<SignalC> _signalC;
	}

	[BlahBefore(typeof(SystemE))]
	private class SystemDBase
	{
		private IBlahSignalRead<SignalC> _signalC;	
	}

	private class SystemD : SystemDBase { }

	private class SystemE { }
}
}