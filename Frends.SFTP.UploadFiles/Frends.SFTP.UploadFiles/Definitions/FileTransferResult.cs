using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// FileTransferResult object with private setters.
    /// </summary>
    public class FileTransferResult
    {
        /// <summary>
        /// Boolean value of the skipped Action.
        /// </summary>
        public bool ActionSkipped { get; set; }

        /// <summary>
        /// Boolean value of the successful transfer.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message of the transfer operations.
        /// </summary>
        public string UserResultMessage { get; set; }

        /// <summary>
        /// Count of files that has been successfully transferred.
        /// </summary>
        public int SuccessfulTransferCount { get; set; }

        /// <summary>
        /// Count of files that have not been transferred.
        /// </summary>
        public int FailedTransferCount { get; set; }

        /// <summary>
        /// List of transferred file names.
        /// </summary>
        public IEnumerable<string> TransferredFileNames { get; set; }

        /// <summary>
        /// Dictionary of file names and errors messages of the failed transfers.
        /// </summary>
        public Dictionary<string, IList<string>> TransferErrors { get; set; }

        /// <summary>
        /// List of transferred file paths.
        /// </summary>
        public IEnumerable<string> TransferredFilePaths { get; set; }

        /// <summary>
        /// Operations logs for the transfer.
        /// </summary>
        public IDictionary<string, string> OperationsLog { get; set; }
    }
}
