namespace SomeOtherXunitTests
{
    using System;
    using Xunit;

    public class TheOtherXSuiteTests
    {
        [Fact]
        public void ThisIsFineTest()
        {

        }

        [Fact]
        public void ThisIsBadTest()
        {
            Assert.False(true);
        }

        [Fact]
        public void DeathTest()
        {
            throw new ArgumentException();
        }
    }
}
