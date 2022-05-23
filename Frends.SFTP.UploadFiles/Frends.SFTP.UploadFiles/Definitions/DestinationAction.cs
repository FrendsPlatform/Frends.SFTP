#pragma warning disable 1591

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Enumeration to specify actions if destination file exists.
    /// </summary>
    public enum DestinationAction
    {
        Append,
        Overwrite,
        Error
    }
}
