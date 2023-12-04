using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalNested
{
	[TestCase(new [] { 1 })]
	[TestCase(new [] { 1, 2 })]
	[TestCase(new [] { 1, 2, 3 })]
	public void TestIterating(int[] values)
	{
		var context  = new BlahPoolsContext();
		var consumer = context.GetSignalConsumer<MockSignalEntry>();
		var producer = context.GetSignalProducer<MockSignalEntry>();

		foreach (int val in values)
			producer.Add().Val = val;
		
		for (var iter = 0; iter < 3; iter++)
		{
			var visits = new int[values.Length * values.Length];

			foreach (var evA in consumer)
			foreach (var evB in consumer)
				visits[(evA.Val - 1) * values.Length + evB.Val - 1] += 1;

			for (var i = 0; i < visits.Length; i++)
				Assert.AreEqual(1, visits[i], $"idx {i}");
		}
	}

	[Test]
	public void TestAdd()
	{
		var context  = new BlahPoolsContext();
		var consumer = context.GetSignalConsumer<MockSignalEntry>();
		var producer = context.GetSignalProducer<MockSignalEntry>();

		var values = new[] { 1, 2, 3 };
		
		for (var iter = 0; iter < 3; iter++)
		{
			foreach (int val in values)
				producer.Add().Val = val;
			
			var visits = new int[values.Length * values.Length];

			foreach (var evA in consumer)
			foreach (var evB in consumer)
			{
				visits[(evA.Val - 1) * values.Length + evB.Val - 1] += 1;

				if (evA.Val == 2)
					producer.Add().Val = evB.Val + values.Length;
			}

			for (var i = 0; i < visits.Length; i++)
				Assert.AreEqual(1, visits[i], $"idx {i}");

			AssertHelper.CheckContent(consumer, 1, 2, 3, 4, 5, 6);
			
			context.OnNextFrame();
		}
	}
	
	
	[TestCase(2, 3, 0)]
	[TestCase(2, 3, 1)]
	[TestCase(2, null, 0)]
	[TestCase(null, null, 0)]
	public void TestRemove(int? valA, int? valB, int level)
	{
		var context  = new BlahPoolsContext();
		var consumer = context.GetSignalConsumer<MockSignalEntry>();
		var producer = context.GetSignalProducer<MockSignalEntry>();

		var values = new[] { 1, 2, 3 };
		
		for (var iter = 0; iter < 3; iter++)
		{
			var expectedValues = new List<int>(values);

			foreach (int val in values)
				producer.Add().Val = val;
			AssertHelper.CheckContent(consumer, values);

			var visits = new int[values.Length * values.Length];

			foreach (var evA in consumer)
			foreach (var evB in consumer)
			{
				visits[(evA.Val - 1) * values.Length + evB.Val - 1] += 1;

				bool isA = valA == null || evA.Val == valA;
				bool isB = valB == null || evB.Val == valB;

				if (isA && isB)
				{
					consumer.Remove(level);

					expectedValues.Remove(level == 0 ? evA.Val : evB.Val);
				}
			}

			for (var i = 0; i < visits.Length; i++)
				Assert.AreEqual(1, visits[i], $"idx {i}");

			AssertHelper.CheckContent(consumer, expectedValues.ToArray());
			
			context.OnNextFrame();
		}
	}
}
}