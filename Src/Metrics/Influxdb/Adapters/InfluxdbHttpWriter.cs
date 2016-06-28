using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb.Adapters
{
	/// <summary>
	/// This class writes <see cref="InfluxRecord"/>s formatted in the LineProtocol to the InfluxDB server using HTTP POST.
	/// </summary>
	public class InfluxdbHttpWriter : InfluxdbLineWriter
	{

		protected readonly Uri influxDbUri;


		/// <summary>
		/// Creates a new <see cref="InfluxdbHttpWriter"/> with the specified URI.
		/// </summary>
		/// <param name="influxDbUri">The HTTP URI of the InfluxDB server.</param>
		public InfluxdbHttpWriter(Uri influxDbUri)
			: this(new InfluxConfig(influxDbUri)) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbHttpWriter"/> with the specified configuration and batch size.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbHttpWriter(InfluxConfig config, Int32 batchSize = 0)
			: base(config, batchSize) {

			if (String.IsNullOrEmpty(config.Hostname))
				throw new ArgumentNullException(nameof(config.Hostname));
			if (String.IsNullOrEmpty(config.Database))
				throw new ArgumentNullException(nameof(config.Database));

			this.influxDbUri = FormatInfluxUri(config);
			if (influxDbUri == null)
				throw new ArgumentNullException(nameof(influxDbUri));
			if (influxDbUri.Scheme != Uri.UriSchemeHttp && influxDbUri.Scheme != Uri.UriSchemeHttps)
				throw new ArgumentException($"The URI scheme must be either http or https. Scheme: {influxDbUri.Scheme}", nameof(influxDbUri));
		}


		/// <summary>
		/// Creates an HTTP URI for InfluxDB using the values specified in the <see cref="InfluxConfig"/> object.
		/// </summary>
		/// <param name="config">The configuration object to get the relevant fields to build the HTTP URI from.</param>
		/// <returns>A new InfluxDB URI using the configuration specified in the <paramref name="config"/> parameter.</returns>
		protected static Uri FormatInfluxUri(InfluxConfig config) {
			UInt16 port = (config.Port ?? 0) > 0 ? config.Port.Value : InfluxConfig.Default.PortHttp;
			return InfluxUtils.FormatInfluxUri(InfluxUtils.SchemeHttp, config.Hostname, port, config.Database, config.Username, config.Password, config.RetentionPolicy, config.Precision);
		}

		/// <summary>
		/// Writes the byte array to the InfluxDB server in a single HTTP POST operation.
		/// </summary>
		/// <param name="bytes">The bytes to write to the InfluxDB server.</param>
		/// <returns>The HTTP response from the server after writing the message.</returns>
		protected override Byte[] WriteToTransport(Byte[] bytes) {
			try {
				using (var client = new WebClient()) {
					var result = client.UploadData(influxDbUri, bytes);
					return result;
				}
			} catch (WebException ex) {
				String response = new StreamReader(ex.Response?.GetResponseStream() ?? Stream.Null).ReadToEnd();
				String firstNLines = "\n" + String.Join("\n", Encoding.UTF8.GetString(bytes).Split('\n').Take(5)) + "\n";
				MetricsErrorHandler.Handle(ex, $"Error while uploading {Batch.Count} measurements ({fmtSize(bytes.Length)}) to InfluxDB over HTTP [{influxDbUri}] [ResponseStatus: {ex.Status}] [Response: {response}] - First 5 lines: {firstNLines}");
				return Encoding.UTF8.GetBytes(response);
			}
		}
	}
}
