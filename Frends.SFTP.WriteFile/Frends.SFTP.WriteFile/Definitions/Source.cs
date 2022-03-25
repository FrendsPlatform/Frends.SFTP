using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.SFTP.WriteFile.Definitions
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
        /// File name to fetch.
        /// </summary>
        [DefaultValue("\"\"")]
        public string FileName { get; set; }
    }
}
