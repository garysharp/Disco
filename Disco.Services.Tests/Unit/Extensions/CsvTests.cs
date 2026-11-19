using Disco.Services.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Disco.Services.Tests.Unit.Extensions
{
    public class CsvTests
    {
        [Fact]
        public void Parse_WithAllSupportedTypes_MapsAllDataTypes()
        {
            var csv = "Text,Integer,Decimal,Boolean,Date Time,Long Integer\r\n" +
                      "Sample,42,98.765,true,2026-11-01 13:14:15.1234567,9876543210123\r\n";

            AllTypesRecord record;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                record = Csv.Parse<AllTypesRecord>(stream).Single();
            }

            Assert.Equal("Sample", record.Text);
            Assert.Equal(42, record.Integer);
            Assert.Equal(98.765, record.Decimal, 3);
            Assert.True(record.Boolean);

            // The DateTime was converted from UTC to local time,
            //  so convert it back to UTC for comparison
            var utcDateTime = record.DateTime.ToUniversalTime();
            Assert.Equal(2026, utcDateTime.Year);
            Assert.Equal(11, utcDateTime.Month);
            Assert.Equal(1, utcDateTime.Day);
            Assert.Equal(13, utcDateTime.Hour);
            Assert.Equal(14, utcDateTime.Minute);
            Assert.Equal(15, utcDateTime.Second);
            Assert.Equal(123, utcDateTime.Millisecond);
            Assert.Equal(4567, utcDateTime.Ticks % TimeSpan.TicksPerMillisecond);

            Assert.Equal(9876543210123L, record.LongInteger);
        }

        [Fact]
        public void Parse_WithMultipleRecords_MapsAllRowsAndRowNumbers()
        {
            var csv = "Text,Integer,Decimal,Boolean,Date Time,Long Integer\r\n" +
                      "First,1,1.5,true,2026-01-01 00:00:00.0000000,100\r\n" +
                      "Second,2,2.5,false,2026-01-02 00:00:00.0000000,200\r\n";

            AllTypesRecord[] records;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                records = Csv.Parse<AllTypesRecord>(stream).ToArray();
            }

            Assert.Equal(2, records.Length);

            Assert.Equal(1, records[0].RowNumber);
            Assert.Equal("First", records[0].Text);
            Assert.Equal(1, records[0].Integer);
            Assert.Equal(100L, records[0].LongInteger);

            Assert.Equal(2, records[1].RowNumber);
            Assert.Equal("Second", records[1].Text);
            Assert.Equal(2, records[1].Integer);
            Assert.Equal(200L, records[1].LongInteger);
        }

        [Fact]
        public void Parse_WithEmptyStringValues_MapsStringsToNull()
        {
            var csv = "Text,Optional Note\r\n" +
                      ",\"\"\r\n";

            StringRecord record;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                record = Csv.Parse<StringRecord>(stream).Single();
            }

            Assert.Null(record.Text);
            Assert.Null(record.OptionalNote);
        }

        [Fact]
        public void Parse_WithEscapedQuotedValues_ParsesTextCorrectly()
        {
            var csv = "Text,Optional Note\r\n" +
                      "\"Comma, and \"\"quoted\"\" text\",\"Single quote ' and \"\"double quote\"\"\"\r\n";

            StringRecord record;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                record = Csv.Parse<StringRecord>(stream).Single();
            }

            Assert.Equal("Comma, and \"quoted\" text", record.Text);
            Assert.Equal("Single quote ' and \"double quote\"", record.OptionalNote);
        }

        [Fact]
        public void Parse_WithAllValuesQuoted_ParsesSupportedTypes()
        {
            var csv = "\"Text\",\"Integer\",\"Decimal\",\"Boolean\",\"Date Time\",\"Long Integer\"\r\n" +
                      "\"Quoted\",\"42\",\"98.765\",\"true\",\"2026-11-01 13:14:15.1234567\",\"9876543210123\"\r\n";

            AllTypesRecord record;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                record = Csv.Parse<AllTypesRecord>(stream).Single();
            }

            Assert.Equal("Quoted", record.Text);
            Assert.Equal(42, record.Integer);
            Assert.Equal(98.765, record.Decimal, 3);
            Assert.True(record.Boolean);
            Assert.Equal(9876543210123L, record.LongInteger);
        }

        [Fact]
        public void Parse_WithEmptyValueForStructType_ThrowsFormatException()
        {
            var csv = "Text,Integer\r\n" +
                      "Value,\r\n";

            var exception = Assert.Throws<FormatException>(() =>
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
                {
                    Csv.Parse<NonNullableIntRecord>(stream).Single();
                }
            });

            Assert.Contains("cannot be empty", exception.Message);
            Assert.Contains("Integer", exception.Message);
        }

        [Fact]
        public void Parse_WithEmptyValueForNullableStruct_MapsNull()
        {
            var csv = "Text,Nullable Integer\r\n" +
                      "Value,\r\n";

            NullableIntRecord record;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                record = Csv.Parse<NullableIntRecord>(stream).Single();
            }

            Assert.Equal("Value", record.Text);
            Assert.Null(record.NullableInteger);
        }

        private class AllTypesRecord : Csv.CsvRecord
        {
            [Display(Name = "Text")]
            public string Text { get; set; }

            [Display(Name = "Integer")]
            public int Integer { get; set; }

            [Display(Name = "Decimal")]
            public double Decimal { get; set; }

            [Display(Name = "Boolean")]
            public bool Boolean { get; set; }

            [Display(Name = "Date Time")]
            public DateTime DateTime { get; set; }

            [Display(Name = "Long Integer")]
            public long LongInteger { get; set; }
        }

        private class StringRecord : Csv.CsvRecord
        {
            [Display(Name = "Text")]
            public string Text { get; set; }

            [Display(Name = "Optional Note")]
            public string OptionalNote { get; set; }
        }

        private class NonNullableIntRecord : Csv.CsvRecord
        {
            [Display(Name = "Text")]
            public string Text { get; set; }

            [Display(Name = "Integer")]
            public int Integer { get; set; }
        }

        private class NullableIntRecord : Csv.CsvRecord
        {
            [Display(Name = "Text")]
            public string Text { get; set; }

            [Display(Name = "Nullable Integer")]
            public int? NullableInteger { get; set; }
        }
    }
}
