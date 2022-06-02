using System.ComponentModel;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Options for file transfer
    /// </summary>
    public class Options
    {   
        /// <summary>
        /// Should an exception be thrown when file transfer fails.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(true)]
        public bool ThrowErrorOnFail { get; set; }

        /// <summary>
        /// Should the destination file be renamed with temporary file name during file transfer as a locking mechanism.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(true)]
        public bool RenameSourceFileBeforeTransfer { get; set; }

        /// <summary>
        /// Should the destination file be renamed with temporary file name during file transfer as a locking mechanism.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(true)]
        public bool RenameDestinationFileDuringTransfer { get; set; }

        /// <summary>
        /// Should the destination directories be created if they do not exist. May not work on all servers. 
        /// </summary>
        /// <example>true</example>
        [DefaultValue(false)]
        public bool CreateDestinationDirectories { get; set; }

        /// <summary>
        /// Should the Last Modified timestamp be preserved from the source.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(false)]
        public bool PreserveLastModified { get; set; }

        /// <summary>
        /// While enabled all operations logs of executions will be returned with the result.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(true)]
        public bool OperationLog { get; set; }
    }
}
