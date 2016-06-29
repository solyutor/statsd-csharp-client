using System;
using NUnit.Framework;

namespace SharpStatsd.Tests
{
    [TestFixture]
    public class PerformanceTests
    {
        public class Result
        {
            public TimeSpan Spanned;
            public int GC0Collections;
            public double OpSec;
        }

        [Test]
        public void Compare_performance()
        {
        }
    }
}