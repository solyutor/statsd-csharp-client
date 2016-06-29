using System;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace SharpStatsd.Tests
{
    [TestFixture]
    public class PerformanceTests
    {
        public class Result
        {
            private readonly int _iterations;
            private readonly long _started;
            private long _finished;
            private readonly int _gc0Before;
            private int _gc0After;

            public Result(int iterations)
            {
                _iterations = iterations;
                _started = Stopwatch.GetTimestamp();
                _gc0Before = GC.CollectionCount(0);
            }

            public void Stop()
            {
                _finished = Stopwatch.GetTimestamp();
                _gc0After = GC.CollectionCount(0);
            }

            public TimeSpan Spanned => TimeSpan.FromTicks(_finished - _started);

            public int GC0Collections => _gc0After - _gc0Before;

            public double OpSec => _iterations / Spanned.TotalSeconds;

            public override string ToString()
            {
                return $"Spanned={Spanned}; {OpSec:N3}; op/sec GC0={GC0Collections}";
            }
        }

        [Test]
        public void Fee()
        {
            Console.WriteLine($": = {Encoding.UTF8.GetBytes(":")[0]}");
            Console.WriteLine($"| = {Encoding.UTF8.GetBytes("|")[0]}");
        }

        [Test]
        public void Compare_performance()
        {
            int iterations = 10 * 1000 * 1000;
            var prefix = "DC1.Service.Request";

            //Warm-up
            TestOriginal(10, prefix);
            TestModern(10, prefix);

            var original = TestOriginal(iterations, prefix);
            var modern = TestModern(iterations, prefix);

            Console.WriteLine($"Original:   {original}");
            Console.WriteLine($"Refactored: {modern}");
        }

        private object TestModern(int iterations, string prefix)
        {
            var client = new Statsd(new NullOutputChannel(), prefix);
            var result = new Result(iterations);
            for (int i = 0; i < iterations; i++)
            {
                client.LogGauge("Perfomanto", i);
            }
            result.Stop();

            return result;
        }

        private Result TestOriginal(int iterations, string prefix)
        {
            var client = new StatsdClient.Statsd(null, 0, prefix: prefix, outputChannel: null); //gives us a null output channel.
            var result = new Result(iterations);
            for (int i = 0; i < iterations; i++)
            {
                client.LogGauge("Perfomanto", i);
            }
            result.Stop();

            return result;
        }
    }
}