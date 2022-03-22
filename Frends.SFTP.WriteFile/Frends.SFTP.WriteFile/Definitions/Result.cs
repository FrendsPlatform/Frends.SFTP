using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.SFTP.WriteFile
{
    /// <summary>
    /// Return object with private setters
    /// </summary>
    public class Result
    {
        /// <summary>
        /// The name of the file. Does not include the path.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
	    public string FileName { get; private set; }

        /// <summary>
        /// The full source path of the file.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string SourcePath { get; private set; }

        /// <summary>
        /// Boolean value of the successful transfer.
        /// </summary>
        public bool Success { get; private set; }

        public Result()
        {

        }

        public Result(string name, string sourcePath, bool success)
        {
            FileName = name;
            SourcePath = sourcePath;
            Success = success;
        }
    }
}
