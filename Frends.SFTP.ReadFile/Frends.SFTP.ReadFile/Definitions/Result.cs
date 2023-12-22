using Renci.SshNet.Sftp;

namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    /// <summary>
    /// Content of the file in string format.
    /// </summary>
    /// <example>This is a test file</example>
    public string Content { get; private set; }

    /// <summary>
    /// Full name of the file.
    /// </summary>
    /// <example>c:\source\Test.txt</example>
	public string Path { get; private set; }

    /// <summary>
    /// Size of the read file.
    /// </summary>
    /// <example>0</example>
    public double SizeInMegaBytes { get; private set; }

    /// <summary>
    /// Timestamp of when the file was last modified.
    /// </summary>
    /// <example>2022-06-14T12:45:28.6058477+03:00</example>
    public DateTime LastWriteTime { get; private set; }

    internal Result(ISftpFile file, string content)
    {
        Content = content;
        Path = file.FullName;
        SizeInMegaBytes = Math.Round((file.Length / 1024d / 1024d), 3);
        LastWriteTime = file.LastWriteTime;
    }
}

