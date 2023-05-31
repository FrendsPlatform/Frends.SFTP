namespace Frends.SFTP.RenameFile.Definitions;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    /// <summary>
    /// Path for the renamed file.
    /// </summary>
    /// <example>/test/folder/example.txt</example>
	public string Path { get; private set; }

    internal Result(string path)
    {
        Path = path;
    }
}

