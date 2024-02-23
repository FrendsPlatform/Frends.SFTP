// Pragma is for self-explanatory enum attributes.
#pragma warning disable 1591, SA1602

namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Enumeration to specify actions if the source file is not found.
/// </summary>
public enum SourceAction
{
    Error,
    Info,
    Ignore,
}