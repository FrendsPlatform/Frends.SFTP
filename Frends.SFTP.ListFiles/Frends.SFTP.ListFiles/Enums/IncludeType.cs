namespace Frends.SFTP.ListFiles.Enums;

/// <summary>
/// Enumeration to specify if the directory listing should contain files, directories or both.
/// </summary>
public enum IncludeType
{
    /// <summary>
    /// Include files only.
    /// </summary>
    File,

    /// <summary>
    /// Include directories only.
    /// </summary>
    Directory,

    /// <summary>
    /// Include both files and directories.
    /// </summary>
    Both,
}
