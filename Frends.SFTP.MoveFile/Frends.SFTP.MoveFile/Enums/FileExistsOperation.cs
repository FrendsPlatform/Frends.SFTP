namespace Frends.SFTP.MoveFile.Enums;

/// <summary>
/// Operation which will be made when target file exists in destination.
/// </summary>
public enum FileExistsOperation
{
    /// <summary>
    /// Exception is thrown.
    /// </summary>
    Throw,
    /// <summary>
    /// Transferred file will be renamed by appending a number to the end.
    /// </summary>
    Rename,
    /// <summary>
    /// Transferred file will overwrite the existing file.
    /// </summary>
    Overwrite
}
