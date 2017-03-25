using Disco.Models.Services.Devices.Importing;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class CsvDeviceImportContext : BaseDeviceImportContext
    {
        private List<string[]> rawData;
        private bool hasHeaderRow;

        public CsvDeviceImportContext(string Filename, bool HasHeader, Stream CsvStream)
            : base(Filename)
        {
            hasHeaderRow = HasHeader;

            ParseCsv(CsvStream);
        }

        public override int RecordCount
        {
            get
            {
                return rawData.Count;
            }
        }

        public override IDeviceImportDataReader GetDataReader()
        {
            return new CsvDeviceImportDataReader(this, rawData, hasHeaderRow);
        }

        private void ParseCsv(Stream CsvStream)
        {
            using (TextReader csvTextReader = new StreamReader(CsvStream))
            {
                using (CsvReader csvReader = new CsvReader(csvTextReader, hasHeaderRow))
                {
                    csvReader.DefaultParseErrorAction = ParseErrorAction.ThrowException;
                    csvReader.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    rawData = csvReader.ToList();
                    SetColumns(csvReader.GetFieldHeaders().Select((h, i) => new DeviceImportColumn()
                    {
                        Index = i,
                        Name = h,
                        Type = DeviceImportFieldTypes.IgnoreColumn
                    }));
                }
            }
        }
    }
}