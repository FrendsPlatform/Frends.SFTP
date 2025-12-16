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
    [DisplayFormat(DataFormatString = "Text")]
    public string RemotePath { get; }
        /// <summary>
    /// Size of the new file in destination.
    /// </summary>
    /// <example>3.2</example>
    public double SizeInMegaBytes { get; }


    /// <summary>
    /// Indicates whether the file verification was successful.
    /// </summary>
    public bool Verified { get; }

    /// <summary>
    /// Constructor for failed verification.
    /// </summary>
    public Result(string remotePath)
    {
        RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
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


