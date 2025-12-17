using Renci.SshNet.Sftp;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.WriteFile.Definitions;

/// <summary>
/// Return object for verification result
/// </summary>
public class Result
{
    /// <summary>
    /// Full path to the written file.
    /// </summary>
    /// <example>/upload/test.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string RemotePath { get; }
        /// <summary>
    /// Size of the new file in destination.
    /// </summary>
    /// <example>3.2</example>
    public double SizeInMegaBytes { get; }


    /// <summary>
    /// Indicates whether the file verification was made.
    /// </summary>
    public bool Verified { get; }

    /// <summary>
    /// Constructor for skipped verification.
    /// </summary>
    public Result(string remotePath, long fileSize)
    {
        RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
        SizeInMegaBytes = Math.Round((fileSize / 1024d / 1024d), 3);
        Verified = false;
    }

    /// <summary>
    /// Constructor for successful verification.
    /// </summary>
    public Result(ISftpFile sftpFile)
    {
        RemotePath = sftpFile.FullName;
        SizeInMegaBytes = Math.Round((sftpFile.Length / 1024d / 1024d), 3);
        Verified = true;
    }

}


