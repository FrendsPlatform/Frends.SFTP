namespace Frends.SFTP.RenameFile.Enums;
/// <summary>
/// Rename behaviour if a file with the new name already exists.
/// </summary>
public enum RenameBehaviour
{
    /// <summary>
    /// Exception is thrown.
    /// </summary>
    Throw,
    /// <summary>
    /// The existing file is overwritten.
    /// </summary>
    Overwrite,
    /// <summary>
    /// A number is appended to the end of the new file name.
    /// </summary>
    Rename
}
