using System;
using System.Linq;
using Metrics.Influxdb.Adapters;

namespace Metrics.Influxdb.Model
{
	/// <summary>
	/// Configuration for the InfluxDB reporter.
	/// </summary>
	public class InfluxConfig
	{

		#region Default Values

		/// <summary>
		/// Default InfluxDB configuration settings.
		/// </summary>
		public static class Default
		{
			/// <summary>
			/// The default port when using the HTTP protocol to connect to the InfluxDB server. This value is: 8086
			/// </summary>
			public static UInt16 PortHttp { get; }

			/// <summary>
			/// The default <see cref="InfluxPrecision"/> specifier value. This value is: <see cref="InfluxPrecision.Seconds"/>
			/// </summary>
			public static InfluxPrecision Precision { get; }

			static Default() {
				PortHttp = 8086;
				Precision = InfluxPrecision.Seconds;
			}
		}

		#endregion

		#region InfluxDB Server Connection Settings

		/// <summary>
		/// The hostname of the InfluxDB server.
		/// </summary>
		public String Hostname { get; set; }

		/// <summary>
		/// The port of the InfluxDB server. Set to null to use the default
		/// port for the chosen transport and protocol, if there is one.
		/// </summary>
		public UInt16? Port { get; set; }

		/// <summary>
		/// The name of the database on the InfluxDB server to write the datapoints to.
		/// </summary>
		public String Database { get; set; }

		/// <summary>
		/// The username used to authenticate with the InfluxDB server, if authentication is required.
		/// </summary>
		public String Username { get; set; }

		/// <summary>
		/// The password used to authenticate with the InfluxDB server, if authentication is required.
		/// </summary>
		public String Password { get; set; }

		/// <summary>
		/// The retention policy to use when writing datapoints to the InfluxDB database.
		/// If this is null, the database's default retention policy is used.
		/// </summary>
		public String RetentionPolicy { get; set; }

		/// <summary>
		/// The precision of the timestamp value in the line protocol syntax.
		/// It is recommended to use as large a precision as possible to improve compression and bandwidth usage.
		/// </summary>
		public InfluxPrecision? Precision { get; set; }

		#endregion

		#region InfluxDB Adapter Instances

		/// <summary>
		/// The <see cref="InfluxdbConverter"/> is used to convert metric values into <see cref="InfluxRecord"/> objects.
		/// </summary>
		public InfluxdbConverter Converter { get; set; }

		/// <summary>
		/// The <see cref="InfluxdbFormatter"/> is used to format the context and metric names into strings which are
		/// used as the table name to insert <see cref="InfluxRecord"/>s into. This also optionally formats the tag
		/// and field keys into column names by converting the case and replacing spaces with another character.
		/// </summary>
		public InfluxdbFormatter Formatter { get; set; }

		/// <summary>
		/// The <see cref="InfluxdbWriter"/> used to write formatted <see cref="InfluxRecord"/>s to the database.
		/// </summary>
		public InfluxdbWriter Writer { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new InfluxDB configuration object with default values.
		/// </summary>
		public InfluxConfig() 
			: this(null, null) {
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with the specified hostname and database.
		/// </summary>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		public InfluxConfig(String host, String database)
			: this(host, null, database) {
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with the specified hostname, database, and precision.
		/// </summary>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		public InfluxConfig(String host, String database, InfluxPrecision? precision)
			: this(host, null, database, null, precision) {
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with the specified hostname, database, retention policy, and precision.
		/// </summary>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		public InfluxConfig(String host, String database, String retentionPolicy, InfluxPrecision? precision)
			: this(host, null, database, retentionPolicy, precision) {
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with the specified hostname, port, and database.
		/// </summary>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		public InfluxConfig(String host, UInt16? port, String database)
			: this(host, port, database, null, null) {
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with the specified hostname, port, database, retention policy, and precision.
		/// </summary>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		public InfluxConfig(String host, UInt16? port, String database, String retentionPolicy, InfluxPrecision? precision)
			: this(host, port, database, null, null, retentionPolicy, precision) {
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with the specified hostname, port, database, retention policy, precision, and credentials.
		/// </summary>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		/// <param name="username">The username to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="password">The password to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		public InfluxConfig(String host, UInt16? port, String database, String username, String password, String retentionPolicy, InfluxPrecision? precision) {
			this.Hostname = host;
			this.Port = port;
			this.Database = database;
			this.Username = username;
			this.Password = password;
			this.RetentionPolicy = retentionPolicy;
			this.Precision = precision;
		}

		/// <summary>
		/// Creates a new InfluxDB configuration object with values initialized from the URI.
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public InfluxConfig(Uri influxDbUri) {
			if (influxDbUri == null)
				throw new ArgumentNullException(nameof(influxDbUri));
			Hostname = influxDbUri.Host;
			Port = (UInt16)influxDbUri.Port;
			var queryKvps = influxDbUri.ParseQueryString();
			foreach (var kvp in queryKvps.Where(k => !String.IsNullOrEmpty(k.Value))) {
				if (kvp.Key.ToLower() == "db") Database = kvp.Value;
				if (kvp.Key.ToLower() == "rp") RetentionPolicy = kvp.Value;
				if (kvp.Key.ToLower() == "u") Username = kvp.Value;
				if (kvp.Key.ToLower() == "p") Password = kvp.Value;
				if (kvp.Key.ToLower() == "precision") Precision = InfluxUtils.FromShortName(kvp.Value);
			}
		}

		#endregion

	}
}
