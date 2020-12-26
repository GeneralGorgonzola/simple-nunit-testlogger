namespace SomeNunitTests
{
    using System;
    using NUnit.Framework;

    public class MoreTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SomeFineTest()
        {
            Assert.Pass();
        }

        [Test]
        public void SomeBuggyTest()
        {
            Assert.Fail();
        }

        [Test]
        public void UnpredictableTest()
        {
            throw new InvalidOperationException();
        }
    }
}