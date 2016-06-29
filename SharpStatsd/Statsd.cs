using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharpStatsd
{
    /// <summary>
    /// The statsd client library.
    /// </summary>
    public class Statsd : IStatsd
    {
        private readonly IOutputChannel _outputChannel;

        private readonly MemoryStream _stream;
        private readonly long _initialPosition;
        private readonly StreamWriter _writer;

        /// <summary>
        /// Creates a new instance of the Statsd client.
        /// </summary>
        /// <param name="prefix">A string prefix to prepend to every metric.</param>
        /// <param name="outputChannel">Optional output channel (useful for mocking / testing).</param>
        public Statsd(IOutputChannel outputChannel, string prefix = null)
        {
            if (outputChannel == null)
            {
                throw new ArgumentNullException(nameof(outputChannel));
            }
            _outputChannel = outputChannel;

            _stream = new MemoryStream(256);
            _writer = new StreamWriter(_stream, new UTF8Encoding(false), 128, true);

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            _writer.Write(prefix);
            if (!prefix.EndsWith("."))
            {
                _writer.Write(".");
            }
            _writer.Flush();
            _initialPosition = _stream.Position;
        }

        /// <summary>
        /// Log a counter.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="count">The counter value (defaults to 1).</param>
        public void LogCount(string name, int count = 1)
        {
            SendMetric(MetricType.COUNT, name, count);
        }

        /// <summary>
        /// Log a timing / latency
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="milliseconds">The duration, in milliseconds, for this metric.</param>
        public void LogTiming(string name, int milliseconds)
        {
            SendMetric(MetricType.TIMING, name, milliseconds);
        }

        /// <summary>
        /// Log a gauge.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="value">The value for this gauge</param>
        public void LogGauge(string name, int value)
        {
            SendMetric(MetricType.GAUGE, name, value);
        }

        /// <summary>
        /// Log to a set
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The value to log.</param>
        /// <remarks>Logging to a set is about counting the number
        /// of occurrences of each event.</remarks>
        public void LogSet(string name, int value)
        {
            SendMetric(MetricType.SET, name, value);
        }

        /// <summary>
        /// Log a calendargram metric
        /// </summary>
        /// <param name="name">The metric namespace</param>
        /// <param name="value">The unique value to be counted in the time period</param>
        /// <param name="period">The time period, can be one of h,d,dow,w,m</param>
        public void LogCalendargram(string name, string value, string period)
        {
            SendMetric(MetricType.CALENDARGRAM, name, value, period);
        }

        /// <summary>
        /// Log a calendargram metric
        /// </summary>
        /// <param name="name">The metric namespace</param>
        /// <param name="value">The unique value to be counted in the time period</param>
        /// <param name="period">The time period, can be one of h,d,dow,w,m</param>
        public void LogCalendargram(string name, int value, string period)
        {
            SendMetric(MetricType.CALENDARGRAM, name, value, period);
        }

        /// <summary>
        /// Log a raw metric that will not get aggregated on the server.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="epoch">(optional) The epoch timestamp. Leave this blank to have the server assign an epoch for you.</param>
        public void LogRaw(string name, int value, long? epoch = null)
        {
            SendMetric(MetricType.RAW, name, value, epoch.HasValue ? epoch.ToString() : (string)null);
        }

        private void SendMetric(string metricType, string name, int value, string postFix = null)
        {
            if (value < 0)
            {
                Trace.TraceWarning(String.Format("Metric value for {0} was less than zero: {1}. Not sending.", name, value));
                return;
            }
            SendMetric(metricType, name, value.ToString(), postFix);
        }

        private void SendMetric(string metricType, string name, string value, string postFix = null)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            _stream.SetLength(_initialPosition); // prefix is already written with dot ending
            _writer.Write(name);
            _writer.Write(":");
            _writer.Write(value);
            _writer.Write("|");
            _writer.Write(metricType);
            _writer.Flush();

            _outputChannel.Send(_stream.GetBuffer(), (int)_stream.Position);
        }
    }
}