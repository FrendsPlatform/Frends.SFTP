using Renci.SshNet.Sftp;

namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    /// <summary>
    /// Content of the file as a byte array.
    /// </summary>
    /// <example>[1, 2, 3]</example>
    public byte[] BinaryContent { get; private init; } = Array.Empty<byte>();

    /// <summary>
    /// Content of the file in string format.
    /// </summary>
    /// <example>This is a test file</example>
    public string TextContent { get; private init; } = string.Empty;

    /// <summary>
    /// Full name of the file.
    /// </summary>
    /// <example>c:\source\Test.txt</example>
    public string Path { get; private init; }

    /// <summary>
    /// Size of the read file.
    /// </summary>
    /// <example>0</example>
    public double SizeInMegaBytes { get; private init; }

    /// <summary>
    /// Timestamp of when the file was last modified.
    /// </summary>
    /// <example>2022-06-14T12:45:28.6058477+03:00</example>
    public DateTime LastWriteTime { get; private init; }

    internal Result(ISftpFile file, string content)
    {
        TextContent = content;
        Path = file.FullName;
        SizeInMegaBytes = Math.Round((file.Length / 1024d / 1024d), 3);
        LastWriteTime = file.LastWriteTime;
    }

    internal Result(ISftpFile file, byte[] content)
    {
        BinaryContent = content;
        Path = file.FullName;
        SizeInMegaBytes = Math.Round((file.Length / 1024d / 1024d), 3);
        LastWriteTime = file.LastWriteTime;
    }
}
