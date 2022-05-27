#pragma warning disable 1591

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Enumeration to specify authentication type for the transfer.
    /// </summary>
    public enum AuthenticationType
    {
        UsernamePassword,
        UsernamePrivateKeyFile,
        UsernamePrivateKeyString,
        UsernamePasswordPrivateKeyFile,
        UsernamePasswordPrivateKeyString
    }
}
