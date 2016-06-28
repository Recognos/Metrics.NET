using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb.Adapters
{
	/// <summary>
	/// This class provides functions to format the context name and metric
	/// name into a string used to identify the table to insert records into.
	/// The formatter is also used to format the <see cref="InfluxRecord"/>
	/// before writing it to the database. This also optionally formats the column
	/// names by converting the case and replacing spaces with another character.
	/// </summary>
	public abstract class InfluxdbFormatter
	{

		public delegate String ContextFormatterDelegate(IEnumerable<String> contextStack, String contextName);
		public delegate String MetricFormatterDelegate(String context, String name, Unit unit, String[] tags);
		public delegate String TagKeyFormatterDelegate(String tagKey);
		public delegate String FieldKeyFormatterDelegate(String fieldKey);


		/// <summary>
		/// Formats the context stack and context name into a custom context name string.
		/// </summary>
		public ContextFormatterDelegate ContextNameFormatter { get; set; }

		/// <summary>
		/// Formats the context name and metric into a string used as the table to insert records into.
		/// </summary>
		public MetricFormatterDelegate MetricNameFormatter { get; set; }

		/// <summary>
		/// Formats a tag key into a string used as the column name in the InfluxDB table.
		/// </summary>
		public TagKeyFormatterDelegate TagKeyFormatter { get; set; }

		/// <summary>
		/// Formats a field key into a string used as the column name in the InfluxDB table.
		/// </summary>
		public FieldKeyFormatterDelegate FieldKeyFormatter { get; set; }

		/// <summary>
		/// If not null, will replace all space characters in context names, metric names,
		/// tag keys, and field keys with the specified string. The default value is an underscore.
		/// </summary>
		public String ReplaceSpaceChar { get; set; }

		/// <summary>
		/// If set to true will convert all context names, metric names, tag keys, and field keys 
		/// to lowercase. If false, it does not modify the names. The default value is true.
		/// </summary>
		public Boolean LowercaseNames { get; set; }


		/// <summary>
		/// Creates a new <see cref="InfluxdbFormatter"/> with default values.
		/// The default formatters convert identifiers to lowercase, replace spaces with underscores, and if applicable, join multiple identifiers with periods.
		/// </summary>
		public InfluxdbFormatter() {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbFormatter"/> with default values, including the <see cref="LowercaseNames"/> and <see cref="ReplaceSpaceChar"/> properties.
		/// The default formatters convert identifiers to lowercase, replace spaces with underscores, and if applicable, join multiple identifiers with periods.
		/// </summary>
		/// <param name="lowercase">If true, converts the string to lowercase.</param>
		/// <param name="replaceChars">The character(s) to replace all space characters with (underscore by default). If null, spaces are not replaced.</param>
		public InfluxdbFormatter(Boolean lowercase, String replaceChars)
			: this() {
			LowercaseNames   = lowercase;
			ReplaceSpaceChar = replaceChars;
		}


		/// <summary>
		/// Formats the context name using the <see cref="ContextNameFormatter"/> if it is set, otherwise returns null.
		/// This will apply lowercasing and replace space characters if it is configured on this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="contextStack">The list of parent context names.</param>
		/// <param name="contextName">The current context name.</param>
		/// <returns>The context name after applying the formatters and transformations, or null if the <see cref="ContextNameFormatter"/> is not set.</returns>
		public virtual String FormatContextName(IEnumerable<String> contextStack, String contextName) {
			String value = ContextNameFormatter?.Invoke(contextStack, contextName);
			if (value == null) return null; // return null so that caller knows that it can call its own default implementation if it has one
			return InfluxUtils.LowerAndReplaceSpaces(value, LowercaseNames, ReplaceSpaceChar);
		}

		/// <summary>
		/// Formats the metric name using the <see cref="MetricNameFormatter"/> if it is set, otherwise returns null.
		/// This will apply lowercasing and replace space characters if it is configured on this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="context">The metrics context name.</param>
		/// <param name="name">The metric name.</param>
		/// <param name="unit">The metric units.</param>
		/// <param name="tags">The metric tags.</param>
		/// <returns>The metric name after applying the formatters and transformations, or null if the <see cref="MetricNameFormatter"/> is not set.</returns>
		public virtual String FormatMetricName(String context, String name, Unit unit, String[] tags) {
			String value = MetricNameFormatter?.Invoke(context, name, unit, tags);
			if (value == null) return null; // return null so that caller knows that it can call its own default implementation if it has one
			return InfluxUtils.LowerAndReplaceSpaces(value, LowercaseNames, ReplaceSpaceChar);
		}

		/// <summary>
		/// Formats the tag key name using the <see cref="TagKeyFormatter"/> if it is set, otherwise uses the unmodified key value.
		/// This will apply lowercasing and replace space characters if it is configured on this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="tagKey">The <see cref="InfluxTag.Key"/> string value to format.</param>
		/// <returns>The tag key name after applying the formatters and transformations.</returns>
		public virtual String FormatTagKey(String tagKey) {
			String value = TagKeyFormatter?.Invoke(tagKey) ?? tagKey;
			return InfluxUtils.LowerAndReplaceSpaces(value, LowercaseNames, ReplaceSpaceChar);
		}

		/// <summary>
		/// Formats the field key name using the <see cref="FieldKeyFormatter"/> if it is set, otherwise uses the unmodified key value.
		/// This will apply lowercasing and replace space characters if it is configured on this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="fieldKey">The <see cref="InfluxField.Key"/> string value to format.</param>
		/// <returns>The field key name after applying the formatters and transformations.</returns>
		public virtual String FormatFieldKey(String fieldKey) {
			String value = FieldKeyFormatter?.Invoke(fieldKey) ?? fieldKey;
			return InfluxUtils.LowerAndReplaceSpaces(value, LowercaseNames, ReplaceSpaceChar);
		}

		/// <summary>
		/// Formats the measurement name, tag keys, and field keys on the specified <see cref="InfluxRecord"/>
		/// with the defined tag and key formatters and returns the same record instance.
		/// </summary>
		/// <param name="record">The <see cref="InfluxRecord"/> to format the tag and field keys for.</param>
		/// <returns>The same <see cref="InfluxRecord"/> instance with the tag and field keys formatted.</returns>
		public virtual InfluxRecord FormatRecord(InfluxRecord record) {
			record.Name = FormatMetricName(null, record.Name, Unit.None, null) ?? record.Name;

			for (int i = 0; i < record.Tags.Count; i++) {
				InfluxTag tag = record.Tags[i];
				String fmtKey = FormatTagKey(tag.Key);
				record.Tags[i] = new InfluxTag(fmtKey, tag.Value);
			}

			for (int i = 0; i < record.Fields.Count; i++) {
				InfluxField field = record.Fields[i];
				String fmtKey = FormatFieldKey(field.Key);
				record.Fields[i] = new InfluxField(fmtKey, field.Value);
			}

			return record;
		}

	}

	/// <summary>
	/// The default formatter used for formatting records. Has some modifications over the default <see cref="Reporters.BaseReport"/>
	/// implementation to generate cleaner output that follows the InfluxDB naming conventions.
	/// </summary>
	public class DefaultFormatter : InfluxdbFormatter
	{
		/// <summary>
		/// Default InfluxDB formatters and settings.
		/// </summary>
		public static class Default
		{
			/// <summary>
			/// The default context name formatter which formats the context stack and context name into a custom context name string.
			/// </summary>
			public static ContextFormatterDelegate ContextNameFormatter { get; }

			/// <summary>
			/// The default metric name formatter which formats the context name and metric into a string used as the table to insert records into.
			/// </summary>
			public static MetricFormatterDelegate MetricNameFormatter { get; }

			/// <summary>
			/// The default tag key formatter which formats a tag key into a string used as the column name in the InfluxDB table.
			/// </summary>
			public static TagKeyFormatterDelegate TagKeyFormatter { get; }

			/// <summary>
			/// The default field key formatter which formats a field key into a string used as the column name in the InfluxDB table.
			/// </summary>
			public static FieldKeyFormatterDelegate FieldKeyFormatter { get; }

			/// <summary>
			/// The default character used to replace space characters in identifier names. This value is an underscore.
			/// </summary>
			public static String ReplaceSpaceChar { get; set; }

			/// <summary>
			/// The default value for whether to convert identifier names to lowercase. This value is true.
			/// </summary>
			public static Boolean LowercaseNames { get; }

			static Default() {
				ContextNameFormatter = (contextStack, contextName) => String.Join(".", contextStack.Concat(new[] { contextName }).Where(c => !String.IsNullOrWhiteSpace(c)));
				MetricNameFormatter  = (context, name, unit, tags) => $"{context}.{name}".Trim(' ', '.');
				TagKeyFormatter      = key => key;
				FieldKeyFormatter    = key => key;
				ReplaceSpaceChar     = "_";
				LowercaseNames       = true;
			}
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbFormatter"/> with default values.
		/// The default formatters convert identifiers to lowercase, replace spaces with underscores, and if applicable, join multiple identifiers with periods.
		/// </summary>
		public DefaultFormatter()
			: base() {
			ContextNameFormatter = Default.ContextNameFormatter;
			MetricNameFormatter  = Default.MetricNameFormatter;
			TagKeyFormatter      = Default.TagKeyFormatter;
			FieldKeyFormatter    = Default.FieldKeyFormatter;
			ReplaceSpaceChar     = Default.ReplaceSpaceChar;
			LowercaseNames       = Default.LowercaseNames;
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbFormatter"/> with default values, including the <see cref="LowercaseNames"/> and <see cref="ReplaceSpaceChar"/> properties.
		/// The default formatters convert identifiers to lowercase, replace spaces with underscores, and if applicable, join multiple identifiers with periods.
		/// </summary>
		/// <param name="lowercase">If true, converts the string to lowercase.</param>
		/// <param name="replaceChars">The character(s) to replace all space characters with (underscore by default). If null, spaces are not replaced.</param>
		public DefaultFormatter(Boolean lowercase, String replaceChars)
			: this() {
			LowercaseNames   = lowercase;
			ReplaceSpaceChar = replaceChars;
		}
	}
}
