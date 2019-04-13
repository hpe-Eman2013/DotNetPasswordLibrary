using System.Collections.Generic;
using PasswordCoreLibrary.Enums;
using PasswordCoreLibrary.Models;

namespace PasswordCoreLibrary.Interfaces
{
    public interface ITextFile
    {
        string SourceFolder { get; set; }
        string DestinationFolder { get; set; }
        string DestinationFile { get; set; }
        bool PlacePasswordRecordsInFile(string pin, ShiftDirection direction);
        bool SerializeJsonToFile(List<Passwords> pwList, string pin);
        int AddPasswordListToDb();
        int GetUserIdFromPin(string entryPin);
        int GetUserIdFromFile(string sourceFileName);
        string GetPinFromUserId(int userId, string fileName = "");
    }
}
