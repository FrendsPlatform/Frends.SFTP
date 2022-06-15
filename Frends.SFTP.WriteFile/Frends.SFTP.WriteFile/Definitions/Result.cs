using Renci.SshNet.Sftp;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.WriteFile.Definitions;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
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

    internal Result(SftpFile file)
    {
        Path = file.FullName;
        SizeInMegaBytes = Math.Round((file.Length / 1024d / 1024d), 3);
    }
}

