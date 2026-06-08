namespace Devnet.Vault.Application.Security.Encryption.Interfaces;

public interface IEncryptionService
{
    /// <summary>
    /// Encrypt using fixed key defined in appsettings.json
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Dec
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Hashes the input
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    string Hash(string input);

    /// <summary>
    /// Encrypt the input with custome key
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="userKey"></param>
    /// <returns></returns>
    string EncryptWithUserKey(string plainText, string userKey);

    /// <summary>
    /// Decrypt the input with user key
    /// </summary>
    /// <param name="encryptedText"></param>
    /// <param name="userKey"></param>
    /// <returns></returns>
    string DecryptWithUserKey(string encryptedText, string userKey);
}
