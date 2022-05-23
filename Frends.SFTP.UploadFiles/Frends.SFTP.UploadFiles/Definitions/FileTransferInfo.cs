using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// File transfer info class.
    /// </summary>
    public class FileTransferInfo
    {
        /// <summary>
        /// Name of the transfer.
        /// </summary>
        public string TransferName { get; set; } // 0

        /// <summary>
        /// Batch id.
        /// </summary>
        public Guid BatchId { get; set; } // 1

        /// <summary>
        /// Source file name.
        /// </summary>
        public string SourceFile { get; set; } // 2

        /// <summary>
        /// Start time of the transfer.
        /// </summary>
        public DateTime TransferStarted { get; set; } // 3

        /// <summary>
        /// End time of the transfer.
        /// </summary>
        public DateTime TransferEnded { get; set; } // 4

        /// <summary>
        /// Destination file name.
        /// </summary>
        public string DestinationFile { get; set; } // 5

        /// <summary>
        /// Size of the file.
        /// </summary>
        public long FileSize { get; set; } // 6

        /// <summary>
        /// Error info.
        /// </summary>
        public string ErrorInfo { get; set; } // 7

        /// <summary>
        /// Result of the transfer.
        /// </summary>
        public TransferResult Result { get; set; } // 10

        /// <summary>
        /// Destination address.
        /// </summary>
        public string DestinationAddress { get; set; } // 13

        /// <summary>
        /// Source file path.
        /// </summary>
        public string SourcePath { get; set; } // 15

        /// <summary>
        /// Destination file path.
        /// </summary>
        public string DestinationPath { get; set; } // 16

        /// <summary>
        /// Service id of the transfer.
        /// </summary>
        public string ServiceId { get; set; } // 17

        /// <summary>
        /// Routine Uri.
        /// </summary>
        public string RoutineUri { get; set; } // 18

        /// <summary>
        /// Transfer id of a single file transfer.
        /// </summary>
        public Guid SingleFileTransferId { get; set; }

        /// <summary>
        /// Overrided ToString method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
            $@"{ErrorInfo}

            TransferName: {TransferName}
            BatchId: {BatchId}
            Sourcefile: {SourceFile}
            DestinationFile: {DestinationFile}
            SourcePath: {SourcePath}
            DestinationPath: {DestinationPath}
            DestinationAddress: {DestinationAddress}
            TransferStarted: {TransferStarted}
            TransferEnded: {TransferEnded}
            TransferResult: {Result}
            FileSize: {FileSize} bytes
            ServiceId: {string.Empty}
            RoutineUri: {ServiceId}");
        }
    }
}
