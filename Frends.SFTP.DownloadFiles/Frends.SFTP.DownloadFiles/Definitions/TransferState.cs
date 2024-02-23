namespace Frends.SFTP.DownloadFiles.Definitions;

#pragma warning disable SA1602
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
    Connection,
    CheckSourceFiles,
}