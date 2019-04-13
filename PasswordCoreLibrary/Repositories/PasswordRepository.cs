using PasswordCoreLibrary.Enums;
using PasswordCoreLibrary.Interfaces;
using PasswordCoreLibrary.Models;
using PasswordCoreLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace PasswordCoreLibrary.Repositories
{
    public class PasswordRepository : IRepository<Passwords>, IPasswordRepository
    {
        private readonly IDbConnector _connector;
        private readonly string _tableName;
        private readonly string _storedProc;
        public PasswordRepository(IDbConnector db)
        {
            _connector = db;
            _tableName = _connector.GetAppSettingsValue("PasswordTracker");
            _storedProc = _connector.GetAppSettingsValue("spPasswordRecords");
        }
        public int UserId { get; set; }

        public Passwords GetRecordById(int id)
        {
            var passwordRecords = GetRecords();
            return passwordRecords.FirstOrDefault(p => p.Id == id);
        }

        public void EditEntry(int id, Passwords newValues)
        {
            var query = UpdateQueryStatement();
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd = InsertPasswordValuesIntoCommandObject(newValues, cmd);
            cmd.Parameters.AddWithValue("@id", id);
            _connector.ExecuteSqlStatement(cmd);
        }

        public void DeleteEntry(int id)
        {
            try
            {
                var query = $"Delete From {_tableName} Where ID = @id";
                var cmd = new SqlCommand(query, _connector.GetConnectionObject());
                cmd.Parameters.AddWithValue("@id", id);
                _connector.ExecuteSqlStatement(cmd);
            }
            catch (Exception)
            {
                throw new Exception("There was an error deleting the selected record!");
            }
        }

        public int InsertNewRecord(Passwords pw)
        {
            var query = InsertQueryStatement();
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            pw.DateCreated = string.IsNullOrEmpty(pw.DateCreated.ToString(CultureInfo.InvariantCulture)) 
                ? DateTime.Today 
                : pw.DateCreated;
            pw.DateModified = string.IsNullOrEmpty(pw.DateModified.ToString(CultureInfo.InvariantCulture))
                ? DateTime.Today
                : pw.DateModified;
            cmd = InsertPasswordValuesIntoCommandObject(pw, cmd);
            return Convert.ToInt32(_connector.ExecuteSqlScalarStatement(cmd));
        }

        private SqlCommand InsertPasswordValuesIntoCommandObject(Passwords pwToUpdate, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@name", pwToUpdate.Name);
            cmd.Parameters.AddWithValue("@username", pwToUpdate.Username);
            cmd.Parameters.AddWithValue("@password", pwToUpdate.Password);
            cmd.Parameters.AddWithValue("@description", pwToUpdate.Description);
            cmd.Parameters.AddWithValue("@applicationPath", pwToUpdate.ApplicationPath);
            cmd.Parameters.AddWithValue("@specialNotes", pwToUpdate.SpecialNotes);
            cmd.Parameters.AddWithValue("@dateCreated", pwToUpdate.DateCreated);
            cmd.Parameters.AddWithValue("@dateModified", pwToUpdate.DateModified);
            cmd.Parameters.AddWithValue("@userId", pwToUpdate.UserId);
            return cmd;
        }

        public Passwords GetRecordByCredentials(string username, string password)
        {
            var pw = GetRecords();

            return pw.FirstOrDefault(p => p.Username.ToLower().Equals(username.ToLower())
                                          && p.Password.ToLower().Equals(password.ToLower()));
        }

        public string GetRandomPassword(PasswordStrength ps)
        {
            return RandomGenerator.GetRandomAsciiString((int)ps);
        }

        private string UpdateQueryStatement()
        {
            var queryStatement = "Update " + _tableName +
                                 " Set Name = @name, " +
                                 "Username = @username, " +
                                 "Password = @password, " +
                                 "Description = @description, " +
                                 "ApplicationPath = @applicationPath, " +
                                 "SpecialNotes = @specialNotes, " +
                                 "DateCreated = @dateCreated, " +
                                 "DateModified = @dateModified, " +
                                 "UserId = @userId " +
                                 "Where ID = @id";
            return queryStatement;
        }

        private string InsertQueryStatement()
        {
            var queryStatement = "Insert Into " + _tableName +
            "(Name, Username, Password, Description, " +
            "ApplicationPath, SpecialNotes, DateCreated, DateModified, UserId) " +
            "Values(@name, @username, @password, @description, " +
            "@applicationPath, @specialNotes, @dateCreated, @dateModified, @userId) " +
                "SELECT SCOPE_IDENTITY()";
            return queryStatement;
        }
        

        public List<Passwords> GetRecords()
        {
            var com = new SqlCommand(_storedProc, _connector.GetConnectionObject())
            {
                CommandType = CommandType.StoredProcedure
            };
            com.Parameters.AddWithValue("@userId", UserId);
            var dt = new DataTable();
            com.Connection.Open();
            dt.Load(com.ExecuteReader());
            com.Connection.Close();
            var passwordLists = (from pw in dt.AsEnumerable()
                select new Passwords()
                {
                    Id = Convert.ToInt32(pw["ID"]),
                    Name = pw["Name"].ToString(),
                    Username = pw["Username"].ToString(),
                    Password = pw["Password"].ToString(),
                    Description = pw["Description"].ToString(),
                    ApplicationPath = pw["ApplicationPath"].ToString(),
                    SpecialNotes = pw["SpecialNotes"].ToString(),
                    DateCreated = Convert.ToDateTime(pw["DateCreated"].ToString()),
                    DateModified = Convert.ToDateTime(pw["DateModified"].ToString()),
                    UserId = Convert.ToInt32(pw["UserId"])
                }).ToList();
            return passwordLists;
        }
    }
}
