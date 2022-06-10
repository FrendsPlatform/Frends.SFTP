namespace Frends.SFTP.UploadFiles.Definitions;

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
    CheckIfDestinationFileExists
}

