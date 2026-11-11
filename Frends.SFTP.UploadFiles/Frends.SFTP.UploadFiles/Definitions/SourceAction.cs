namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Enumeration to specify actions if the source file is not found.
/// </summary>
public enum SourceAction
{
    /// <summary>
    /// Returns a task failure when no source files are found.
    /// </summary>
    Error,

    /// <summary>
    /// Writes an informational message when no source files are found.
    /// </summary>
    Info,

    /// <summary>
    /// Ignores the absence of source files.
    /// </summary>
    Ignore,
}
