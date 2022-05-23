using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Destination transfer options
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
        /// Operation to determine what to do if destination file exists.
        /// </summary>
        [DefaultValue(DestinationAction.Error)]
        public DestinationAction Action { get; set; }
    }
}
