using System.Collections.Generic;
using PasswordCoreLibrary.Models;
using PasswordCoreLibrary.Enums;

namespace PasswordCoreLibrary.Interfaces
{
    public interface IPasswordRepository
    {
        int UserId { get; set; }
        Passwords GetRecordById(int id);
        void EditEntry(int id, Passwords newValues);
        void DeleteEntry(int id);
        int InsertNewRecord(Passwords pw);
        Passwords GetRecordByCredentials(string username, string password);
        string GetRandomPassword(PasswordStrength ps);
        List<Passwords> GetRecords();
    }
}