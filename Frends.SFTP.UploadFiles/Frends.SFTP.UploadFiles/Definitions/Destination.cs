using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Destination transfer options.
    /// </summary>
    public class Destination
    {
        /// <summary>
        /// Directory on the server.
        /// </summary>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; }

        /// <summary>
        /// File name of the destination file with possible macros.
        /// </summary>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FileName { get; set; }

        /// <summary>
        /// If set, this ecoding will be used to encode and decode command 
        /// parameters and server responses, such as file names. 
        /// By selecting 'Other' you can use any encoding.
        /// </summary>
        [DefaultValue(FileEncoding.ANSI)]
        public FileEncoding FileNameEncoding { get; set; }

        /// <summary>
        /// Additional option for UTF-8 encoding to enable bom.
        /// </summary>
        [UIHint(nameof(FileNameEncoding), "", FileEncoding.UTF8)]
        public bool EnableBomForFileName { get; set; }

        /// <summary>
        /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
        /// </summary>
        [UIHint(nameof(FileNameEncoding), "", FileEncoding.Other)]
        public string FileNameEncodingInString { get; set; }

        /// <summary>
        /// Operation to determine what to do if destination file exists.
        /// </summary>
        [DefaultValue(DestinationAction.Error)]
        public DestinationAction Action { get; set; }

        /// <summary>
        /// Encoding for the appending content. By selecting 'Other' you can use any encoding.
        /// </summary>
        [DefaultValue(FileEncoding.ANSI)]
        [UIHint(nameof(Action), "", DestinationAction.Append)]
        public FileEncoding FileContentEncoding { get; set; }

        /// <summary>
        /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
        /// </summary>
        [UIHint(nameof(FileContentEncoding), "", FileEncoding.Other)]
        public string FileContentEncodingInString { get; set; }

        /// <summary>
        /// Additional option for UTF-8 encoding to enable bom.
        /// </summary>
        [UIHint(nameof(FileContentEncoding), "", FileEncoding.UTF8)]
        public bool EnableBomForContent { get; set; }
    }
}
