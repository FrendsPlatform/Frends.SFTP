// Pragma is for the self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Enumeration of the current states. Used in Operations log.
    /// </summary>
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
}
