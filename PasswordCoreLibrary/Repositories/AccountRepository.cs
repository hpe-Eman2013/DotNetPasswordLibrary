using PasswordCoreLibrary.Interfaces;
using PasswordCoreLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PasswordCoreLibrary.Repositories
{
    public class AccountRepository : IRepository<AccountUser>, IAccountRepository
    {
        private readonly IDbConnector _connector;
        private readonly string _tableName;

        public AccountRepository(IDbConnector db)
        {
            _connector = db;
            _tableName = _connector.GetAppSettingsValue("UserAccount");
        }
        public List<AccountUser> AccountUsers { get; set; }
        public void DeleteEntry(int id)
        {
            if (id == 0)
                throw new Exception("There was an error deleting the selected record!");
            DeletePasswordFile(id);
        }

        private void DeletePasswordFile(int id)
        {
            var query = $"Delete From {_tableName} Where UserId = @userId";
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd.Parameters.AddWithValue("@userId", id);
            _connector.ExecuteSqlStatement(cmd);
        }

        public void EditEntry(int id, AccountUser newValues)
        {
            throw new NotImplementedException();
        }

        public int EditEntry(AccountUser newValues)
        {
            var query = UpdateQueryStatement();
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd = InsertAccountParameterValues(newValues, cmd);
            cmd.Parameters.AddWithValue("@userId", newValues.UserId);
            _connector.ExecuteSqlStatement(cmd);
            return newValues.UserId;
        }

        public AccountUser GetRecordByCredentials(string username, string password)
        {
            var pw = GetRecords();
            return pw.FirstOrDefault(p => p.Username.ToLower().Equals(username.ToLower())
                                          && p.Password.ToLower().Equals(password.ToLower()));
        }

        public AccountUser GetRecordById(int id)
        {
            return GetRecords().FirstOrDefault(a => a.UserId == id);
        }

        public List<AccountUser> GetRecords()
        {
            var com = new SqlCommand($"Select * From {_tableName} ", _connector.GetConnectionObject());
            DataTable dt = new DataTable();
            com.Connection.Open();
            dt.Load(com.ExecuteReader());
            AccountUsers = (from pw in dt.AsEnumerable()
                            select new AccountUser()
                            {
                                UserId = Convert.ToInt32(pw["UserId"]),
                                FirstName = pw["FirstName"].ToString(),
                                LastName = pw["LastName"].ToString(),
                                Username = pw["Username"].ToString(),
                                Password = pw["Password"].ToString(),
                                Pin = pw["Pin"].ToString()
                            }).ToList();
            com.Connection.Close();
            return AccountUsers;
        }

        public int InsertNewRecord(AccountUser record)
        {
            var existingRec = GetRecordByCredentials(record.Username, record.Password);
            if (existingRec != null) return 0;
            var query = InsertQueryStatement();
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd = InsertAccountParameterValues(record, cmd);
            _connector.ExecuteSqlStatement(cmd);
            return GetRecordByCredentials(record.Username, record.Password).UserId;
        }

        private string UpdateQueryStatement()
        {
            var queryStatement = $"Update {_tableName} " +
                                 "Set Username = @username, " +
                                 "Password = @password, " +
                                 "FirstName = @firstname, " +
                                 "LastName = @lastname, " +
                                 "Pin = @pin " +
                                 "Where UserId = @userId";
            return queryStatement;
        }
        private string InsertQueryStatement()
        {
            var queryStatement =
                $"Insert Into {_tableName} " +
                "(Username, Password, FirstName, LastName, Pin) " +
                "Values(@username, @password, @firstname, @lastname, @pin) " +
                "SELECT SCOPE_IDENTITY()";
            return queryStatement;
        }

        private SqlCommand InsertAccountParameterValues(AccountUser user, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@firstname", user.FirstName);
            cmd.Parameters.AddWithValue("@lastname", user.LastName);
            cmd.Parameters.AddWithValue("@pin", user.Pin);
            return cmd;
        }
    }
}
