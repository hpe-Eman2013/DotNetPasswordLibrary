using System.Collections.Generic;
using PasswordCoreLibrary.Models;

namespace PasswordCoreLibrary.Repositories
{
    public interface IEmailAccountRepository
    {
        EmailAccount GetRecordById(int id);
        void EditEntry(int id, EmailAccount newValues);
        void DeleteEntry(int id);
        int InsertNewRecord(EmailAccount record);
        EmailAccount GetRecordByCredentials(string username, string password);
        List<EmailAccount> GetRecords();

    }
}