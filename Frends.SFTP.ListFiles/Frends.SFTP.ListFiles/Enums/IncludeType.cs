// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.ListFiles.Enums;

/// <summary>
/// Enumeration to specify if the directory listing should contain files, directories or both.
/// </summary>
public enum IncludeType
{
    File,
    Directory,
    Both
}

