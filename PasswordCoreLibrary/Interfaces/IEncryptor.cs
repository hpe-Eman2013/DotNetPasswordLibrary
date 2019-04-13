using PasswordCoreLibrary.Enums;

namespace PasswordCoreLibrary.Interfaces
{
    public interface IEncryptor
    {
        int ShiftNumber { get; set; }
        string GetHashedPassword(string pw, ShiftDirection direction, int shift);
    }
}
