#pragma warning disable 1591

namespace Frends.SFTP.ReadFile.Definitions
{
    /// <summary>
    /// Enumeration to specify authentication type.
    /// </summary>
    public enum AuthenticationType
    {
        UsernamePassword,
        PrivateKey,
        PrivateKeyPassphrase
    }

    /// <summary>
    /// Enumeration to specify operation if destination file exists.
    /// </summary>
    public enum DestinationOperation
    {
        Rename,
        Overwrite,
        Error
    }
}
