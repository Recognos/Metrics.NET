using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics.Influxdb;
using Xunit;
using FluentAssertions;
using Metrics.Utils;
using System.Threading;
using Metrics.Influxdb.Model;

namespace Metrics.Tests.Influxdb
{
	public class InfluxdbTests
	{


		[Fact]
		public void InfluxTag_CanParse_InvalidValueReturnsEmpty() {
			// invalid input strings
			InfluxTag empty = InfluxTag.Empty;
			String nullReason = "Because the input string should contain a single key and value separated by an equals sign.";
			InfluxUtils.ToInfluxTag("key").Should().Be(empty, nullReason);
			InfluxUtils.ToInfluxTag("key=").Should().Be(empty, nullReason);
			InfluxUtils.ToInfluxTag("=value").Should().Be(empty, nullReason);
			InfluxUtils.ToInfluxTag("key=value1=value2").Should().Be(empty, nullReason);
			InfluxUtils.ToInfluxTag("key,value").Should().Be(empty, nullReason);
			InfluxUtils.ToInfluxTag("key==").Should().Be(empty, nullReason);
			InfluxUtils.ToInfluxTag("==val").Should().Be(empty, nullReason);
		}

		[Fact]
		public void InfluxTag_CanParse_SingleFromString() {
			// valid input strings
			InfluxUtils.ToInfluxTag("key=value").Should().Be(new InfluxTag("key", "value"));
			InfluxUtils.ToInfluxTag("key with spaces=value with spaces").Should().Be(new InfluxTag("key with spaces", "value with spaces"));
			InfluxUtils.ToInfluxTag("key,with,commas=value,with,commas").Should().Be(new InfluxTag("key,with,commas", "value,with,commas"));
			InfluxUtils.ToInfluxTag("key\"with\"quot=value\"with\"quot").Should().Be(new InfluxTag("key\"with\"quot", "value\"with\"quot"));
		}

		[Fact]
		public void InfluxTag_CanParse_MultiFromCommaSeparatedString() {
			// comma-separated single string
			InfluxUtils.ToInfluxTags("key").Should().BeEmpty();
			InfluxUtils.ToInfluxTags("key1=value1,key2=value2").Should().BeEquivalentTo(new InfluxTag("key1", "value1"), new InfluxTag("key2", "value2"));
			InfluxUtils.ToInfluxTags("key1,key2=value2,key3,key4").Should().BeEquivalentTo(new InfluxTag("key2", "value2"));
		}

		[Fact]
		public void InfluxTag_CanParse_MultiFromStringArray() {
			// string[] array
			InfluxUtils.ToInfluxTags("key1", "key2").Should().BeEmpty();
			InfluxUtils.ToInfluxTags("key1=value1", "key2=value2").Should().BeEquivalentTo(new InfluxTag("key1", "value1"), new InfluxTag("key2", "value2"));
			InfluxUtils.ToInfluxTags("key1", "key2=value2", "key3", "key4").Should().BeEquivalentTo(new InfluxTag("key2", "value2"));
		}

		[Fact]
		public void InfluxTag_CanParse_FromSingleMetricTags() {
			InfluxUtils.ToInfluxTags(new MetricTags("key1", "key2")).Should().BeEmpty();
			InfluxUtils.ToInfluxTags(new MetricTags("key1=value1", "key2")).Should().BeEquivalentTo(new InfluxTag("key1", "value1"));
			InfluxUtils.ToInfluxTags(new MetricTags("key1", "key2=value2")).Should().BeEquivalentTo(new InfluxTag("key2", "value2"));
			InfluxUtils.ToInfluxTags(new MetricTags("key1=value1", "key2=value2")).Should().BeEquivalentTo(new InfluxTag("key1", "value1"), new InfluxTag("key2", "value2"));
		}

		[Fact]
		public void InfluxTag_CanParse_FromMultiMetricTags() {
			InfluxUtils.ToInfluxTags(new MetricTags("key1", "key2"), new MetricTags("key3", "key4")).Should().BeEmpty();
			InfluxUtils.ToInfluxTags(new MetricTags("key1=value1", "key2"), new MetricTags("key3=value3", "key4")).Should().BeEquivalentTo(new InfluxTag("key1", "value1"), new InfluxTag("key3", "value3"));
			InfluxUtils.ToInfluxTags(new MetricTags("key1", "key2=value2"), new MetricTags("key3", "key4=value4")).Should().BeEquivalentTo(new InfluxTag("key2", "value2"), new InfluxTag("key4", "value4"));
			InfluxUtils.ToInfluxTags(new MetricTags("key1=value1", "key2=value2"), new MetricTags("key3=value3", "key4=value4")).Should().BeEquivalentTo(new InfluxTag("key1", "value1"), new InfluxTag("key2", "value2"), new InfluxTag("key3", "value3"), new InfluxTag("key4", "value4"));
		}


		[Fact]
		public void InfluxField_SupportsValidValueTypes() {
			var validTypes = InfluxUtils.ValidValueTypes;
			foreach (var type in validTypes)
				InfluxUtils.IsValidValueType(type).Should().BeTrue();
		}

		[Theory]
		[MemberData(nameof(TagTestCasesArray))]
		public void InfluxTag_FormatsTo_LineProtocol(InfluxTag tag, String output) {
			tag.ToLineProtocol().Should().Be(output);
			tag.ToString().Should().Be(output);
		}

		[Theory]
		[MemberData(nameof(FieldTestCasesArray))]
		public void InfluxField_FormatsTo_LineProtocol(InfluxField field, String output) {
			field.ToLineProtocol().Should().Be(output);
			field.ToString().Should().Be(output);
		}

		[Fact]
		public void InfluxRecord_FormatsTo_LineProtocol() {
			// test values
			var testNow = new DateTime(2016, 6, 1, 0, 0, 0, DateTimeKind.Utc);
			var testTags = TagTestCases.Select(tc => tc.Tag);
			var testFields = FieldTestCases.Select(tc => tc.Field);

			// expected values
			String expTime = InfluxLineProtocol.FormatTimestamp(testNow, InfluxPrecision.Seconds);
			String expTags = String.Join(",", TagTestCases.Select(tc => tc.Output));
			String expFields = String.Join(",", FieldTestCases.Select(tc => tc.Output));
			String expOutput = String.Format("test_name,{0} {1} {2}", expTags, expFields, expTime);

			// assert line values match expected
			new InfluxRecord("name spaces", new[] { new InfluxField("field1", 123456) })
				.ToLineProtocol().Should().Be(@"name\ spaces field1=123456i");
			new InfluxRecord("test_name", new[] { new InfluxTag("tag1", "value1") }, new[] { new InfluxField("field1", 123456) })
				.ToLineProtocol().Should().Be(@"test_name,tag1=value1 field1=123456i");
			new InfluxRecord("test_name", new[] { new InfluxTag("tag1", "value1"), new InfluxTag("tag2", "value2") }, new[] { new InfluxField("field1", 123456), new InfluxField("field2", true) })
				.ToLineProtocol().Should().Be(@"test_name,tag1=value1,tag2=value2 field1=123456i,field2=True");
			new InfluxRecord("test_name", new[] { new InfluxTag("tag1", "value1") }, new[] { new InfluxField("field1", "test string") }, testNow)
				.ToLineProtocol().Should().Be($@"test_name,tag1=value1 field1=""test string"" {expTime}");
			new InfluxRecord("test_name", testTags, testFields, testNow)
				.ToLineProtocol().Should().Be(expOutput);
		}

		[Fact]
		public void InfluxBatch_FormatsTo_LineProtocol() {
			var testNow = new DateTime(2016, 6, 1, 0, 0, 0, DateTimeKind.Utc);
			var testTags = TagTestCases.Select(tc => tc.Tag);
			var testFields = FieldTestCases.Select(tc => tc.Field);
			var expTime = InfluxLineProtocol.FormatTimestamp(testNow, InfluxPrecision.Seconds);

			// test with empty batch
			InfluxBatch batch = new InfluxBatch();
			batch.ToLineProtocol().Should().BeEmpty();

			// test with single record
			batch.Add(new InfluxRecord("test_name", new[] { new InfluxTag("tag1", "value1") }, new[] { new InfluxField("field1", 123456) }));
			batch.ToLineProtocol().Should().NotEndWith("\n").And.Be(@"test_name,tag1=value1 field1=123456i");
			batch.Clear();

			// test with multiple records
			batch.Add(new InfluxRecord("test_name1", new[] { new InfluxTag("tag1", "value1") }, new[] { new InfluxField("field1", 123456) }));
			batch.Add(new InfluxRecord("test_name2", new[] { new InfluxTag("tag2", "value2") }, new[] { new InfluxField("field2", 234561) }));
			batch.Add(new InfluxRecord("test_name3", new[] { new InfluxTag("tag3", "value3") }, new[] { new InfluxField("field3", 345612) }, testNow));
			batch.Add(new InfluxRecord("test_name4", new[] { new InfluxTag("tag4", "value4") }, new[] { new InfluxField("field4", 456123) }, testNow));
			batch.Add(new InfluxRecord("test_name5", new[] { new InfluxTag("tag5", "value5") }, new[] { new InfluxField("field5", 561234) }, testNow));

			String expOutput = String.Join("\n",
				$@"test_name1,tag1=value1 field1=123456i",
				$@"test_name2,tag2=value2 field2=234561i",
				$@"test_name3,tag3=value3 field3=345612i {expTime}",
				$@"test_name4,tag4=value4 field4=456123i {expTime}",
				$@"test_name5,tag5=value5 field5=561234i {expTime}"
			);

			batch.ToLineProtocol().Should().NotEndWith("\n").And.Be(expOutput);
		}

		[Fact]
		public void InfluxReport_CanAddRecords_ForGauge() {
			var writer = new InfluxdbTestWriter();
			var config = new InfluxConfig("localhost", "testdb") { Writer = writer };
			var report = new InfluxdbHttpReport(config);
			var context = new DefaultMetricsContext("TestContext");
			var precision = config.Precision ?? InfluxConfig.Default.Precision;
			var metricsData = context.DataProvider.CurrentMetricsData;

			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().BeEmpty("Because running a report with no metrics should not result in any records.");

			context.Gauge("test_gauge", () => 123.456, Unit.Bytes, new MetricTags("key1=value1,tag2,tag3,key4=value4"));
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			var expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().Be($@"testcontext.test_gauge,key1=value1,key4=value4 value=123.456 {expTime}");
		}

		[Fact]
		public void InfluxReport_CanAddRecords_ForCounter() {
			var writer = new InfluxdbTestWriter();
			var config = new InfluxConfig("localhost", "testdb") { Writer = writer };
			var report = new InfluxdbHttpReport(config);
			var context = new DefaultMetricsContext("TestContext");
			var precision = config.Precision ?? InfluxConfig.Default.Precision;
			var metricsData = context.DataProvider.CurrentMetricsData;
			var counter = context.Counter("test_counter", Unit.Bytes, new MetricTags("key1=value1,tag2,tag3,key4=value4"));

			// add normally
			counter.Increment(300);
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			var expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().Be($@"testcontext.test_counter,key1=value1,key4=value4 count=300i {expTime}");

			// add with set item
			counter.Increment("item1,item2=ival2,item3=ival3", 100);
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(2);

			expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().Be($@"testcontext.test_counter,key1=value1,key4=value4 count=400i {expTime}");
			writer.LastBatch[1].ToLineProtocol().Should().Be($@"testcontext.test_counter,item2=ival2,item3=ival3,key1=value1,key4=value4 count=100i,percent=25 {expTime}");
		}

		[Fact]
		public void InfluxReport_CanAddRecords_ForMeter() {
			var writer = new InfluxdbTestWriter();
			var config = new InfluxConfig("localhost", "testdb") { Writer = writer };
			var report = new InfluxdbHttpReport(config);
			var context = new DefaultMetricsContext("TestContext");
			var precision = config.Precision ?? InfluxConfig.Default.Precision;
			var metricsData = context.DataProvider.CurrentMetricsData;
			var meter = context.Meter("test_meter", Unit.Bytes, TimeUnit.Seconds, new MetricTags("key1=value1,tag2,tag3,key4=value4"));

			// add normally
			meter.Mark(300);
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			var expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().StartWith($@"testcontext.test_meter,key1=value1,key4=value4 count=300i,mean_rate=").And.EndWith($@",1_min_rate=0,5_min_rate=0,15_min_rate=0 {expTime}"); ;

			// add with set item
			meter.Mark("item1,item2=ival2,item3=ival3", 100);
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(2);

			expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().StartWith($@"testcontext.test_meter,key1=value1,key4=value4 count=400i,mean_rate=").And.EndWith($@",1_min_rate=0,5_min_rate=0,15_min_rate=0 {expTime}");
			writer.LastBatch[1].ToLineProtocol().Should().StartWith($@"testcontext.test_meter,item2=ival2,item3=ival3,key1=value1,key4=value4 count=100i,percent=25,mean_rate=").And.EndWith($@",1_min_rate=0,5_min_rate=0,15_min_rate=0 {expTime}");
		}

		[Fact]
		public void InfluxReport_CanAddRecords_ForHistogram() {
			var writer = new InfluxdbTestWriter();
			var config = new InfluxConfig("localhost", "testdb") { Writer = writer };
			var report = new InfluxdbHttpReport(config);
			var context = new DefaultMetricsContext("TestContext");
			var precision = config.Precision ?? InfluxConfig.Default.Precision;
			var metricsData = context.DataProvider.CurrentMetricsData;
			var hist = context.Histogram("test_hist", Unit.Bytes, SamplingType.Default, new MetricTags("key1=value1,tag2,tag3,key4=value4"));

			// add normally
			hist.Update(300);
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			var expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().Be($@"testcontext.test_hist,key1=value1,key4=value4 count=1i,last=300,min=300,mean=300,max=300,stddev=0,median=300,sample_size=1i,percentile_75%=300,percentile_95%=300,percentile_98%=300,percentile_99%=300,percentile_99.9%=300 {expTime}");

			// add with set item
			hist.Update(100, "item1,item2=ival2,item3=ival3");
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().Be($@"testcontext.test_hist,key1=value1,key4=value4 count=2i,last=100,min=100,mean=200,max=300,stddev=100,median=300,sample_size=2i,percentile_75%=300,percentile_95%=300,percentile_98%=300,percentile_99%=300,percentile_99.9%=300 {expTime}");
		}

		[Fact]
		public void InfluxReport_CanAddRecords_ForTimer() {
			var writer = new InfluxdbTestWriter();
			var config = new InfluxConfig("localhost", "testdb") { Writer = writer };
			var report = new InfluxdbHttpReport(config);
			var context = new DefaultMetricsContext("TestContext");
			var precision = config.Precision ?? InfluxConfig.Default.Precision;
			var metricsData = context.DataProvider.CurrentMetricsData;
			var timer = context.Timer("test_timer", Unit.Bytes, SamplingType.Default, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags("key1=value1,tag2,tag3,key4=value4"));

			// add normally
			timer.Record(100, TimeUnit.Seconds);
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			var expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().StartWith($@"testcontext.test_timer,key1=value1,key4=value4 active_sessions=0i,total_time=100i,count=1i,").And.EndWith($@",1_min_rate=0,5_min_rate=0,15_min_rate=0,last=100,min=100,mean=100,max=100,stddev=0,median=100,sample_size=1i,percentile_75%=100,percentile_95%=100,percentile_98%=100,percentile_99%=100,percentile_99.9%=100 {expTime}");

			// add with set item
			timer.Record(50, TimeUnit.Seconds, "item1,item2=ival2,item3=ival3");
			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, hsFunc, CancellationToken.None);
			writer.LastBatch.Should().HaveCount(1);

			expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().StartWith($@"testcontext.test_timer,key1=value1,key4=value4 active_sessions=0i,total_time=150i,count=2i,").And.EndWith($@",1_min_rate=0,5_min_rate=0,15_min_rate=0,last=50,min=50,mean=75,max=100,stddev=25,median=100,sample_size=2i,percentile_75%=100,percentile_95%=100,percentile_98%=100,percentile_99%=100,percentile_99.9%=100 {expTime}");
		}

		[Fact]
		public void InfluxReport_CanAddRecords_ForHealthCheck() {
			var writer = new InfluxdbTestWriter();
			var config = new InfluxConfig("localhost", "testdb") { Writer = writer };
			var report = new InfluxdbHttpReport(config);
			var context = new DefaultMetricsContext("TestContext");
			var precision = config.Precision ?? InfluxConfig.Default.Precision;
			var metricsData = context.DataProvider.CurrentMetricsData;

			HealthChecks.UnregisterAllHealthChecks();
			HealthChecks.RegisterHealthCheck("Health Check 1", () => HealthCheckResult.Healthy($"Healthy check!"));
			HealthChecks.RegisterHealthCheck("Health Check 2", () => HealthCheckResult.Unhealthy($"Unhealthy check!"));
			HealthChecks.RegisterHealthCheck("Health Check 3,tag3=key3",      () => HealthCheckResult.Healthy($"Healthy check!"));
			HealthChecks.RegisterHealthCheck("Health Check 4,tag 4=key 4",    () => HealthCheckResult.Healthy($"Healthy check!"));
			HealthChecks.RegisterHealthCheck("Name=Health Check 5,tag5=key5", () => HealthCheckResult.Healthy($"Healthy check!"));

			metricsData = context.DataProvider.CurrentMetricsData;
			report.RunReport(metricsData, () => HealthChecks.GetStatus(), CancellationToken.None);
			HealthChecks.UnregisterAllHealthChecks(); // unreg first in case something below throws
			writer.LastBatch.Should().HaveCount(5);

			var expTime = InfluxLineProtocol.FormatTimestamp(metricsData.Timestamp, precision);
			writer.LastBatch[0].ToLineProtocol().Should().Be($@"health_checks,name=health_check_1 ishealthy=True,message=""Healthy check!"" {expTime}");
			writer.LastBatch[1].ToLineProtocol().Should().Be($@"health_checks,name=health_check_2 ishealthy=False,message=""Unhealthy check!"" {expTime}");
			writer.LastBatch[2].ToLineProtocol().Should().Be($@"health_checks,name=health_check_3,tag3=key3 ishealthy=True,message=""Healthy check!"" {expTime}");
			writer.LastBatch[3].ToLineProtocol().Should().Be($@"health_checks,name=health_check_4,tag_4=key\ 4 ishealthy=True,message=""Healthy check!"" {expTime}");
			writer.LastBatch[4].ToLineProtocol().Should().Be($@"health_checks,name=health\ check\ 5,tag5=key5 ishealthy=True,message=""Healthy check!"" {expTime}");
		}



		#region Tag and Field Test Cases and Other Static Members

		public static IEnumerable<TagTestCase> TagTestCases = new[] {
			new TagTestCase("key1", "value1", @"key1=value1"),
			new TagTestCase("key2 with spaces", "value2 with spaces", @"key2\ with\ spaces=value2\ with\ spaces"),
			new TagTestCase("key3,with,commas", "value3,with,commas", @"key3\,with\,commas=value3\,with\,commas"),
			new TagTestCase("key4=with=equals", "value4=with=equals", @"key4\=with\=equals=value4\=with\=equals"),
			new TagTestCase("key5\"with\"quot", "value5\"with\"quot", "key5\"with\"quot=value5\"with\"quot"),
			new TagTestCase("key6\" with,all=", "value6\" with,all=", @"key6""\ with\,all\==value6""\ with\,all\="),
		};

		public static IEnumerable<FieldTestCase> FieldTestCases = new[] {
			new FieldTestCase("field1_int1",  100, @"field1_int1=100i"),
			new FieldTestCase("field1_int2", -100, @"field1_int2=-100i"),
			new FieldTestCase("field2_double1",  123456789.123456, @"field2_double1=123456789.123456"),
			new FieldTestCase("field2_double2", -123456789.123456, @"field2_double2=-123456789.123456"),
			new FieldTestCase("field2_double3", Math.PI, @"field2_double3=3.1415926535897931"),
			new FieldTestCase("field2_double4", Double.MinValue, @"field2_double4=-1.7976931348623157E+308"),
			new FieldTestCase("field2_double5", Double.MaxValue, @"field2_double5=1.7976931348623157E+308"),
			new FieldTestCase("field3_bool1", true,  @"field3_bool1=True"),
			new FieldTestCase("field3_bool2", false, @"field3_bool2=False"),
			new FieldTestCase("field4_string1", "string value1",  @"field4_string1=""string value1"""),
			new FieldTestCase("field4_string2", "string\"value2", @"field4_string2=""string\""value2"""),
			new FieldTestCase("field5 spaces", 100, @"field5\ spaces=100i"),
			new FieldTestCase("field6,commas", 100, @"field6\,commas=100i"),
			new FieldTestCase("field7=equals", 100, @"field7\=equals=100i"),
			new FieldTestCase("field8\"quote", 100, @"field8""quote=100i"),
		};

		// these must be defined after the above are defined so the variables are not null
		public static IEnumerable<Object[]> TagTestCasesArray = TagTestCases.Select(t => t.ToArray());
		public static IEnumerable<Object[]> FieldTestCasesArray = FieldTestCases.Select(t => t.ToArray());


		private static readonly Func<HealthStatus> hsFunc = () => new HealthStatus();

		#endregion

	}
}
