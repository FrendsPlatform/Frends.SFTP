#pragma warning disable 1591

namespace Frends.SFTP.ListFiles.Definitions
{
    /// <summary>
    /// Enumeration to specify operation if destination file exists.
    /// </summary>
    public enum DestinationOperation
    {
        Rename,
        Overwrite,
        Error
    }

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
    /// Enumeration to specify if the directory listing should contain files, directories or both.
    /// </summary>
    public enum IncludeType
    {
        File,
        Directory,
        Both
    }
}
