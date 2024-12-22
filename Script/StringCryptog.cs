using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 加密和解密字串的腳本。
/// </summary>
public static class StringCryptog
{
    /// <summary>
    /// 對稱式加密演算法
    /// </summary>
    /// <param name="plainText">要加密的字串</param>
    /// <param name="key">加密用的金鑰</param>
    /// <returns></returns>
    public static string Encode(string plainText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); // AES-256
            aesAlg.GenerateIV();
            byte[] iv = aesAlg.IV;

            using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv))
            using (var msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(iv, 0, iv.Length); // Prepend IV to the encrypted data
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    byte[] inputBuffer = Encoding.UTF8.GetBytes(plainText);
                    csEncrypt.Write(inputBuffer, 0, inputBuffer.Length);
                    csEncrypt.FlushFinalBlock();
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    /// <summary>
    /// 對稱式解密演算法
    /// </summary>
    /// <param name="cipherText">要解密的字串</param>
    /// <param name="key">解密用的金鑰</param>
    /// <returns></returns>
    public static string Decode(string cipherText, string key)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); // AES-256
            byte[] iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv))
            using (var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}