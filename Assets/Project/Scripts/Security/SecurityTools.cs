using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class SecurityTools
{
    private static string KeyLengthFix(string key, int size)
    {
        string result = null;
        if (string.IsNullOrEmpty(key))
        {
            key = string.Empty;
        }
        if (key.Length == size)
        {
            result = key;
        }
        else if (key.Length > size)
        {
            result = key.Substring(0, size);
        }
        else
        {
            result = key;
            while (result.Length < size)
            {
                result = result + "0";
            }
        }
        return result;
    }


    public static string EncryptAes(string plainText, string Key, string IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        string encryptedString = null;
        byte[] encrypted;

        if (string.IsNullOrEmpty(plainText) == false)
        {
            // Create a new AesManaged
            using (AesManaged aes = new AesManaged())
            {
                // Set Aes Properties
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Key = Encoding.ASCII.GetBytes(KeyLengthFix(Key, 24));
                aes.IV = Encoding.ASCII.GetBytes(KeyLengthFix(IV, aes.BlockSize / 8));
                // Create Encryptor
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                // Create MemoryStream
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create CryptoStream
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        encrypted = ms.ToArray();
                        encryptedString = Convert.ToBase64String(encrypted);
                    }
                }
            }
        }
        return encryptedString;
    }

    public static string DecryptAes(string EncryptedText, string Key, string IV)
    {
        byte[] cipherText = Convert.FromBase64String(EncryptedText);

        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        string plainText = null;
        // Create a new AesManaged
        using (AesManaged aes = new AesManaged())
        {
            // Set Aes Properties
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Key = Encoding.ASCII.GetBytes(KeyLengthFix(Key, 24));
            aes.IV = Encoding.ASCII.GetBytes(KeyLengthFix(IV, aes.BlockSize / 8));
            // Create Decryptor
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            // Create MemoryStream
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                // Create CryptoStream
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    // Create StreamReader and Read data
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        plainText = sr.ReadToEnd();
                    }
                }
            }
        }
        return plainText;
    }
}
