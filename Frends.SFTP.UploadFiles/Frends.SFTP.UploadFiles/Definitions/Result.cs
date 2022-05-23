using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Return object with private setters.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Boolean value of the skipped Action.
        /// </summary>
        public bool ActionSkipped { get; private set; }

        /// <summary>
        /// Boolean value of the successful transfer.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Message of the transfer operations.
        /// </summary>
        public string UserResultMessage { get; private set; }

        /// <summary>
        /// Count of files that has been successfully transferred.
        /// </summary>
        public int SuccessfulTransferCount { get; private set; }

        /// <summary>
        /// Count of files that have not been transferred.
        /// </summary>
        public int FailedTransferCount { get; private set; }

        /// <summary>
        /// List of transferred file names.
        /// </summary>
        public IEnumerable<string> TransferredFileNames { get; private set; }

        /// <summary>
        /// Dictionary of file names and errors messages of the failed transfers.
        /// </summary>
        public Dictionary<string, IList<string>> TransferErrors { get; private set; }

        /// <summary>
        /// List of transferred file paths.
        /// </summary>
        public IEnumerable<string> TransferredFilePaths { get; private set; }

        /// <summary>
        /// Operations logs for the transfer.
        /// </summary>
        public IDictionary<string, string> OperationsLog { get; set; }

        internal Result(Dictionary<string, IList<string>> transferErrors)
        {
            TransferErrors = transferErrors;
        }

        internal Result(FileTransferResult result)
        {
            ActionSkipped = result.ActionSkipped;
            Success = result.Success;
            UserResultMessage = result.UserResultMessage;
            SuccessfulTransferCount = result.SuccessfulTransferCount;
            FailedTransferCount = result.FailedTransferCount;
            TransferredFileNames = result.TransferredFileNames;
            TransferErrors = result.TransferErrors;
            TransferredFilePaths = result.TransferredFilePaths;
            OperationsLog = result.OperationsLog;
        }

        internal Result(
            bool actionSkipped, 
            bool success, 
            string userResultMessage, 
            int successfulTransferCount, 
            int failedTransferCount, 
            IEnumerable<string> transferredFileNames, 
            Dictionary<string, IList<string>> transferErrors, 
            IEnumerable<string> transferredFilePaths, 
            IDictionary<string, string> operationsLog)
        {
            ActionSkipped = actionSkipped;
            Success = success;
            UserResultMessage = userResultMessage;
            SuccessfulTransferCount = successfulTransferCount;
            FailedTransferCount = failedTransferCount;
            TransferredFileNames = transferredFileNames;
            TransferErrors = transferErrors;
            TransferredFilePaths = transferredFilePaths;
            OperationsLog = operationsLog;
        }
    }
}
