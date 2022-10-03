namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Enumeration to specify operation for the source file after transfer.
/// </summary>
public enum SourceOperation
{
    /// <summary>
    /// Deletes the source file after transfer.
    /// </summary>
    Delete,
    /// <summary>
    /// Renames the source file after transfer.
    /// </summary>
    Rename,
    /// <summary>
    /// Moves the source file after transfer.
    /// </summary>
    Move,
    /// <summary>
    /// Leaves the source file unchanged.
    /// </summary>
    Nothing
}

