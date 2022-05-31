// Pragma is for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Enumeration to specify actions if the source file is not found.
    /// </summary>
    public enum SourceAction
    {
        Error,
        Info,
        Ignore
    }
}
