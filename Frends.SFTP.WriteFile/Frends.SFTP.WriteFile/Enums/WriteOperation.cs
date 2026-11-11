namespace Frends.SFTP.WriteFile.Enums;

/// <summary>
/// Enumeration for operation if destination file exists.
/// </summary>
public enum WriteOperation
{
    /// <summary>
    /// Append content to the existing file.
    /// </summary>
    Append,

    /// <summary>
    /// Replace the existing file content.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Fail if the destination file already exists.
    /// </summary>
    Error,
}
