namespace Frends.SFTP.DownloadFiles.Definitions;

internal enum TransferState
{
    RenameSourceFileBeforeTransfer,
    GetFile,
    MessageProcessing,
    PutFile,
    SourceOperationMove,
    SourceOperationRename,
    SourceOperationDelete,
    RestoreSourceFile,
    RemoveTemporaryDestinationFile,
    AppendToDestinationFile,
    DeleteDestinationFile,
    RenameDestinationFile,
    CleanUpFiles,
    CheckIfDestinationFileExists,
    RestoreModified,
    Connection
}

