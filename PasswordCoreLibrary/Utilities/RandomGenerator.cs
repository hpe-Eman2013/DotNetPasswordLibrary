using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordCoreLibrary.Utilities
{
    public static class RandomGenerator
    {
        private static readonly Random GetRandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (GetRandom)
            {
                return GetRandom.Next(min, max);
            }
        }
        public static string GetRandomUnicodeString(int length, int maxValue, Predicate<int> valueFilter)
        {
            var seedBuff = new byte[4];
            byte[] charBuff;

            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(seedBuff); // The array is now filled with cryptographically strong random bytes.

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, new UTF8Encoding(false, false)))
                {
                    var random = new Random(BitConverter.ToInt32(seedBuff, 0));

                    for (var i = 0; i < length; i++)
                    {
                        var temp = random.Next(maxValue); //should we cap? 

                        while (!valueFilter(temp))
                            temp = random.Next(maxValue);

                        sw.Write((char)temp);
                    }
                }
                charBuff = ms.ToArray();
            }
            return new UTF8Encoding(false, false).GetString(charBuff);
        }

        public static string GetRandomAsciiString(int length)
        {
            return GetRandomUnicodeString(length, 0x7E, o => o == 0x21 || o >= 0x23 && o <= 0x25 || o >= 0x30 && o <= 0x7E);
            //return GetRandomUnicodeString(length, 126, o => o >= 33 && o <= 126); //could use integers instead
        }
    }
    
}
