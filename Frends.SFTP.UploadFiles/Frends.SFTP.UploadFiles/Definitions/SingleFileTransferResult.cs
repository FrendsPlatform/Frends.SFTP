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
    public class SingleFileTransferResult
    {
        /// <summary>
        /// Boolean value of successful tranfer state.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Boolean value indicating if the transfer action was skipped.
        /// </summary>
        public bool ActionSkipped { get; set; }

        /// <summary>
        /// List of error messages that happened during transfer or cleanup.
        /// </summary>
        public IList<string> ErrorMessages { get; set; }

        /// <summary>
        /// Name of the transferred file.
        /// </summary>
        public string TransferredFile { get; set; }

        /// <summary>
        /// Full path of the transferred file.
        /// </summary>
        public string TransferredFilePath { get; set; }

        /// <summary>
        /// Constructor for the class in case of error.
        /// </summary>
        public SingleFileTransferResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}
