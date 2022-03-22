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
	    public string Name { get; private set; }

        /// <summary>
        /// The full source path of the file.
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// The full destination path of the file.
        /// </summary>
        public string DestinationPath { get; private set; }

        /// <summary>
        /// Boolean value of the successful transfer.
        /// </summary>
        public bool Success { get; private set; }

        public Result()
        {

        }

        public Result(string name, string sourcePath, string destinationPath, bool success)
        {
            Name = name;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            Success = success;
        }
    }
}
