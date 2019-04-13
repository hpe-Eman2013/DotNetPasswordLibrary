using PasswordCoreLibrary.Models;
using System.Collections.Generic;

namespace PasswordCoreLibrary.Interfaces
{
    public interface IAccountRepository
    {
        void DeleteEntry(int id);
        int EditEntry(AccountUser newValues);
        AccountUser GetRecordByCredentials(string username, string password);
        AccountUser GetRecordById(int id);
        List<AccountUser> GetRecords();
        int InsertNewRecord(AccountUser record);

    }
}
