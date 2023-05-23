namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    /// <summary>
    /// List of FileItem objects with source path and target path attributes.
    /// </summary>
    /// <example>[object { SourcePath: /source/test1.txt, TargetPath: /destination/test1.txt }, object { SourcePath: /source/test2.txt, TargetPath: /destination/test2.txt } ]</example>
    public List<FileItem> Files { get; private set; }

    /// <summary>
    /// Message consisting information on the transfer.
    /// </summary>
    public string Message { get; private set; }

    internal Result(List<FileItem> files, string message)
    {
        Files = files;
        Message = message;
    }
}

