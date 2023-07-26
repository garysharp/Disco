using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using Microsoft.Win32;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace Disco.Data.Repository
{
    public class DiscoDatabaseConnectionFactory : IDbConnectionFactory
    {
        private const string DiscoRegistryKey = @"SOFTWARE\Disco";

        private IDbConnectionFactory DefaultConnectionFactory;
        private IDbConnectionFactory SqlCeConnectionFactory;
        private static string _DiscoDataContextConnectionString;

        public static string DiscoDataContextConnectionString
        {
            get
            {
                if (_DiscoDataContextConnectionString == null)
                {
                    // Retrieve from Registry
                    using (var key = Registry.LocalMachine.OpenSubKey(DiscoRegistryKey))
                    {
                        if (key != null)
                            _DiscoDataContextConnectionString = (string)key.GetValue("DatabaseConnectionString", null);
                    }
                }
                return _DiscoDataContextConnectionString;
            }
        }

        public static void SetDiscoDataContextConnectionString(string ConnectionString, bool Persist)
        {
            // Set to Local Cache
            _DiscoDataContextConnectionString = ConnectionString;

            if (Persist)
            {
                // Set to Registry
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(DiscoRegistryKey))
                    {
                        key.SetValue("DatabaseConnectionString", ConnectionString, RegistryValueKind.String);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new SecurityException(string.Format("Unable to write to the Registry Location: HKML\\{0}[DatabaseConnectionString]", DiscoRegistryKey), ex);
                }
            }
        }

        public DiscoDatabaseConnectionFactory(IDbConnectionFactory Default)
        {
            this.DefaultConnectionFactory = Default;
            this.SqlCeConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
        }

        public System.Data.Common.DbConnection CreateConnection(string nameOrConnectionString)
        {
            if (nameOrConnectionString == "Disco.Data.Repository.DiscoDataContext")
            {

                var connectionString = DiscoDataContextConnectionString;
                if (connectionString == null)
                {
                    throw new InvalidOperationException("The Disco ICT DataContext Connection String has not been configured");
                }

                // Build DiscoDataContext - Use Default Connection Factory (SQLClient)

                //return this.DefaultConnectionFactory.CreateConnection(connectionString);
                var connection = DbProviderFactories.GetFactory("System.Data.SqlClient").CreateConnection();
                connection.ConnectionString = connectionString;
                return connection;
            }

            return SqlCeConnectionFactory.CreateConnection(nameOrConnectionString);
        }
    }
}
