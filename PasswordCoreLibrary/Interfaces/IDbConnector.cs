using System.Collections.Generic;
using System.Data.SqlClient;

namespace PasswordCoreLibrary.Interfaces
{
    public interface IDbConnector
    {
        Dictionary<string, string> ConfigDictionary { get; set; }
        string GetConnectionString();
        SqlConnection GetConnectionObject();
        string GetAppSettingsValue(string key);
        dynamic ExecuteSqlScalarStatement(SqlCommand command);
        void ExecuteSqlStatement(SqlCommand command);
    }
}