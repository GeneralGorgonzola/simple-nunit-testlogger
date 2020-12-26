namespace SomeOtherXunitTests
{
    using System;
    using Xunit;

    public class TheXSuiteTests
    {
        [Fact]
        public void SomeOkTest()
        {

        }

        [Fact]
        public void SomeBadTest()
        {
            Assert.False(true);
        }

        [Fact]
        public void ArghTest()
        {
            throw new ArgumentException();
        }
    }
}
