namespace Metrics.Influxdb.Model
{
	/// <summary>
	/// The precision to format timestamp values in the InfluxDB LineProtocol syntax.
	/// </summary>
	public enum InfluxPrecision
	{
		Nanoseconds = 0,
		Microseconds,
		Milliseconds,
		Seconds,
		Minutes,
		Hours
	}
}
