namespace Frends.SFTP.DownloadFiles.Definitions;

using System.Collections.Concurrent;
using Serilog;

/// <summary>
/// SFTP internal logger interface
/// </summary>
internal interface ISFTPLogger : IDisposable
{
    void NotifyError(BatchContext context, string msg, Exception e);

    void NotifyInformation(BatchContext context, string msg);

    void NotifyTrace(string message);

    void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context);

    void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception);

    void LogBatchFinished(BatchContext context, string userResultMessage, bool success, bool actionSkipped);
}

internal class SFTPLogger : ISFTPLogger
{
    private ConcurrentBag<FileTransferInfo> _fileTransfers;
    private ILogger _log;

    private bool _disposed;

    public SFTPLogger(ILogger log)
    {
        _fileTransfers = new ConcurrentBag<FileTransferInfo>();
        _log = log;
    }

    public static FileTransferInfo CreateFileTransferInfo(TransferResult result, SingleFileTransfer transfer, BatchContext context, string errorMessage = null)
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
            _log.Error(errorMessage, e);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging error message: " + ex.Message, ex);
        }
    }

    public void NotifyInformation(BatchContext context, string msg)
    {
        try
        {
            _log.Information(msg);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging information message: " + ex.Message, ex);
        }
    }

    public void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context)
    {
        try
        {
            var fileTransferInfoForSuccess = CreateFileTransferInfo(TransferResult.Success, transfer, context);
            _fileTransfers.Add(fileTransferInfoForSuccess);
            _log.Information("File transfer succeeded: " + transfer.SourceFile.Name);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging success message: " + ex.Message, ex);
        }
    }

    public void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception)
    {
        try
        {
            var fileTransferInfoForFailure = CreateFileTransferInfo(TransferResult.Fail, transfer, context, errorMessage);
            _fileTransfers.Add(fileTransferInfoForFailure);

            _log.Error("File transfer failed: " + fileTransferInfoForFailure.ErrorInfo, exception);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging failure: " + ex.Message, ex);
        }
    }

    public void LogBatchFinished(BatchContext context, string userResultMessage, bool success, bool actionSkipped)
    {
        try
        {
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging batch finished: " + ex.Message, ex);
        }
    }

    public void NotifyTrace(string message)
    {
        // only log to debug trace
        _log.Debug(message);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        _fileTransfers = null;
        _log = null;

        _disposed = true;
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