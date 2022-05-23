#pragma warning disable 1591

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Enumeration of the current states
    /// </summary>
    public enum TransferState
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
}
