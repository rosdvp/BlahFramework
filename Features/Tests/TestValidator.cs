using NUnit.Framework;

namespace Blah.Features.Tests
{
internal class TestValidator
{
	[Test]
	public void Test_Valid_NoThrows()
	{
		BlahFeaturesValidator.Validate(new MockFeatureA());
		BlahFeaturesValidator.Validate(new MockFeatureB());
	}

	[Test]
	public void Test_InvalidSignalConsumer_Throws()
	{
		var feature = new MockFeatureInvalidSignalConsumer();
		try
		{
			BlahFeaturesValidator.Validate(feature);
			Assert.Fail();
		}
		catch (BlahFeatureValidatorException exc)
		{
			Assert.AreEqual(feature, exc.Feature);
			Assert.AreEqual(typeof(MockFeatureInvalidSignalConsumer.MockInvalidSignal), exc.InvalidType);
		}
	}
	
	[Test]
	public void Test_InvalidSignalProducer_Throws()
	{
		var feature = new MockFeatureInvalidSignalProducer();
		try
		{
			BlahFeaturesValidator.Validate(feature);
			Assert.Fail();
		}
		catch (BlahFeatureValidatorException exc)
		{
			Assert.AreEqual(feature, exc.Feature);
			Assert.AreEqual(typeof(MockFeatureInvalidSignalProducer.MockInvalidSignal), exc.InvalidType);
		}
	}
	
	[Test]
	public void Test_InvalidDataConsumer_Throws()
	{
		var feature = new MockFeatureInvalidDataConsumer();
		try
		{
			BlahFeaturesValidator.Validate(feature);
			Assert.Fail();
		}
		catch (BlahFeatureValidatorException exc)
		{
			Assert.AreEqual(feature, exc.Feature);
			Assert.AreEqual(typeof(MockFeatureInvalidDataConsumer.MockInvalidData), exc.InvalidType);
		}
	}
	
	[Test]
	public void Test_InvalidDataProducer_Throws()
	{
		var feature = new MockFeatureInvalidDataProducer();
		try
		{
			BlahFeaturesValidator.Validate(feature);
			Assert.Fail();
		}
		catch (BlahFeatureValidatorException exc)
		{
			Assert.AreEqual(feature, exc.Feature);
			Assert.AreEqual(typeof(MockFeatureInvalidDataProducer.MockInvalidData), exc.InvalidType);
		}
	}

	[Test]
	public void Test_InvalidService_Throws()
	{
		var feature = new MockFeatureInvalidService();
		try
		{
			BlahFeaturesValidator.Validate(feature);
			Assert.Fail();
		}
		catch (BlahFeatureValidatorException exc)
		{
			Assert.AreEqual(feature, exc.Feature);
			Assert.AreEqual(typeof(MockFeatureInvalidService.MockInvalidService), exc.InvalidType);
		}
	}

	[Test]
	public void Test_InvalidExcessiveConsumer_Throws()
	{
		var feature = new MockFeatureExcessiveSignalConsumer();
		try
		{
			BlahFeaturesValidator.Validate(feature);
			Assert.Fail();
		}
		catch (BlahFeatureValidatorException exc)
		{
			Assert.AreEqual(feature, exc.Feature);
			Assert.AreEqual(typeof(MockFeatureExcessiveSignalConsumer.MockInvalidSignal), exc.InvalidType);
		}
	}
}
}