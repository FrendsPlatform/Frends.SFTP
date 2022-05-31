using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Result object of one individual file transfer.
    /// </summary>
    internal class SingleFileTransferResult
    {
        /// <summary>
        /// Boolean value of successful tranfer state.
        /// </summary>
        internal bool Success { get; set; }

        /// <summary>
        /// Boolean value indicating if the transfer action was skipped.
        /// </summary>
        internal bool ActionSkipped { get; set; }

        /// <summary>
        /// List of error messages that happened during transfer or cleanup.
        /// </summary>
        internal IList<string> ErrorMessages { get; set; }

        /// <summary>
        /// Name of the transferred file.
        /// </summary>
        internal string TransferredFile { get; set; }

        /// <summary>
        /// Full path of the transferred file.
        /// </summary>
        internal string TransferredFilePath { get; set; }

        /// <summary>
        /// Constructor for the class in case of error.
        /// </summary>
        internal SingleFileTransferResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}
