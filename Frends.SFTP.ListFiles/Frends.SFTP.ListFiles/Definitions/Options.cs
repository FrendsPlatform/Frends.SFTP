using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.ListFiles.Definitions
{
    /// <summary>
    /// Source transfer options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Directory on the server.
        /// </summary>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; } = "/";

        /// <summary>
        /// Pattern to match.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string FileMask { get; set; }

        /// <summary>
        /// Types to include in the directory listing.
        /// </summary>
        [DefaultValue(IncludeType.File)]
        public IncludeType IncludeType { get; set; } = IncludeType.File;

        /// <summary>
        /// Include subdirectories?
        /// </summary>
        public bool IncludeSubdirectories { get; set; }
    }
}
