using PasswordCoreLibrary.Enums;
using PasswordCoreLibrary.Interfaces;
using PasswordCoreLibrary.Utilities;

namespace PasswordCoreLibrary.Repositories
{
    public class Encryptor : IEncryptor
    {
        public int ShiftNumber { get; set; }
        
        public string GetHashedPassword(string pw, ShiftDirection direction, int shift)
        {
            try
            {
                shift = shift == 0 ? 1 : shift;
                int numHashChars = (shift * pw.Length) + shift;
                var pwHash = RandomGenerator.GetRandomAsciiString(numHashChars);
                pwHash = pwHash.Replace('\\', ':');
                string encryptedPassword = "";
                if (pw.Length == 0) return null;
                switch (direction)
                {
                    case ShiftDirection.ShiftLeft:
                        return ShiftHashToTheLeft(pw, shift, pwHash);
                    case ShiftDirection.ShiftRight:
                        return ShiftHashToTheRight(pw, shift, pwHash);
                    default:
                        for (int i = 0; i < pw.Length; i++)
                        {
                            encryptedPassword = string.Concat(encryptedPassword, pw.Substring(i, 1),
                                pwHash.Substring(i, 1));
                        }
                        break;
                }
                return encryptedPassword;
            }
            catch (System.Exception)
            {
                return pw;
            }
        }

        private string ShiftHashToTheLeft(string pw, int shift, string pwHash)
        {
            string encryptedPassword = "";
            int counter = 0;
            for (var i = 0; i < pw.Length; i++)
            {
                for (int j = 0; j < shift; j++)
                {
                    encryptedPassword = string.Concat(encryptedPassword, pwHash.Substring(counter, 1));
                    counter++;
                }
                encryptedPassword = string.Concat(encryptedPassword, pw.Substring(i, 1));
            }
            return encryptedPassword;
        }

        private string ShiftHashToTheRight(string pw, int shift, string pwHash)
        {
            string encryptedPassword = "";
            int counter = 0;
            for (var i = 0; i < pw.Length; i++)
            {
                encryptedPassword = string.Concat(encryptedPassword, pw.Substring(i, 1));

                for (int j = 0; j < shift; j++)
                {
                    encryptedPassword = string.Concat(encryptedPassword, pwHash.Substring(counter, 1));
                    counter++;
                }
            }
            return encryptedPassword;
        }

    }
}
