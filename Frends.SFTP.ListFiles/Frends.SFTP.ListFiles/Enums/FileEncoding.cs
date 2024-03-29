﻿// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.ListFiles.Enums;

/// <summary>
/// Enumeration of file encoding options.
/// </summary>
public enum FileEncoding
{
    UTF8,
    ANSI,
    ASCII,
    WINDOWS1252,
    /// <summary>
    /// Other enables users to add other encoding options as string.
    /// </summary>
    Other
}
