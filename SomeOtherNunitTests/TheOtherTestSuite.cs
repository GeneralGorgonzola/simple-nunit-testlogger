namespace SomeNunitTests
{
    using System;
    using NUnit.Framework;

    public class TheOtherTestSuite
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SuccessTest()
        {
            Assert.Pass();
        }

        [Test]
        public void FailTest()
        {
            Assert.Fail();
        }

        [Test]
        public void CatastropheTest()
        {
            throw new InvalidOperationException();
        }
    }
}