using System.ComponentModel.DataAnnotations;
using Renci.SshNet.Sftp;

namespace Frends.SFTP.WriteFile.Definitions;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    internal Result(ISftpFile file = null)
    {
        if (file is null)
            return;

        Path = file.FullName;
        SizeInMegaBytes = Math.Round(file.Length / 1024d / 1024d, 3);
        Success = true;
        Error = null;
    }

    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Full path to the written file.
    /// </summary>
    /// <example>/destination/newfile.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; private set; }

    /// <summary>
    /// Size of the new file in destination.
    /// </summary>
    /// <example>3.2</example>
    public double SizeInMegaBytes { get; private set; }

    /// <summary>
    /// Error that occurred during the task.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; set; }
}
