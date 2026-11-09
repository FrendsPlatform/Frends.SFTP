using System.Collections.Concurrent;
using Serilog;

namespace Frends.SFTP.UploadFiles.Definitions;

internal class SFTPLogger : ISFTPLogger
{
    private readonly string debuglogFileName;
    private ConcurrentBag<FileTransferInfo> fileTransfers;
    private ILogger log;
    private bool disposed;

    public SFTPLogger(ILogger log)
    {
        fileTransfers = new ConcurrentBag<FileTransferInfo>();
        this.log = log;
        debuglogFileName = $"{DateTime.Now:yyyy_MM_ddZhhmmss}_debuglog.txt";
    }

    ~SFTPLogger()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the logger's resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Records an error and its associated exception.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="msg">Error message to log.</param>
    /// <param name="e">Exception associated with the error.</param>
    public void NotifyError(BatchContext context, string msg, Exception e)
    {
        try
        {
            NotifyInformation(context, e.ToString());
            if (context == null) context = new BatchContext();

            var sourceEndPointName = GetEndPointName(context, EndPoint.Source, "unknown source end point");
            var destinationEndPointName = GetEndPointName(context, EndPoint.Destination, "unknown destination end point");
            var transferName = context.Info == null ? "unknown" : context.Info.TransferName;
            var transferNameForLog = transferName ?? string.Empty;

            var errorMessage = $"\r\n\r\nFRENDS SFTP file transfer '{transferNameForLog}' from '{sourceEndPointName}' to '{destinationEndPointName}': \r\n{msg}\r\n";
            log.Error(errorMessage, e);
        }
        catch (Exception ex)
        {
            log.Error("Error when logging error message: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Records an informational message.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="msg">Message to log.</param>
    public void NotifyInformation(BatchContext context, string msg)
    {
        try
        {
            log.Information(msg);
        }
        catch (Exception ex)
        {
            log.Error("Error when logging information message: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Records a successful file transfer.
    /// </summary>
    /// <param name="transfer">Details of the completed file transfer.</param>
    /// <param name="context">Context for the current batch.</param>
    public void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context)
    {
        try
        {
            var fileTransferInfoForSuccess = CreateFileTransferInfo(TransferResult.Success, transfer, context);
            fileTransfers.Add(fileTransferInfoForSuccess);
            log.Information("File transfer succeeded: " + transfer.SourceFile.Name);
        }
        catch (Exception ex)
        {
            log.Error("Error when logging success message: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Records a failed file transfer.
    /// </summary>
    /// <param name="transfer">Details of the failed file transfer.</param>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="errorMessage">Description of the transfer failure.</param>
    /// <param name="exception">Exception associated with the failure.</param>
    public void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception)
    {
        try
        {
            var fileTransferInfoForFailure = CreateFileTransferInfo(TransferResult.Fail, transfer, context, errorMessage);
            fileTransfers.Add(fileTransferInfoForFailure);

            log.Error("File transfer failed: " + fileTransferInfoForFailure.ErrorInfo, exception);
        }
        catch (Exception ex)
        {
            log.Error("Error when logging failure: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Records the outcome of the complete batch.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="userResultMessage">Summary shown to the task user.</param>
    /// <param name="success">Whether all requested transfers succeeded.</param>
    /// <param name="actionSkipped">Whether the configured action was skipped.</param>
    public void LogBatchFinished(BatchContext context, string userResultMessage, bool success, bool actionSkipped)
    {
        try
        {
        }
        catch (Exception ex)
        {
            log.Error("Error when logging batch finished: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Records a diagnostic trace message.
    /// </summary>
    /// <param name="message">Trace message to log.</param>
    public void NotifyTrace(string message)
    {
        // only log to debug trace
        log.Debug(message);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        fileTransfers = null;
        log = null;

        disposed = true;
    }

    private string GetEndPointName(BatchContext context, EndPoint endpoint, string defaultValue)
    {
        dynamic endpointConfig = (endpoint == EndPoint.Source) ? context.Source : context.Destination;
        if (endpointConfig == null || context.Connection.Address == null) return defaultValue;

        if (endpoint == EndPoint.Destination)
            return $"SFTP://{context.Connection.Address}/{context.Destination.Directory}";

        var directory = endpointConfig.Directory;

        return $"FILE://{directory}/{endpointConfig.FileName}";
    }

    /// <summary>
    /// Creates a transfer information record for logging.
    /// </summary>
    /// <param name="result">Outcome of the file transfer.</param>
    /// <param name="transfer">Details of the file transfer, when available.</param>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="errorMessage">Description of the error, if the transfer failed.</param>
    /// <returns>Information about the file transfer.</returns>
    private FileTransferInfo CreateFileTransferInfo(TransferResult result, SingleFileTransfer transfer, BatchContext context, string errorMessage = null)
    {
        // Create 2 dummy endpoints and initialize some local variables which are needed in case if cobalt config is not
        // succesfully initialized, i.e. when there has been a failure creating the config (invalid xml etc..) and config elements are left null
        var sourceFile = string.Empty;
        var destinationFile = string.Empty;
        var localFileName = string.Empty;
        var singleFileTransferId = Guid.NewGuid();

        if (transfer != null)
        {
            sourceFile = transfer.SourceFile.Name;
            destinationFile = Path.GetFileName(transfer.DestinationFileWithMacrosExpanded);
            localFileName = context.Info.WorkDir;
            singleFileTransferId = Guid.NewGuid();
        }

        var transferStarted = DateTime.UtcNow;
        var batchId = Guid.Empty;
        var serviceId = string.Empty;

        var routineUri = string.Empty;

        var transferName = string.Empty;

        if (context != null)
        {
            transferStarted = context.BatchTransferStartTime;
            batchId = context.InstanceId;
            serviceId = context.ServiceId;

            routineUri = context.RoutineUri;
            transferName = context.Info != null ? context.Info.TransferName : string.Empty;
        }

        return new FileTransferInfo
        {
            Result = result,
            SourceFile = sourceFile ?? string.Empty,
            DestinationFile = destinationFile ?? string.Empty,
            FileSize = GetFileSize(localFileName),
            TransferStarted = transferStarted,
            TransferEnded = DateTime.UtcNow,
            BatchId = batchId,
            TransferName = transferName ?? string.Empty,
            ServiceId = serviceId ?? string.Empty,
            RoutineUri = routineUri ?? string.Empty,
            ErrorInfo = errorMessage ?? string.Empty,
            SingleFileTransferId = singleFileTransferId,
        };
    }

    private long GetFileSize(string filepath)
    {
        return File.Exists(filepath) ? new FileInfo(filepath).Length : 0;
    }
}
