namespace SomeNunitTests
{
    using System;
    using NUnit.Framework;

    public class TheTestSuite
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FirstTest()
        {
            Assert.Pass();
        }

        [Test]
        public void SecondTest()
        {
            Assert.Fail();
        }

        [Test]
        public void SomeTestThatThrows()
        {
            throw new InvalidOperationException();
        }
    }
}