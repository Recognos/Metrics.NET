using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb.Adapters
{
	/// <summary>
	/// This class writes <see cref="InfluxRecord"/>s formatted in the LineProtocol to the InfluxDB server using the UDP transport.
	/// </summary>
	public class InfluxdbUdpWriter : InfluxdbLineWriter
	{

		protected readonly InfluxConfig config;


		/// <summary>
		/// Creates a new <see cref="InfluxdbUdpWriter"/> with the specified URI.
		/// </summary>
		/// <param name="influxDbUri">The UDP URI of the InfluxDB server. Should be: net.udp//{host}:{port}/</param>
		public InfluxdbUdpWriter(Uri influxDbUri)
			: this(new InfluxConfig(influxDbUri)) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbUdpWriter"/> with the specified URI.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbUdpWriter(InfluxConfig config, Int32 batchSize = 0)
			: base(batchSize) {
			this.config = config;
			if (config == null)
				throw new ArgumentNullException(nameof(config));
			if (String.IsNullOrEmpty(config.Hostname))
				throw new ArgumentNullException(nameof(config.Hostname));
			if ((config.Port ?? 0) == 0)
				throw new ArgumentNullException(nameof(config.Port), "Port is required for UDP connections.");
		}

		public override void Flush() {
			// UDP only supports ns precision
			Batch.ForEach(r => r.Precision = InfluxPrecision.Nanoseconds);
			base.Flush();
		}

		/// <summary>
		/// Writes the byte array to the InfluxDB server in a single UDP send operation.
		/// </summary>
		/// <param name="bytes">The bytes to write to the InfluxDB server.</param>
		/// <returns>The HTTP response from the server after writing the message.</returns>
		protected override Byte[] WriteToTransport(Byte[] bytes) {
			try {
				using (var client = new UdpClient()) {
					int result = client.Send(bytes, bytes.Length, config.Hostname, config.Port.Value);
					return BitConverter.GetBytes(result);
				}
			} catch (Exception ex) {
				MetricsErrorHandler.Handle(ex, $"Error while uploading {Batch.Count} measurements to InfluxDB over UDP [net.udp://{config.Hostname}:{config.Port.Value}/]");
				return BitConverter.GetBytes(0);
			}
		}
	}
}
