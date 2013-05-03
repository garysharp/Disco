using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using Disco.Data.Configuration;

namespace Disco.BI
{
    public static class DataStore
    {

        public static string CreateLocation(DiscoDataContext dbContext, string SubLocation, DateTime? SubSubLocationTimestamp = null)
        {
            return CreateLocation(dbContext.DiscoConfiguration, SubLocation, SubSubLocationTimestamp);
        }
        public static string CreateLocation(SystemConfiguration DiscoConfiguration, string SubLocation, DateTime? SubSubLocationTimestamp = null)
        {
            string SubSubLocation = string.Empty;
            if (SubSubLocationTimestamp.HasValue)
                SubSubLocation = SubSubLocationTimestamp.Value.ToString(@"yyyy\\MM");

            string storeDirectory = System.IO.Path.Combine(DiscoConfiguration.DataStoreLocation, SubLocation, SubSubLocation);
            if (!System.IO.Directory.Exists(storeDirectory))
                System.IO.Directory.CreateDirectory(storeDirectory);

            return storeDirectory;
        }

    }
}
