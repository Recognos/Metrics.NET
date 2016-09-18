using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Metrics.Json;
using Metrics.Logging;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb.Adapters
{
	/// <summary>
	/// This class writes <see cref="InfluxRecord"/>s formatted as a JSON object to the InfluxDB server using HTTP POST.
	/// NOTE: This protocol is only supported in InfluxDB version v0.9.1 and earlier.
	/// </summary>
	public class InfluxdbJsonWriter : InfluxdbWriter
	{

		private static readonly ILog log = LogProvider.GetCurrentClassLogger();

		protected readonly InfluxConfig config;
		protected readonly Uri influxDbUri;


		/// <summary>
		/// Creates a new <see cref="InfluxdbHttpWriter"/> with the specified URI.
		/// NOTE: This protocol is only supported in InfluxDB version v0.9.1 and earlier.
		/// </summary>
		/// <param name="influxDbUri">The HTTP URI of the InfluxDB server.</param>
		public InfluxdbJsonWriter(Uri influxDbUri)
			: this(new InfluxConfig(influxDbUri)) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbHttpWriter"/> with the specified URI.
		/// NOTE: This protocol is only supported in InfluxDB version v0.9.1 and earlier.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbJsonWriter(InfluxConfig config, Int32 batchSize = 0)
			: base(batchSize) {
			this.config = config;
			if (config == null)
				throw new ArgumentNullException(nameof(config));
			if (String.IsNullOrEmpty(config.Database))
				throw new ArgumentNullException(nameof(config.Database));
			if (config.Precision != InfluxPrecision.Seconds)
				log.Warn($"InfluxDB timestamp precision '{config.Precision}' is not supported by the JSON protocol, defaulting to {InfluxPrecision.Seconds}.");

			this.influxDbUri = FormatInfluxUri(config);
			if (influxDbUri == null)
				throw new ArgumentNullException(nameof(influxDbUri));
			if (influxDbUri.Scheme != Uri.UriSchemeHttp && influxDbUri.Scheme != Uri.UriSchemeHttps)
				throw new ArgumentException($"The URI scheme must be either http or https. Scheme: {influxDbUri.Scheme}", nameof(influxDbUri));
		}


		/// <summary>
		/// Creates an HTTP JSON URI for InfluxDB using the values specified in the <see cref="InfluxConfig"/> object.
		/// </summary>
		/// <param name="config">The configuration object to get the relevant fields to build the HTTP URI from.</param>
		/// <returns>A new InfluxDB JSON URI using the configuration specified in the <paramref name="config"/> parameter.</returns>
		protected Uri FormatInfluxUri(InfluxConfig config) {
			InfluxPrecision prec = config.Precision ?? InfluxConfig.Default.Precision;
			return new Uri($@"http://{config.Hostname}:{config.Port}/db/{config.Database}/series?u={config.Username}&p={config.Password}&time_precision={prec.ToShortName()}");
		}


		/// <summary>
		/// Gets the byte representation of the <see cref="InfluxBatch"/> in JSON syntax using UTF8 encoding.
		/// </summary>
		/// <param name="batch">The batch to get the bytes for.</param>
		/// <returns>The byte representation of the batch.</returns>
		protected override Byte[] GetBatchBytes(InfluxBatch batch) {
			var strBatch = ToJson(batch);
			var bytes = Encoding.UTF8.GetBytes(strBatch);
			System.Diagnostics.Debug.WriteLine($"[NEW JSON]:\n{strBatch}");
			return bytes;
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
				MetricsErrorHandler.Handle(ex, $"Error while uploading {Batch.Count} measurements ({fmtSize(bytes.Length)}) to InfluxDB over JSON HTTP [{influxDbUri}] [ResponseStatus: {ex.Status}] [Response: {response}] - First 5 lines: {firstNLines}");
				return Encoding.UTF8.GetBytes(response);
			}
		}


		#region Format JSON Object Methods

		private static String ToJson(InfluxBatch batch) {
			return new CollectionJsonValue(batch.Select(r => ToJsonObject(r))).AsJson();
		}

		private static JsonObject ToJsonObject(InfluxRecord record) {
			if (record == null)
				throw new ArgumentNullException(nameof(record));
			if (String.IsNullOrWhiteSpace(record.Name))
				throw new ArgumentNullException(nameof(record.Name), "The measurement name must be specified.");
			if (record.Fields.Count == 0)
				throw new ArgumentNullException(nameof(record.Fields), $"Must specify at least one field. Metric name: {record.Name}");

			var cols = record.Tags.Select(t => t.Key).Concat(record.Fields.Select(f => f.Key));
			var data = record.Tags.Select(t => t.Value).Concat(record.Fields.Select(f => f.Value)).Select(v => FormatValue(v));
			return ToJsonObject(record.Name, record.Timestamp ?? DateTime.Now, cols, data);
		}

		private static JsonObject ToJsonObject(String name, DateTime timestamp, IEnumerable<String> columns, IEnumerable<JsonValue> data) {
			var cols   = new[] { "time" }.Concat(columns);
			var points = new[] { new LongJsonValue(ToUnixTime(timestamp)) }.Concat(data);

			return new JsonObject(new[] {
				new JsonProperty("name", name),
				new JsonProperty("columns", cols),
				new JsonProperty("points", new JsonValueArray(new[] { new JsonValueArray(points) }))
			});
		}

		/// <summary>
		/// Formats the field value in the appropriate line protocol format based on the type of the value object.
		/// The value type must be a string, boolean, or integral or floating-point type.
		/// </summary>
		/// <param name="value">The field value to format.</param>
		/// <returns>The field value formatted as a string used in the line protocol format.</returns>
		private static JsonValue FormatValue(Object value) {
			Type type = value?.GetType();
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (!InfluxUtils.IsValidValueType(type))
				throw new ArgumentException(nameof(value), $"Value is not one of the supported types: {type} - Valid types: {String.Join(", ", InfluxUtils.ValidValueTypes.Select(t => t.Name))}");

			if (InfluxUtils.IsIntegralType(type))
				return FormatValue(Convert.ToInt64(value));
			if (InfluxUtils.IsFloatingPointType(type))
				return FormatValue(Convert.ToDouble(value));
			if (value is String)
				return FormatValue((String)value);
			if (value is Char)
				return FormatValue(value.ToString());
			return FormatValue(value.ToString());
		}

		private static JsonValue FormatValue(Int64 value) {
			return new LongJsonValue(value);
		}

		private static JsonValue FormatValue(Double value) {
			return new DoubleJsonValue(value);
		}

		private static JsonValue FormatValue(String value) {
			return new StringJsonValue(value);
		}

		private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		private static long ToUnixTime(DateTime datetime) {
			return Convert.ToInt64((datetime.ToUniversalTime() - unixEpoch).TotalSeconds);
		}

		#endregion

	}
}
