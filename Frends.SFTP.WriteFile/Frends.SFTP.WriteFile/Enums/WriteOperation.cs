// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.WriteFile.Enums;

/// <summary>
/// Enumeration for operation if destination file exists.
/// </summary>
public enum WriteOperation
{
    Append,
    Overwrite,
    Error
}

