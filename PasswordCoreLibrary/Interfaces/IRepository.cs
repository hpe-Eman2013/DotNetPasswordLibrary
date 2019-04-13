using System.Collections.Generic;

namespace PasswordCoreLibrary.Interfaces
{
    public interface IRepository<T>
    {
        T GetRecordById(int id);
        void EditEntry(int id, T newValues);
        void DeleteEntry(int id);
        int InsertNewRecord(T record);
        T GetRecordByCredentials(string username, string password);
        List<T> GetRecords();
    }
}
