using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Disco.Web.Models.InitialConfig
{
    public class DatabaseModel
    {
        public DatabaseModel()
        {
            // Set Defaults
            Server = "(local)";
            DatabaseName = "Disco";
            AuthMethod = "SSPI";
        }

        public static DatabaseModel FromConnectionString(string ConnectionString)
        {
            var result = new DatabaseModel();

            try
            {
                var csb = new SqlConnectionStringBuilder(ConnectionString);

                if (!string.IsNullOrEmpty(csb.DataSource))
                    result.Server = csb.DataSource;
                if (!string.IsNullOrEmpty(csb.InitialCatalog))
                    result.DatabaseName = csb.InitialCatalog;
                if (csb.IntegratedSecurity)
                {
                    result.AuthMethod = "SSPI";
                }
                else
                {
                    result.AuthMethod = "SQL";
                    result.Auth_SQL_Username = csb.UserID;
                    result.Auth_SQL_Password = csb.Password;
                }
            }
            catch (Exception)
            {
                // Ignore Parsing errors
            }

            return result;
        }

        public SqlConnectionStringBuilder ToConnectionString()
        {
            var csb = new SqlConnectionStringBuilder()
            {
                DataSource = Server,
                InitialCatalog = DatabaseName,
                IntegratedSecurity = (AuthMethod.Equals("SSPI", StringComparison.OrdinalIgnoreCase)),
                UserID = (AuthMethod.Equals("SQL", StringComparison.OrdinalIgnoreCase)) ? Auth_SQL_Username : string.Empty,
                Password = (AuthMethod.Equals("SQL", StringComparison.OrdinalIgnoreCase)) ? Auth_SQL_Password : string.Empty,
                ApplicationName = "Disco ICT WebApp",
                MultipleActiveResultSets = true,
                Pooling = true
            };

            return csb;
        }

        [Required(ErrorMessage = "The Server name is required")]
        public string Server { get; set; }
        [Required(ErrorMessage = "The Database name is required")]
        public string DatabaseName { get; set; }
        [Required(ErrorMessage = "The Authentication Method is required")]
        public string AuthMethod { get; set; }

        [CustomValidation(typeof(DatabaseModel), "SqlAuthRequired", ErrorMessage = "When using SQL Authentication a Username is required")]
        public string Auth_SQL_Username { get; set; }
        [DataType(DataType.Password), CustomValidation(typeof(DatabaseModel), "SqlAuthRequired", ErrorMessage="When using SQL Authentication a Password is required")]
        public string Auth_SQL_Password { get; set; }

        public List<SelectListItem> AuthMethods
        {
            get
            {
                return new List<SelectListItem>(){
                    new SelectListItem(){
                         Value="SSPI", Text="Integrated Authentication", Selected=true
                    },
                    new SelectListItem(){
                         Value="SQL", Text="SQL Authentication", Selected=false
                    }
                };
            }
        }

        public static ValidationResult SqlAuthRequired(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance as DatabaseModel;

            if (instance != null && instance.AuthMethod != null && instance.AuthMethod.Equals("SQL", StringComparison.OrdinalIgnoreCase))
            {
                var stringValue = value as string;
                if (string.IsNullOrWhiteSpace(stringValue))
                    return null; // Invalid
            }

            return ValidationResult.Success;
        }

    }
}