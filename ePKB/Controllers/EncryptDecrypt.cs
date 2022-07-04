using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public static class EncryptDecrypt
{
    const string pwd = "^rAH8eF4fpwB!bMZt*ff^av@N3BnaN3!G6cf8j^UWpGqC?dfX+8gk9w-T7n6zzGcsBQVhbyAS_Q5P8WXgX-QRkBbtr+RBSSJwEwx?h7@#ucyMmR3=?--#AWyvwzdjxbRUget5=bKM%#vW%6YfHYrZz&hRuhaHCG&5Py!=@-xc57Yp^vKQ&FA9Jq$jLAvV55a_fu2H?RhPpU^S--G6RL8VgE+=GK9SLYQc9#WJY8QQG8vz+dNMUj%X_kf&T6$Em+w";
    public static string Encrypt(string text)
    {
        byte[] originalBytes = Encoding.UTF8.GetBytes(text);
        byte[] encryptedBytes = null;
        byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);

        // Hash the password with SHA256
        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        // Generating salt bytes
        byte[] saltBytes = GetRandomBytes();

        // Appending salt bytes to original bytes
        byte[] bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
        for (int i = 0; i < saltBytes.Length; i++)
        {
            bytesToBeEncrypted[i] = saltBytes[i];
        }
        for (int i = 0; i < originalBytes.Length; i++)
        {
            bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];
        }

        encryptedBytes = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string decryptedText)
    {
        byte[] bytesToBeDecrypted = Convert.FromBase64String(decryptedText);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);

        // Hash the password with SHA256
        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        byte[] decryptedBytes = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

        // Getting the size of salt
        int _saltSize = 4;

        // Removing salt bytes, retrieving original bytes
        byte[] originalBytes = new byte[decryptedBytes.Length - _saltSize];
        for (int i = _saltSize; i < decryptedBytes.Length; i++)
        {
            originalBytes[i - _saltSize] = decryptedBytes[i];
        }

        return Encoding.UTF8.GetString(originalBytes);
    }

    private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[] encryptedBytes = null;

        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }
                encryptedBytes = ms.ToArray();
            }
        }

        return encryptedBytes;
    }

    private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[] decryptedBytes = null;

        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }
                decryptedBytes = ms.ToArray();
            }
        }

        return decryptedBytes;
    }

    private static byte[] GetRandomBytes()
    {
        int _saltSize = 4;
        byte[] ba = new byte[_saltSize];
        RNGCryptoServiceProvider.Create().GetBytes(ba);
        return ba;
    }

    public static string SHA1(string input)
    {
        using (SHA1Managed sha1 = new SHA1Managed())
        {
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}