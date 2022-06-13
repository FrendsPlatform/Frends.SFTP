// Pragma is for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Enumeration to specify operation for the source file after transfer.
/// </summary>
public enum SourceOperation
{
    Delete,
    Rename,
    Move,
    Nothing
}

