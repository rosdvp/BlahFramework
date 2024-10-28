using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Blah.Pools.Tests.Data
{
internal class TestDataPtr
{
	[Test]
	public void TestNewPtr_PtrInvalid()
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();

		var ptr = new BlahDataPtr();
		Assert.IsFalse(consumer.IsPtrValid(ptr));
		producer.Add();
		Assert.IsFalse(consumer.IsPtrValid(ptr));
		producer.Add();
		Assert.IsFalse(consumer.IsPtrValid(ptr));
	}

	[Test]
	public void TestPtrFromDiffPool_PtrInvalid()
	{
		var context   = new BlahPoolsContext();
		var producerA = context.GetDataFull<MockDataEntry>();
		var consumerA = context.GetDataGetter<MockDataEntry>();
		var producerB = context.GetDataFull<MockDataEntryB>();
		var consumerB = context.GetDataGetter<MockDataEntryB>();

		producerA.Add(out var ptrA);
		producerB.Add(out var ptrB);
		
		Assert.IsTrue(consumerA.IsPtrValid(ptrA));
		Assert.IsTrue(consumerB.IsPtrValid(ptrB));
		
		Assert.IsFalse(consumerA.IsPtrValid(ptrB));
		Assert.IsFalse(consumerB.IsPtrValid(ptrA));
	}
	
	[Test]
	public void TestAddWithPtr_ValuesEqual()
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();

		producer.Add(out var ptr1).Val = 1;
		Assert.AreEqual(1, consumer.Get(ptr1).Val);
		
		producer.Add(out var ptr2).Val = 2;
		Assert.AreEqual(1, consumer.Get(ptr1).Val);
		Assert.AreEqual(2, consumer.Get(ptr2).Val);
		
		producer.Add(out var ptr3).Val = 3;
		Assert.AreEqual(1, consumer.Get(ptr1).Val);
		Assert.AreEqual(2, consumer.Get(ptr2).Val);
		Assert.AreEqual(3, consumer.Get(ptr3).Val);
	}

	[Test]
	public void TestGetPtr_ValuesEqual()
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();

		var ptr1 = new BlahDataPtr();
		var ptr2 = new BlahDataPtr();
		var ptr3 = new BlahDataPtr();
		
		Assert.IsFalse(consumer.IsPtrValid(ptr1));
		Assert.IsFalse(consumer.IsPtrValid(ptr2));
		Assert.IsFalse(consumer.IsPtrValid(ptr3));
		
		producer.Add().Val = 1;
		producer.Add().Val = 2;
		producer.Add().Val = 3;
        
		Assert.IsFalse(consumer.IsPtrValid(ptr1));
		Assert.IsFalse(consumer.IsPtrValid(ptr2));
		Assert.IsFalse(consumer.IsPtrValid(ptr3));

		foreach (ref var data in consumer)
			if (data.Val == 1)
				ptr1 = consumer.GetPtr();
			else if (data.Val == 2)
				ptr2 = consumer.GetPtr();
			else if (data.Val == 3)
				ptr3 = consumer.GetPtr();
        
		Assert.AreEqual(1, consumer.Get(ptr1).Val);
		Assert.AreEqual(2, consumer.Get(ptr2).Val);
		Assert.AreEqual(3, consumer.Get(ptr3).Val);
	}

	[TestCase(0, 1, 2, 3)]
	[TestCase(3, 2, 1, 0)]
	[TestCase(1, 2, 0, 3)]
	[TestCase(2, 1, 3, 0)]
	public void TestForeachRemoveOne_PtrInvalid(params int[] valsToRemove)
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();
		
		producer.Add(out var ptr0).Val = 0;
		producer.Add(out var ptr1).Val = 1;
		producer.Add(out var ptr2).Val = 2;
		producer.Add(out var ptr3).Val = 3;
		var ptrs = new[]
		{
			ptr0, ptr1, ptr2, ptr3,
		};

		var removedVals = new HashSet<int>();
		for (var valIdxToRemove = 0; valIdxToRemove < valsToRemove.Length; valIdxToRemove++)
		{
			foreach (ref var data in consumer)
				if (data.Val == valsToRemove[valIdxToRemove])
				{
					removedVals.Add(data.Val);
					consumer.Remove();
					break;
				}

			for (var i = 0; i < ptrs.Length; i++)
				if (removedVals.Contains(i))
				{
					Assert.IsFalse(consumer.IsPtrValid(ptrs[i]));
				}
				else
				{
					Assert.IsTrue(consumer.IsPtrValid(ptrs[i]));
					Assert.AreEqual(i, consumer.Get(ptrs[i]).Val);
				}
		}
	}

	[TestCase(0, 1)]
	[TestCase(0, 2)]
	[TestCase(0, 3)]
	[TestCase(1, 2)]
	[TestCase(1, 3)]
	[TestCase(2, 3)]
	[TestCase(0, 1, 2)]
	[TestCase(0, 1, 3)]
	[TestCase(0, 2, 3)]
	[TestCase(0, 1, 2, 3)]
	public void TestForeachRemoveBatch_PtrsInvalid(params int[] rawValsToRemove)
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();

		producer.Add(out var ptr0).Val = 0;
		producer.Add(out var ptr1).Val = 1;
		producer.Add(out var ptr2).Val = 2;
		producer.Add(out var ptr3).Val = 3;
		var ptrs = new[]
		{
			ptr0, ptr1, ptr2, ptr3,
		};

		var valsToRemove = new HashSet<int>(rawValsToRemove);
		foreach (ref var data in consumer)
			if (valsToRemove.Contains(data.Val))
				consumer.Remove();

		for (var i = 0; i < ptrs.Length; i++)
			if (valsToRemove.Contains(i))
			{
				Assert.IsFalse(consumer.IsPtrValid(ptrs[i]));
			}
			else
			{
				Assert.IsTrue(consumer.IsPtrValid(ptrs[i]));
				Assert.AreEqual(i, consumer.Get(ptrs[i]).Val);
			}
	}
    
	[TestCase(0, 1, 2, 3)]
	[TestCase(3, 2, 1, 0)]
	[TestCase(1, 2, 0, 3)]
	[TestCase(2, 1, 3, 0)]
	public void TestPtrRemoveOne_PtrInvalid(params int[] valsToRemove)
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();
		
		producer.Add(out var ptr0).Val = 0;
		producer.Add(out var ptr1).Val = 1;
		producer.Add(out var ptr2).Val = 2;
		producer.Add(out var ptr3).Val = 3;
		var ptrs = new[]
		{
			ptr0, ptr1, ptr2, ptr3,
		};

		var removedVals = new HashSet<int>();
		for (var valIdxToRemove = 0; valIdxToRemove < valsToRemove.Length; valIdxToRemove++)
		{
			removedVals.Add(valsToRemove[valIdxToRemove]);
			consumer.Remove(ptrs[valsToRemove[valIdxToRemove]]);

			for (var i = 0; i < ptrs.Length; i++)
				if (removedVals.Contains(i))
				{
					Assert.IsFalse(consumer.IsPtrValid(ptrs[i]));
				}
				else
				{
					Assert.IsTrue(consumer.IsPtrValid(ptrs[i]));
					Assert.AreEqual(i, consumer.Get(ptrs[i]).Val);
				}
		}
	}
	
	[TestCase(0, 1)]
	[TestCase(0, 2)]
	[TestCase(0, 3)]
	[TestCase(1, 2)]
	[TestCase(1, 3)]
	[TestCase(2, 3)]
	[TestCase(0, 1, 2)]
	[TestCase(0, 1, 3)]
	[TestCase(0, 2, 3)]
	[TestCase(0, 1, 2, 3)]
	public void TestPtrRemoveBatch_PtrsInvalid(params int[] rawValsToRemove)
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();

		producer.Add(out var ptr0).Val = 0;
		producer.Add(out var ptr1).Val = 1;
		producer.Add(out var ptr2).Val = 2;
		producer.Add(out var ptr3).Val = 3;
		var ptrs = new[]
		{
			ptr0, ptr1, ptr2, ptr3,
		};

		var valsToRemove = new HashSet<int>(rawValsToRemove);
		foreach (int val in rawValsToRemove)
			consumer.Remove(ptrs[val]);

		for (var i = 0; i < ptrs.Length; i++)
			if (valsToRemove.Contains(i))
			{
				Assert.IsFalse(consumer.IsPtrValid(ptrs[i]));
			}
			else
			{
				Assert.IsTrue(consumer.IsPtrValid(ptrs[i]));
				Assert.AreEqual(i, consumer.Get(ptrs[i]).Val);
			}
	}

	[Test]
	public void TestPtrAddRemoveAdd_PtrReUsed()
	{
		var context  = new BlahPoolsContext();
		var producer = context.GetDataFull<MockDataEntry>();
		var consumer = context.GetDataGetter<MockDataEntry>();

		producer.Add(out var ptrA);
		
		foreach (ref var data in consumer)
			consumer.Remove();

		producer.Add(out var ptrB);

		Assert.IsFalse(consumer.IsPtrValid(ptrA));
		Assert.IsTrue(consumer.IsPtrValid(ptrB));
		Assert.IsTrue(ptrA != ptrB);
		
		var type          = typeof(BlahDataPtr);
		var fieldEntryPtr = type.GetField("EntryPtr", BindingFlags.Instance | BindingFlags.NonPublic);
		var fieldGen      = type.GetField("Gen", BindingFlags.Instance | BindingFlags.NonPublic);

		var entryPtrA = (int)fieldEntryPtr.GetValue(ptrA);
		var genA      = (int)fieldGen.GetValue(ptrA);
		
		var entryPtrB = (int)fieldEntryPtr.GetValue(ptrB);
		var genB      = (int)fieldGen.GetValue(ptrB);
		
		Assert.AreEqual(entryPtrA, entryPtrB);
		Assert.AreNotEqual(genA, genB);
	}
}
}