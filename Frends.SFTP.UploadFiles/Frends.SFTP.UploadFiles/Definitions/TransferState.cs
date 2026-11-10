namespace Frends.SFTP.UploadFiles.Definitions;

internal enum TransferState
{
    /// <summary>
    /// Renaming the source file before transfer.
    /// </summary>
    RenameSourceFileBeforeTransfer,

    /// <summary>
    /// Downloading the source file to the local working directory.
    /// </summary>
    GetFile,

    /// <summary>
    /// Processing a transfer message.
    /// </summary>
    MessageProcessing,

    /// <summary>
    /// Uploading the file to the destination.
    /// </summary>
    PutFile,

    /// <summary>
    /// Moving the source file after transfer.
    /// </summary>
    SourceOperationMove,

    /// <summary>
    /// Renaming the source file after transfer.
    /// </summary>
    SourceOperationRename,

    /// <summary>
    /// Deleting the source file after transfer.
    /// </summary>
    SourceOperationDelete,

    /// <summary>
    /// Restoring the source file after a failed transfer.
    /// </summary>
    RestoreSourceFile,

    /// <summary>
    /// Removing a temporary destination file.
    /// </summary>
    RemoveTemporaryDestinationFile,

    /// <summary>
    /// Appending the source file to an existing destination file.
    /// </summary>
    AppendToDestinationFile,

    /// <summary>
    /// Deleting an existing destination file.
    /// </summary>
    DeleteDestinationFile,

    /// <summary>
    /// Renaming the destination file.
    /// </summary>
    RenameDestinationFile,

    /// <summary>
    /// Removing temporary working files.
    /// </summary>
    CleanUpFiles,

    /// <summary>
    /// Checking whether the destination file already exists.
    /// </summary>
    CheckIfDestinationFileExists,

    /// <summary>
    /// Creating destination directories.
    /// </summary>
    CreateDestinationDirectories,

    /// <summary>
    /// Connecting to the SFTP server.
    /// </summary>
    Connection,

    /// <summary>
    /// Restoring the source file modification time.
    /// </summary>
    RestoreModified,

    /// <summary>
    /// Searching for source files.
    /// </summary>
    CheckSourceFiles,
}
