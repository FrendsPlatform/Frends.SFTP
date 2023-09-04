namespace Frends.SFTP.DeleteFiles.Definitions;

using System.Collections.Generic;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    internal Result(List<FileItem> files)
    {
        Files = files;
    }

    /// <summary>
    /// List of file items deleted from directory.
    /// </summary>
    /// <example>[test.txt, test2.txt]</example>
    public List<FileItem> Files { get; set; }
}