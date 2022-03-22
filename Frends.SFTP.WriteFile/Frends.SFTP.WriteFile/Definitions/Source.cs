using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.SFTP.WriteFile
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
        /// File name/mask to fetch (for source endpoint) or destination file name with possible macros (for destination endpoint)
        /// </summary>
        [DefaultValue("\"\"")]
        public string FileName { get; set; }
    }
}
