using System;
using System.Collections;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace Disco.Services.Expressions.Extensions
{
    public static class DataExt
    {
        #region SqlClient

        private static SqlConnection BuildSqlConnection(string Server, string Database, string Username, string Password)
        {
            var dbConnectionStringBuilder = new SqlConnectionStringBuilder();
            dbConnectionStringBuilder.ApplicationName = "Disco";
            dbConnectionStringBuilder.DataSource = Server;
            dbConnectionStringBuilder.InitialCatalog = Database;
            dbConnectionStringBuilder.MultipleActiveResultSets = true;
            dbConnectionStringBuilder.PersistSecurityInfo = true;
            if (Username == null || Password == null)
                dbConnectionStringBuilder.IntegratedSecurity = true;
            else
            {
                dbConnectionStringBuilder.UserID = Username;
                dbConnectionStringBuilder.Password = Password;
            }

            return new SqlConnection(dbConnectionStringBuilder.ConnectionString);
        }
        private static void BuildSqlParameters(SqlCommand dbCommand, Hashtable SqlParameters)
        {
            if (SqlParameters != null)
            {
                foreach (var sqlParameterKey in SqlParameters.Keys)
                {
                    string key = sqlParameterKey.ToString();
                    if (!key.StartsWith("@"))
                        key = string.Concat("@", key);
                    dbCommand.Parameters.AddWithValue(key, SqlParameters[sqlParameterKey]);
                }
            }
        }

        public static DataTable QuerySqlDatabase(string Server, string Database, string Username, string Password, string SqlQuery, Hashtable SqlParameters)
        {
            using (SqlConnection dbConnection = BuildSqlConnection(Server, Database, Username, Password))
            {
                using (SqlCommand dbCommand = new SqlCommand(SqlQuery, dbConnection))
                {
                    BuildSqlParameters(dbCommand, SqlParameters);
                    using (SqlDataAdapter dbAdapter = new SqlDataAdapter(dbCommand))
                    {
                        var dbTable = new DataTable();
                        dbAdapter.Fill(dbTable);
                        return dbTable;
                    }
                }
            }
        }
        public static DataTable QuerySqlDatabase(string Server, string Database, string SqlQuery, Hashtable SqlParameters)
        {
            return QuerySqlDatabase(Server, Database, null, null, SqlQuery, SqlParameters);
        }
        public static DataTable QuerySqlDatabase(string Server, string Database, string SqlQuery)
        {
            return QuerySqlDatabase(Server, Database, null, null, SqlQuery, null);
        }

        public static object QuerySqlDatabaseScalar(string Server, string Database, string Username, string Password, string SqlQuery, Hashtable SqlParameters)
        {
            using (SqlConnection dbConnection = BuildSqlConnection(Server, Database, Username, Password))
            {
                using (SqlCommand dbCommand = new SqlCommand(SqlQuery, dbConnection))
                {
                    BuildSqlParameters(dbCommand, SqlParameters);
                    try
                    {
                        dbConnection.Open();
                        return dbCommand.ExecuteScalar();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        dbConnection.Close();
                    }
                }
            }
        }
        public static object QuerySqlDatabaseScalar(string Server, string Database, string SqlQuery, Hashtable SqlParameters)
        {
            return QuerySqlDatabaseScalar(Server, Database, null, null, SqlQuery, SqlParameters);
        }
        public static object QuerySqlDatabaseScalar(string Server, string Database, string SqlQuery)
        {
            return QuerySqlDatabaseScalar(Server, Database, null, null, SqlQuery, null);
        }

        #endregion

        #region ODBC

        private static OdbcConnection BuildOdbcConnection(string ConnectionString)
        {
            return new OdbcConnection(ConnectionString);
        }
        private static void BuildOdbcParameters(OdbcCommand dbCommand, Hashtable OdbcParameters)
        {
            if (OdbcParameters != null)
            {
                foreach (var odbcParameterKey in OdbcParameters.Keys)
                {
                    string key = odbcParameterKey.ToString();
                    dbCommand.Parameters.AddWithValue(key, OdbcParameters[odbcParameterKey]);
                }
            }
        }

        public static DataTable QueryOdbcDatabase(string ConnectionString, string OdbcQuery, Hashtable OdbcParameters)
        {
            using (OdbcConnection dbConnection = BuildOdbcConnection(ConnectionString))
            {
                using (OdbcCommand dbCommand = new OdbcCommand(OdbcQuery, dbConnection))
                {
                    BuildOdbcParameters(dbCommand, OdbcParameters);
                    using (OdbcDataAdapter dbAdapter = new OdbcDataAdapter(dbCommand))
                    {
                        var dbTable = new DataTable();
                        dbAdapter.Fill(dbTable);
                        return dbTable;
                    }
                }
            }
        }
        public static DataTable QueryOdbcDatabase(string ConnectionString, string OdbcQuery)
        {
            return QueryOdbcDatabase(ConnectionString, OdbcQuery, null);
        }

        public static object QueryOdbcDatabaseScalar(string ConnectionString, string OdbcQuery, Hashtable OdbcParameters)
        {
            using (OdbcConnection dbConnection = BuildOdbcConnection(ConnectionString))
            {
                using (OdbcCommand dbCommand = new OdbcCommand(OdbcQuery, dbConnection))
                {
                    BuildOdbcParameters(dbCommand, OdbcParameters);
                    try
                    {
                        dbConnection.Open();
                        return dbCommand.ExecuteScalar();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        dbConnection.Close();
                    }
                }
            }
        }
        public static object QueryOdbcDatabaseScalar(string ConnectionString, string OdbcQuery)
        {
            return QueryOdbcDatabaseScalar(ConnectionString, OdbcQuery, null);
        }

        #endregion
    }
}
