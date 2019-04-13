using PasswordCoreLibrary.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PasswordCoreLibrary.Utilities
{
    public class DbConnector : IDbConnector
    {
        public Dictionary<string, string> ConfigDictionary { get; set; }

        public DbConnector()
        {
            ConfigDictionary = ConfigDictionary ?? GetPasswordSettings();
        }

        public string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PasswordSettings"].ConnectionString;
        }

        public string GetAppSettingsValue(string key)
        {
            return ConfigDictionary.FirstOrDefault(x => x.Key.Equals(key)).Value;
        }

        public SqlConnection GetConnectionObject()
        {
            return new SqlConnection(GetConnectionString());
        }

        public void ExecuteSqlStatement(SqlCommand command)
        {
            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public dynamic ExecuteSqlScalarStatement(SqlCommand command)
        {
            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();

            dynamic value = command.ExecuteScalar();
            command.Connection.Close();
            return value;
        }

        private Dictionary<string, string> GetPasswordSettings()
        {
            using (var cmd = new SqlCommand("Select * From dbo.PasswordSettings", GetConnectionObject()))
            {
                cmd.Connection.Open();
                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                
                var settings = dt.AsEnumerable().ToDictionary(row => row["KeyName"].ToString(), row => row["KeyValue"].ToString());
                return settings;
            }

        }
    }
}
