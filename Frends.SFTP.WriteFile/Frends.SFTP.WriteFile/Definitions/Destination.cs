using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.SFTP.WriteFile.Definitions
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
        public string Directory { get; set; } = "/";

        /// <summary>
        /// Operation to determine what to do if destination file exists.
        /// </summary>
        [DefaultValue(DestinationOperation.Error)]
        public DestinationOperation Operation { get; set; } = DestinationOperation.Error;
    }
}
