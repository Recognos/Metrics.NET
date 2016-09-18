using System;
using System.Linq;
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

		/// <summary>
		/// Creates a new <see cref="InfluxdbUdpWriter"/> with the specified URI.
		/// </summary>
		/// <param name="influxDbUri">The UDP URI of the InfluxDB server. Should be in the format: net.udp//{host}:{port}/</param>
		public InfluxdbUdpWriter(Uri influxDbUri)
			: this(new InfluxConfig(influxDbUri)) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbUdpWriter"/> with the specified configuration and batch size.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbUdpWriter(InfluxConfig config, Int32 batchSize = 0)
			: base(config, batchSize) {

			if (String.IsNullOrEmpty(config.Hostname))
				throw new ArgumentNullException(nameof(config.Hostname));
			if ((config.Port ?? 0) == 0)
				throw new ArgumentNullException(nameof(config.Port), "Port is required for UDP connections.");
			if ((config.Precision ?? InfluxPrecision.Nanoseconds) != InfluxPrecision.Nanoseconds)
				throw new ArgumentException($"Timestamp precision for UDP connections must be Nanoseconds. Actual: {config.Precision}", nameof(config.Precision));
		}


		/// <summary>
		/// Gets the byte representation of the <see cref="InfluxBatch"/> in LineProtocol syntax using UTF8 encoding.
		/// </summary>
		/// <param name="batch">The batch to get the bytes for.</param>
		/// <returns>The byte representation of the batch.</returns>
		protected override Byte[] GetBatchBytes(InfluxBatch batch) {
			// UDP only supports ns precision
			var strBatch = batch.ToLineProtocol(InfluxPrecision.Nanoseconds);
			var bytes = Encoding.UTF8.GetBytes(strBatch);
			return bytes;
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
					return Encoding.UTF8.GetBytes(result.ToString());
				}
			} catch (Exception ex) {
				String firstNLines = "\n" + String.Join("\n", Encoding.UTF8.GetString(bytes).Split('\n').Take(5)) + "\n";
				MetricsErrorHandler.Handle(ex, $"Error while uploading {Batch.Count} measurements ({fmtSize(bytes.Length)}) to InfluxDB over UDP [net.udp://{config.Hostname}:{config.Port.Value}/] - Ensure that the message size is less than the UDP send buffer size (usually 8-64KB), and reduce the BatchSize on the InfluxdbWriter if necessary. - First 5 lines: {firstNLines}");
				return Encoding.UTF8.GetBytes(0.ToString());
			}
		}
	}
}
