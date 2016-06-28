using System;
using System.Diagnostics;
using System.Text;

namespace StatsdClient
{
    /// <summary>
    /// The statsd client library.
    /// </summary>
    public class Statsd : IStatsd
    {
        private readonly string _prefix;
        private IOutputChannel _outputChannel;

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

            _prefix = _prefix != null && _prefix.EndsWith(".")
                ? _prefix.Substring(0, _prefix.Length - 1)
                : _prefix;
        }

        /// <summary>
        /// Log a counter.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="count">The counter value (defaults to 1).</param>
        public void LogCount(string name, int count = 1)
        {
            SendMetric(MetricType.COUNT, name, _prefix, count);
        }

        /// <summary>
        /// Log a timing / latency
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="milliseconds">The duration, in milliseconds, for this metric.</param>
        public void LogTiming(string name, int milliseconds)
        {
            SendMetric(MetricType.TIMING, name, _prefix, milliseconds);
        }

        /// <summary>
        /// Log a gauge.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="value">The value for this gauge</param>
        public void LogGauge(string name, int value)
        {
            SendMetric(MetricType.GAUGE, name, _prefix, value);
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
            SendMetric(MetricType.SET, name, _prefix, value);
        }

        /// <summary>
        /// Log a calendargram metric
        /// </summary>
        /// <param name="name">The metric namespace</param>
        /// <param name="value">The unique value to be counted in the time period</param>
        /// <param name="period">The time period, can be one of h,d,dow,w,m</param>
        public void LogCalendargram(string name, string value, string period)
        {
            SendMetric(MetricType.CALENDARGRAM, name, _prefix, value, period);
        }

        /// <summary>
        /// Log a calendargram metric
        /// </summary>
        /// <param name="name">The metric namespace</param>
        /// <param name="value">The unique value to be counted in the time period</param>
        /// <param name="period">The time period, can be one of h,d,dow,w,m</param>
        public void LogCalendargram(string name, int value, string period)
        {
            SendMetric(MetricType.CALENDARGRAM, name, _prefix, value, period);
        }

        /// <summary>
        /// Log a raw metric that will not get aggregated on the server.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="value">The metric value.</param>
        /// <param name="epoch">(optional) The epoch timestamp. Leave this blank to have the server assign an epoch for you.</param>
        public void LogRaw(string name, int value, long? epoch = null)
        {
            SendMetric(MetricType.RAW, name, String.Empty, value, epoch.HasValue ? epoch.ToString() : (string)null);
        }

        private void SendMetric(string metricType, string name, string prefix, int value, string postFix = null)
        {
            if (value < 0)
            {
                Trace.TraceWarning(String.Format("Metric value for {0} was less than zero: {1}. Not sending.", name, value));
                return;
            }
            SendMetric(metricType, name, prefix, value.ToString(), postFix);
        }

        private void SendMetric(string metricType, string name, string prefix, string value, string postFix = null)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            string metric = PrepareMetric(metricType, name, prefix, value, postFix);
            byte[] payload = Encoding.UTF8.GetBytes(metric);

            _outputChannel.Send(payload, payload.Length);
        }

        /// <summary>
        /// Prepare a metric prior to sending it off ot the Graphite server.
        /// </summary>
        /// <param name="metricType"></param>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <param name="value"></param>
        /// <param name="postFix">A value to append to the end of the line.</param>
        /// <returns>The formatted metric</returns>
        protected virtual string PrepareMetric(string metricType, string name, string prefix, string value, string postFix = null)
        {
            return (String.IsNullOrEmpty(prefix) ? name : (prefix + "." + name))
                + ":" + value
                + "|" + metricType
                + (postFix == null ? String.Empty : "|" + postFix);
        }
    }
}