using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// https://stackoverflow.com/questions/38649459/encryption-in-unity-c-sharp
public static class VCrypto
{
    private const string saltIndiatior = "MySaltIndacationString";
    private const int startSaltLength = 32;
    private const int endSaltLength = 32;

    public static string RandomString(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*<>+-=_";
        var stringChars = new char[length];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
    private static string GetSalt(int max)
    {
        byte[] salt = new byte[max];
        RNGCryptoServiceProvider.Create().GetNonZeroBytes(salt);
        return UTF8Encoding.UTF8.GetString(salt);
    }
    /// <summary>
    /// Encrypts a message
    /// </summary>
    /// <param name="toEncrypt">plain text to encrypt</param>
    /// <param name="key">Length must be 32</param>
    /// <returns></returns>
    public static string Encrypt(string toEncrypt, string key)
    {
        string startSalt = RandomString(startSaltLength);
        string salt = GetSalt(endSaltLength);
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
        // 256-AES key
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(startSalt + saltIndiatior + "{" + salt.Length + "}" + toEncrypt + salt);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return System.Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    /// <summary>
    /// Decrypts a message
    /// </summary>
    /// <param name="toDecrypt">encrypt</param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Decrypt(string toDecrypt, string key)
    {
        //Debug.Log("Decrypting::" + toDecrypt);
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
        // AES-256 key
        byte[] toEncryptArray = System.Convert.FromBase64String(toDecrypt);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        string decoded = UTF8Encoding.UTF8.GetString(resultArray);

        //Debug.Log("0(decoded)::" + decoded);
        //Remove 32 char long startSalt
        decoded = decoded.Remove(0, startSaltLength);

        if (decoded.StartsWith(saltIndiatior))
        {
            string saltSize = decoded.Substring(0, 1 + decoded.IndexOf("}"));
            //Debug.Log("1 (salt Indicator):: " + saltSize);
            int saltLength = int.Parse(saltSize.Replace(saltIndiatior, "").Replace("{", "").Replace("}", ""));
            //Debug.Log("2(saltLength):: " + saltLength);
            //Debug.Log("3(salt)::" + decoded.Substring(decoded.Length - saltLength, saltLength));
            decoded = decoded.Remove(0, saltSize.Length);
            if (saltLength > 0)
            {
                decoded = decoded.Remove(decoded.Length - saltLength, saltLength);
            }
            //Debug.Log("Finished:: " + decoded);
            return decoded;
        }
        else
        {
            return decoded;
        }

    }
}