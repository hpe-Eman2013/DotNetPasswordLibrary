using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PasswordCoreLibrary.Interfaces;
using PasswordCoreLibrary.Models;

namespace PasswordCoreLibrary.Repositories
{
    public class EmailAccountRepository : IRepository<EmailAccount>, IEmailAccountRepository
    {
        private readonly IDbConnector _connector;
        private readonly string _tableName;

        public EmailAccountRepository(IDbConnector connector)
        {
            _connector = connector;
            _tableName = _connector.GetAppSettingsValue("EmailAccounts");
        }

        public EmailAccount GetRecordById(int id)
        {
            var emailRecords = GetRecords();
            return emailRecords.FirstOrDefault(p => p.UserId == id);
        }

        public void EditEntry(int id, EmailAccount newValues)
        {
            var query = UpdateQueryStatement();
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd = InsertEmailValuesIntoCommandObject(newValues, cmd);
            cmd.Parameters.AddWithValue("@id", newValues.Id);
            _connector.ExecuteSqlStatement(cmd);
        }
        
        private string UpdateQueryStatement()
        {
            var queryStatement = "Update " + _tableName +
                                 " Set Email = @email, " +
                                 "UserId = @userId, " +
                                 "PhotoLocation = @photoLocation " +
                                 "Where ID = @id";
            return queryStatement;
        }

        public void DeleteEntry(int id)
        {
            try
            {
                var query = $"Delete From {_tableName} Where Id = @id";
                var cmd = new SqlCommand(query, _connector.GetConnectionObject());
                cmd.Parameters.AddWithValue("@id", id);
                _connector.ExecuteSqlStatement(cmd);
            }
            catch (Exception)
            {
                throw new Exception("There was an error deleting the selected record!");
            }
        }

        public int InsertNewRecord(EmailAccount record)
        {
            var query = InsertQueryStatement();
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd = InsertEmailValuesIntoCommandObject(record, cmd);
            return Convert.ToInt32(_connector.ExecuteSqlScalarStatement(cmd));
        }

        private string InsertQueryStatement()
        {
            var queryStatement = "Insert Into " + _tableName +
                                 "(Email, UserId, PhotoLocation) " +
                                 "Values(@email, @userId, @photoLocation) " +
                                 "SELECT SCOPE_IDENTITY()";
            return queryStatement;
        }

        private SqlCommand InsertEmailValuesIntoCommandObject(EmailAccount email, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@email", email.Email);
            cmd.Parameters.AddWithValue("@userId", email.UserId);
            cmd.Parameters.AddWithValue("@photoLocation", email.PhotoLocation);
            return cmd;
        }

        public EmailAccount GetRecordByCredentials(string username, string password)
        {
            throw new NotImplementedException();
        }

        public List<EmailAccount> GetRecords()
        {
            var cmd = new SqlCommand($"Select * From {_tableName}", _connector.GetConnectionObject());
            var dt = new DataTable();
            cmd.Connection.Open();
            dt.Load(cmd.ExecuteReader());
            cmd.Connection.Close();
            var emailLists = (from e in dt.AsEnumerable()
                select new EmailAccount()
                {
                    Id = Convert.ToInt32(e["Id"]),
                    Email = e["Email"].ToString(),
                    UserId = Convert.ToInt32(e["UserId"]),
                    PhotoLocation = e["PhotoLocation"].ToString()
                }).ToList();
            return emailLists;
        }
    }
}
