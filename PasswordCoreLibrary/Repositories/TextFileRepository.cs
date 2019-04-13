using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PasswordCoreLibrary.Interfaces;
using System.IO;
using System.Net.Mail;
using PasswordCoreLibrary.Enums;
using PasswordCoreLibrary.Models;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace PasswordCoreLibrary.Repositories
{
    public class TextFileRepository : ITextFile

    {
        private readonly IDbConnector _connector;
        private readonly IPasswordRepository _pwRepository;
        private readonly IEncryptor _encryptor;
        private string _destinationFileName = "";
        private readonly string _storedProc;

        public string SourceFolder { get; set; }
        public string DestinationFolder { get; set; }
        public string DestinationFile { get; set; }

        public TextFileRepository(IDbConnector connector, IPasswordRepository repo, IEncryptor encryptor)
        {
            _connector = connector;
            _pwRepository = repo;
            _encryptor = encryptor;
            SourceFolder = _connector.GetAppSettingsValue("sourceFolder");
            DestinationFolder = _connector.GetAppSettingsValue("destinationFolder");
            _storedProc = _connector.GetAppSettingsValue("spUserId");
        }

        public bool PlacePasswordRecordsInFile(string pin, ShiftDirection direction)
        {
            var pwList = GetPasswordRecordsByPin(pin);
            var newList = pwList.Select(x =>
            {
                x.Password = _encryptor.GetHashedPassword(x.Password, direction, _encryptor.ShiftNumber);
                return x;
            }).ToList();
            var isFinished = SerializeJsonToFile(newList, pin);
            return isFinished;
        }

        public string GetPinFromUserId(int userId, string fileName = "")
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                userId = GetUserIdFromFile(fileName);
            }
            var query = "Select Pin From dbo.UserAccount Where UserId = @userId";
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            cmd.Parameters.AddWithValue("@userId", userId);
            return _connector.ExecuteSqlScalarStatement(cmd).ToString();
        }

        public List<Passwords> GetPasswordRecordsByPin(string pin)
        {
            var id = GetUserIdFromPin(pin);
            // with the userId from the UserAccount table, save the 
            // record to the PasswordTracker table
            _pwRepository.UserId = Convert.ToInt32(id);
            return _pwRepository.GetRecords();
        }

        public int GetUserIdFromPin(string entryPin)
        {
            var qry = "Select UserId From dbo.UserAccount Where Pin = @pin";
            var cmd = new SqlCommand(qry, _connector.GetConnectionObject());
            cmd.Parameters.AddWithValue("@pin", entryPin);
            return _connector.ExecuteSqlScalarStatement(cmd);
        }

        public bool SerializeJsonToFile(List<Passwords> pwList, string pin)
        {
            try
            {
                _destinationFileName = string.IsNullOrEmpty(DestinationFile) ?
                    $"{GetNameFromUserAccount(pin)}OutputFile.json" : DestinationFile;
                var json = JsonConvert.SerializeObject(pwList);
                File.WriteAllText($"{DestinationFolder}{_destinationFileName}", json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetNameFromUserAccount(string pinNumber)
        {
            var qry = "Select FirstName From dbo.UserAccount Where Pin = @pin";
            var cmd = new SqlCommand(qry, _connector.GetConnectionObject());
            cmd.Parameters.AddWithValue("@pin", pinNumber);
            var name = _connector.ExecuteSqlScalarStatement(cmd);
            return name.ToString();
        }

        public int AddPasswordListToDb()
        {
            var sourceFileName = Directory.GetFiles(SourceFolder).FirstOrDefault();
            if (string.IsNullOrEmpty(sourceFileName)) throw new Exception("No files to add!");
            var userId = GetUserIdFromFile(sourceFileName);
            sourceFileName = ModifySourceFile(sourceFileName, userId);
            if (!IsFormatCorrect(sourceFileName)) return 0;

            Passwords pwList = ParsePasswordFile(sourceFileName);
            pwList.UserId = Convert.ToInt32(userId);
            var newRecId = _pwRepository.InsertNewRecord(pwList);
            DeleteSourceFile(sourceFileName);
            return Convert.ToInt32(newRecId);
        }

        public int GetUserIdFromFile(string sourceFileName)
        {
            var userId = GetUserIdFromEmailAddress(sourceFileName);
            return userId;
        }

        private dynamic GetUserIdFromEmailAddress(string sourceFileName)
        {
            var file = File.ReadAllLines(sourceFileName).ToList();
            var email = file.ElementAt(0).Trim();
            if (!IsValid(email)) throw new FormatException("The email is invalid or not present!");

            var cmd = new SqlCommand(_storedProc, _connector.GetConnectionObject())
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
            var userId = _connector.ExecuteSqlScalarStatement(cmd);
            return userId;
        }

        public bool IsValid(string emailaddress)
        {
            try
            {
                var mailAddress = new MailAddress(emailaddress);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string ModifySourceFile(string sourceFileName, int userId)
        {
            var lines = File.ReadAllLines(sourceFileName).ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                if (i == 0) lines.RemoveAt(i);
                if (lines[i].Contains("UserId"))
                {
                    lines[i] = $"\"UserId\" : {userId}";
                    break;
                }
            }
            File.WriteAllLines(sourceFileName, lines);
            return sourceFileName;
        }

        private bool IsFormatCorrect(string filename)
        {
            var json = File.ReadAllText(filename);
            var passwordFile = JsonConvert.DeserializeObject<Passwords>(json);
            if (string.IsNullOrEmpty(passwordFile.Name)) return false;
            if (string.IsNullOrEmpty(passwordFile.Username)) return false;
            if (string.IsNullOrEmpty(passwordFile.Password)) return false;
            return true;
        }

        private Passwords ParsePasswordFile(string fileName)
        {
            string json = File.ReadAllText(fileName);
            var passList = JsonConvert.DeserializeObject<Passwords>(json);
            return passList;
        }

        private void DeleteSourceFile(string fileToDelete)
        {
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }
    }
}
