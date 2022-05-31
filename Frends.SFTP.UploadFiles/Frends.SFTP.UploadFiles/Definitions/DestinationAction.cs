namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Enumeration to specify actions if destination file exists.
    /// </summary>
    public enum DestinationAction
    {
        /// <summary>
        /// Appends the file content to destination file if it exists.
        /// </summary>
        Append,
        /// <summary>
        /// Overwrites destination file if it exists.
        /// </summary>
        Overwrite,
        /// <summary>
        /// Throws exception if destination file exists.
        /// </summary>
        Error
    }
}
