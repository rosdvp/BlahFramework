using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Injection.Tests
{
internal class Test
{
	[Test]
	public void Test_InjectInSingle_Injected()
	{
		var injector = BuildInjector();
		
		var target1  = new MockTarget1();
		injector.InjectInto(target1);
		
		Assert.NotNull(target1.Item1);
		Assert.NotNull(target1.ItemA1);
		Assert.NotNull(target1.ItemB1);
	}

	[Test]
	public void Test_InjectInTwoDifferent_Injected()
	{
		var injector = BuildInjector();
		
		var target1 = new MockTarget1();
		injector.InjectInto(target1);
		var target2 = new MockTarget2();
		injector.InjectInto(target2);
		
		Assert.NotNull(target1.Item1);
		Assert.NotNull(target1.ItemA1);
		Assert.NotNull(target1.ItemB1);
		
		Assert.NotNull(target2.Item2);
		Assert.NotNull(target2.ItemA2);
		Assert.NotNull(target2.ItemB2);
	}

	[Test]
	public void Test_InjectInThreeWithSameFields_InjectedSame()
	{
		var injector = BuildInjector();

		var target1 = new MockTarget1();
		injector.InjectInto(target1);
		var target2 = new MockTarget2();
		injector.InjectInto(target2);
		var target12 = new MockTarget12();
		injector.InjectInto(target12);

		Assert.NotNull(target1.Item1);
		Assert.NotNull(target1.ItemA1);
		Assert.NotNull(target1.ItemB1);

		Assert.NotNull(target2.Item2);
		Assert.NotNull(target2.ItemA2);
		Assert.NotNull(target2.ItemB2);

		Assert.AreSame(target1.Item1, target12.Item1);
		Assert.AreSame(target1.ItemA1, target12.ItemA1);
		Assert.AreSame(target1.ItemB1, target12.ItemB1);
		Assert.AreSame(target2.Item2, target12.Item2);
		Assert.AreSame(target2.ItemA2, target12.ItemA2);
		Assert.AreSame(target2.ItemB2, target12.ItemB2);
	}
	
	[Test]
	public void Test_InjectInheritance_Injected()
	{
		var injector = BuildInjector();

		var              targetChild  = new MockTargetChild();
		MockTargetParent targetParent = targetChild;
		injector.InjectInto(targetParent);
		
		Assert.NotNull(targetChild.ParentItem1);
		Assert.NotNull(targetChild.ParentItem2);
		Assert.NotNull(targetChild.ParentItemA1);
		Assert.NotNull(targetChild.ParentItemB1);
		Assert.NotNull(targetChild.ChildItemA2);
		Assert.NotNull(targetChild.ChildItemB2);
		Assert.AreEqual(targetChild.ParentItem1, targetChild.ChildItem1);
	}



	private BlahInjector BuildInjector()
	{
		var source = new MockSource();

		var injector = new BlahInjector();
		injector.AddSource(source,
		                   typeof(MockInhItemBase),
		                   nameof(MockSource.GetInhItem),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
		);
		injector.AddSource(source,
		                   typeof(MockGenItemA<>),
		                   nameof(MockSource.GetGenItemA),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		injector.AddSource(source,
		                   typeof(MockGenItemB<>),
		                   nameof(MockSource.GetGenItemB),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		return injector;
	}

	private class MockTarget1
	{
		private MockInhItem1 _item1;

		private MockGenItemA<MockGenArgA1> _itemA1;

		private MockGenItemB<MockGenArgB1> _itemB1;

		private int       _dummyInt;
		private List<int> _dummyList;
		
		public MockInhItem1               Item1  => _item1;
		public MockGenItemA<MockGenArgA1> ItemA1 => _itemA1;
		public MockGenItemB<MockGenArgB1> ItemB1 => _itemB1;
	}

	private class MockTarget2
	{
		private MockInhItem2 _item2;

		private MockGenItemA<MockGenArgA2> _itemA2;

		private MockGenItemB<MockGenArgB2> _itemB2;
		
		private int       _dummyInt;
		private List<int> _dummyList;
		
		public MockInhItem2               Item2  => _item2;
		public MockGenItemA<MockGenArgA2> ItemA2 => _itemA2;
		public MockGenItemB<MockGenArgB2> ItemB2 => _itemB2;
	}
	
	private class MockTarget12
	{
		private MockInhItem1 _item1;
		private MockInhItem2 _item2;

		private MockGenItemA<MockGenArgA1> _itemA1;
		private MockGenItemA<MockGenArgA2> _itemA2;

		private MockGenItemB<MockGenArgB1> _itemB1;
		private MockGenItemB<MockGenArgB2> _itemB2;
		
		private int       _dummyInt;
		private List<int> _dummyList;

		public MockInhItem1               Item1  => _item1;
		public MockInhItem2               Item2  => _item2;
		public MockGenItemA<MockGenArgA1> ItemA1 => _itemA1;
		public MockGenItemA<MockGenArgA2> ItemA2 => _itemA2;
		public MockGenItemB<MockGenArgB1> ItemB1 => _itemB1;
		public MockGenItemB<MockGenArgB2> ItemB2 => _itemB2;
	}

	public class MockTargetParent
	{
		private   MockInhItem1               _parentItem1;
		protected MockInhItem2               _parentItem2;
		private   MockGenItemA<MockGenArgA1> _parentItemA1;
		private   MockGenItemB<MockGenArgB1> _parentItemB1;
		
		public MockInhItem1               ParentItem1  => _parentItem1;
		public MockInhItem2               ParentItem2  => _parentItem2;
		public MockGenItemA<MockGenArgA1> ParentItemA1 => _parentItemA1;
		public MockGenItemB<MockGenArgB1> ParentItemB1 => _parentItemB1;
	}

	public class MockTargetChild : MockTargetParent
	{
		private MockInhItem1               _childItem1;
		private MockGenItemA<MockGenArgA2> _childItemA2;
		private MockGenItemB<MockGenArgB2> _childItemB2;
		
		public MockInhItem1               ChildItem1  => _childItem1;
		public MockGenItemA<MockGenArgA2> ChildItemA2 => _childItemA2;
		public MockGenItemB<MockGenArgB2> ChildItemB2 => _childItemB2;
	}
}
}