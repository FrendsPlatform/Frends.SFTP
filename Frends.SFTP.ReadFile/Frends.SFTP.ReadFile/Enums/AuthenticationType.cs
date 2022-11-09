namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Enumeration to specify authentication type for the transfer.
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// Authentication type which uses username and password.
    /// </summary>
    UsernamePassword,
    /// <summary>
    /// Authentication type which uses username and private key file
    /// </summary>
    UsernamePrivateKeyFile,
    /// <summary>
    /// Authentication type which uses username and private key as a string.
    /// </summary>
    UsernamePrivateKeyString,
    /// <summary>
    /// Authentication type which uses username, password and private key file.
    /// </summary>
    UsernamePasswordPrivateKeyFile,
    /// <summary>
    /// Authentication type which uses username, password and private key as a string.
    /// </summary>
    UsernamePasswordPrivateKeyString
}
