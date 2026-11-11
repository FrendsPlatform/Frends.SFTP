using System.Collections.Concurrent;
using Frends.SFTP.DownloadFiles.Definitions;
using Serilog;

namespace Frends.SFTP.DownloadFiles.Helpers;

/// <summary>
/// SFTP internal logger interface
/// </summary>
internal interface ISFTPLogger : IDisposable
{
    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="msg">Error message to log.</param>
    /// <param name="e">Exception that caused the error.</param>
    void NotifyError(BatchContext context, string msg, Exception e);

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="msg">Information message to log.</param>
    void NotifyInformation(BatchContext context, string msg);

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">Trace message to log.</param>
    void NotifyTrace(string message);

    /// <summary>
    /// Logs a successful file transfer.
    /// </summary>
    /// <param name="transfer">Transfer that succeeded.</param>
    /// <param name="context">Context of the batch transfer.</param>
    void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context);

    /// <summary>
    /// Logs a failed file transfer.
    /// </summary>
    /// <param name="transfer">Transfer that failed.</param>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="errorMessage">Description of the failure.</param>
    /// <param name="exception">Exception that caused the failure.</param>
    void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception);

    /// <summary>
    /// Logs the end of the batch transfer.
    /// </summary>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="userResultMessage">Result message of the transfer.</param>
    /// <param name="success">Whether the batch transfer succeeded.</param>
    /// <param name="actionSkipped">Whether the action was skipped.</param>
    void LogBatchFinished(BatchContext context, string userResultMessage, bool success, bool actionSkipped);
}

/// <summary>
/// Logger that notifies about the progress of the SFTP transfer.
/// </summary>
internal class SFTPLogger : ISFTPLogger
{
    private ConcurrentBag<FileTransferInfo> fileTransfers;
    private ILogger log;

    private bool disposed;

    internal SFTPLogger(ILogger log)
    {
        fileTransfers = new ConcurrentBag<FileTransferInfo>();
        this.log = log;
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="msg">Error message to log.</param>
    /// <param name="e">Exception that caused the error.</param>
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
    /// Logs an information message.
    /// </summary>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="msg">Information message to log.</param>
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
    /// Logs a successful file transfer.
    /// </summary>
    /// <param name="transfer">Transfer that succeeded.</param>
    /// <param name="context">Context of the batch transfer.</param>
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
    /// Logs a failed file transfer.
    /// </summary>
    /// <param name="transfer">Transfer that failed.</param>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="errorMessage">Description of the failure.</param>
    /// <param name="exception">Exception that caused the failure.</param>
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
    /// Logs the end of the batch transfer.
    /// </summary>
    /// <param name="context">Context of the batch transfer.</param>
    /// <param name="userResultMessage">Result message of the transfer.</param>
    /// <param name="success">Whether the batch transfer succeeded.</param>
    /// <param name="actionSkipped">Whether the action was skipped.</param>
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
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">Trace message to log.</param>
    public void NotifyTrace(string message)
    {
        // only log to debug trace
        log.Debug(message);
    }

    /// <summary>
    /// Releases the resources used by the logger.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal static FileTransferInfo CreateFileTransferInfo(TransferResult result, SingleFileTransfer transfer, BatchContext context, string errorMessage = null)
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
            destinationFile = transfer.DestinationFileWithMacrosExpanded;
            if (context != null)
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

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        fileTransfers = null;
        log = null;

        disposed = true;
    }

    private static string GetEndPointName(BatchContext context, EndPoint endpoint, string defaultValue)
    {
        dynamic endpointConfig = (endpoint == EndPoint.Source) ? context.Source : context.Destination;
        if (endpointConfig == null || context.Connection.Address == null) return defaultValue;

        if (endpoint == EndPoint.Destination)
            return $"FILE://{context.Destination.Directory}";

        var directory = endpointConfig.Directory;

        return $"SFTP://{context.Connection.Address}/{directory}/{endpointConfig.FileName}";
    }

    private static long GetFileSize(string filepath)
    {
        return File.Exists(filepath) ? new FileInfo(filepath).Length : 0;
    }
}
