using System.Collections.Concurrent;
using Serilog;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// SFTP internal logger interface
    /// </summary>
    public interface ISFTPLogger : IDisposable
    {
        /// <summary>
        /// Notifies of errors
        /// </summary>
        void NotifyError(BatchContext context, string msg, Exception e);

        /// <summary>
        /// Notifies of info-level messages
        /// </summary>
        void NotifyInformation(BatchContext context, string msg);

        /// <summary>
        /// Notifies of debug trace messages
        /// </summary>
        void NotifyTrace(string message);

        /// <summary>
        /// Logs a single successful file transfer
        /// </summary>
        void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context);

        /// <summary>
        /// Logs a single failed file transfer
        /// </summary>
        void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception);

        /// <summary>
        /// Logs a batch finished event
        /// </summary>
        void LogBatchFinished(BatchContext context, string userResultMessage, bool success, bool actionSkipped);
    }

    /// <summary>
    /// SFTP internal logger implementation
    /// </summary>
    public class SFTPLogger : ISFTPLogger
    {
        private ConcurrentBag<FileTransferInfo> _fileTransfers;
        private ILogger _log;

        private bool _disposed;

        /// <summary>
        /// Constructor
        /// </summary>
        public SFTPLogger(ILogger log)
        {
            _fileTransfers = new ConcurrentBag<FileTransferInfo>();
            _log = log;
        }

        /// <summary>
        /// Construtor disposes the SFTPLogger
        /// </summary>
        ~SFTPLogger()
        {
            Dispose(false);
        }

        /// <summary>
        /// Notifies Error
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public void NotifyError(BatchContext context, string msg, Exception e)
        {
            try
            {
                if (context == null)
                {
                    context = new BatchContext();
                }

                var sourceEndPointName = GetEndPointName(context, EndPoint.Source ,"unknown source end point");
                var destinationEndPointName = GetEndPointName(context, EndPoint.Destination, "unknown destination end point");
                var transferName = context.Info == null ? "unknown" : context.Info.TransferName;
                var transferNameForLog = transferName ?? string.Empty;

                var errorMessage = string.Format("\r\n\r\nFRENDS SFTP file transfer '{0}' from '{1}' to '{2}': \r\n{3}\r\n", transferNameForLog, sourceEndPointName, destinationEndPointName, msg);
                _log.Error(errorMessage, e);
            }
            catch (Exception ex)
            {
                _log.Error("Error when logging error message: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Notifies information
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
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

        /// <summary>
        /// Logs succesful tranfer
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="context"></param>
        public void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context)
        {
            try
            {
                var fileTransferInfoForSuccess = CreateFileTransferInfo(TransferResult.Success, transfer, context);
                _fileTransfers.Add(fileTransferInfoForSuccess);
                _log.Information("File transfer succeeded: " + transfer.SourceFile);
            }
            catch (Exception ex)
            {
                _log.Error("Error when logging success message: " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Logs failed transfer.
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        /// <param name="exception"></param>
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

        /// <summary>
        /// Derived method from ILogger
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userResultMessage"></param>
        /// <param name="success"></param>
        /// <param name="actionSkipped"></param>
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

        /// <summary>
        /// Notifies Trace
        /// </summary>
        /// <param name="message"></param>
        public void NotifyTrace(string message)
        {
            // only log to debug trace
            _log.Debug(message);
        }

        private string GetEndPointName(BatchContext context, EndPoint endpoint, string defaultValue)
        {
            dynamic endpointConfig = (endpoint == EndPoint.Source) ? context.Source : context.Destination;
            if (endpointConfig == null || context.Connection.Address == null)
            {
                return defaultValue;
            }

            var directory = endpointConfig.Directory;

            return string.Format("{0}://{1}/{2}/{3}", "SFTP", context.Connection.Address, directory, endpointConfig.FileName);
        }

        /// <summary>
        /// Creates FileTransferInfo
        /// </summary>
        /// <param name="result"></param>
        /// <param name="transfer"></param>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static FileTransferInfo CreateFileTransferInfo(TransferResult result, SingleFileTransfer transfer, BatchContext context, string errorMessage = null)
        {
            // Create 2 dummy endpoints and initialize some local variables which are needed in case if cobalt config is not
            // succesfully initialized, i.e. when there has been a failure creating the config (invalid xml etc..) and config elements are left null
            var sourceFile = string.Empty;
            var destinationFile = string.Empty;
            var localFileName = string.Empty;
            IDictionary<string, object> customData = new Dictionary<string, object>();
            var singleFileTransferId = Guid.NewGuid();

            if (transfer != null)
            {
                sourceFile = transfer.SourceFile.Name;
                destinationFile = transfer.DestinationFileNameWithMacrosExpanded;
                localFileName = context.Info.WorkDir;
                //customData = transfer.ExtendedTransferContext.CustomData;
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
                SingleFileTransferId = singleFileTransferId
            };
        }

        private static long GetFileSize(string filepath)
        {
            return File.Exists(filepath) ? new FileInfo(filepath).Length : 0;
        }

        /// <summary>
        /// Method starts dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual method dispose for SFTPLogger
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _fileTransfers = null;
            _log = null;

            _disposed = true;
        }
    }
}
