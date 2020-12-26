namespace SomeOtherXunitTests
{
    using System;
    using Xunit;

    public class FailFailFail
    {
        [Fact]
        public void AllIsWellTest()
        {

        }

        [Fact]
        public void BadNewsTest()
        {
            Assert.False(true);
        }

        [Fact]
        public void CrashTest()
        {
            throw new ArgumentException();
        }
    }
}
