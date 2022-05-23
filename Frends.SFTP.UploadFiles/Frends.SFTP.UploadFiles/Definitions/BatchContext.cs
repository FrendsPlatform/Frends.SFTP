using System;
using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// BatchContext class for creating collection of Task parameters
    /// </summary>
    public class BatchContext
    {
        /// <summary>
        /// Transfer info parameters.
        /// </summary>
        public Info Info { get; set; }

        /// <summary>
        /// Temporary work directory.
        /// </summary>
        public string TempWorkDir { get; set; }

        /// <summary>
        /// Task transfer options.
        /// </summary>
        public Options Options { get; set; }

        /// <summary>
        /// InstanceId of the task instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Collection of source files.
        /// </summary>
        public IEnumerable<FileItem> SourceFiles { get; set; }

        /// <summary>
        /// Collection of destination files.
        /// </summary>
        public IEnumerable<SftpFile> DestinationFiles { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RoutineUri { get; set; }

        /// <summary>
        /// Start time of the transfer.
        /// </summary>
        public DateTime BatchTransferStartTime { get; set; }

        /// <summary>
        /// Transfer source parameters.
        /// </summary>
        public Source Source { get; set; }

        /// <summary>
        /// Transfer destination parameters.
        /// </summary>
        public Destination Destination { get; set; }

        /// <summary>
        /// Transfer connection parameters.
        /// </summary>
        public Connection Connection  { get; set; }
    }
}
