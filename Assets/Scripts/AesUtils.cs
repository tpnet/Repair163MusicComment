using System;
using System.Security.Cryptography;
using System.Text;

public static class AesUtils
{
    public static string Decrypt(string toDecrypt, string key)
    {
        if (string.IsNullOrEmpty(toDecrypt))
        {
            return "";
        }

        byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = Encoding.UTF8.GetBytes(key);
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return Encoding.UTF8.GetString(resultArray);
    }

    public static string Encrypt(string toEncrypt, string key)
    {
        if (string.IsNullOrEmpty(toEncrypt))
        {
            return "";
        }

        byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = Encoding.UTF8.GetBytes(key);
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }
}