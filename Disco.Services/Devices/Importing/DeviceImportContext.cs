using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class DeviceImportContext : IDeviceImportContext
    {
        public string SessionId { get; private set; }
        public string Filename { get; private set; }
        
        public List<Tuple<string, DeviceImportFieldTypes>> Header { get; internal set; }
        public List<Tuple<string, DeviceImportFieldTypes, Func<string[], string>, Type>> ParsedHeaders { get; internal set; }
        internal int HeaderDeviceSerialNumberIndex { get; set; }
        
        public List<string[]> RawData { get; private set; }

        public List<IDeviceImportRecord> Records { get; internal set; }
        public int AffectedRecords { get; internal set; }

        internal DeviceImportContext(string Filename, List<Tuple<string, DeviceImportFieldTypes>> Header, List<string[]> RawData)
        {
            this.SessionId = Guid.NewGuid().ToString("D");

            this.Filename = Filename;
            this.Header = Header;
            this.RawData = RawData;
        }
    }
}