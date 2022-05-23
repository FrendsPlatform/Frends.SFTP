using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Source transfer options
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Directory on the server.
        /// </summary>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; } = "/";

        /// <summary>
        /// File name or file mask of the files to be fetched.
        /// </summary>
        [DefaultValue("\"\"")]
        public string FileName { get; set; }

        /// <summary>
        /// What to do if source file is not found. Error = alarm and fail,
        /// Info = alarm info and quit with success status, Ignore = quit
        /// with success status.
        /// </summary>
        [DefaultValue(SourceAction.Error)]
        public SourceAction Action { get; set; }

        /// <summary>
        /// What to do with the source file after transfer.
        /// </summary>
        [DefaultValue(SourceOperation.Delete)]
        public SourceOperation Operation { get; set; }

        /// <summary>
        /// Parameter for Rename operation. Set the file name for the source file.
        /// </summary>
        [UIHint(nameof(Operation), "", SourceOperation.Rename)]
        public string FileNameAfterTransfer { get; set; }

        /// <summary>
        /// Parameter for Move operation. Set the full file path for source file.
        /// </summary>
        [UIHint(nameof(Operation), "", SourceOperation.Move)]
        public string DirectoryToMoveAfterTransfer { get; set; }

        /// <summary>
        /// The paths to the files to transfer, mainly meant to be used with the file trigger with the syntax: #trigger.data.filePaths
        /// </summary>
        [DefaultValue("")] // set to empty string so 4.2 shows the field as empty by default
        public object FilePaths { get; set; }
    }
}
