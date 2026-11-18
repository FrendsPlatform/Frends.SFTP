namespace Frends.SFTP.RenameFile.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    internal Result(string path, bool success = true, Error error = null)
    {
        Path = path;
        Success = success;
        Error = error;
    }

    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Path for the renamed file.
    /// </summary>
    /// <example>/test/folder/example.txt</example>
    public string Path { get; private set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; private set; }
}
