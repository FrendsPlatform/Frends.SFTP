namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; init; }

    /// <summary>
    /// Content of the file as a byte array.
    /// </summary>
    /// <example>[1, 2, 3]</example>
    public byte[] BinaryContent { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Content of the file in string format.
    /// </summary>
    /// <example>This is a test file</example>
    public string TextContent { get; init; } = string.Empty;

    /// <summary>
    /// Full name of the file.
    /// </summary>
    /// <example>/source/Test.txt</example>
    public string Path { get; init; }

    /// <summary>
    /// Size of the read file.
    /// </summary>
    /// <example>1.25</example>
    public double SizeInMegaBytes { get; init; }

    /// <summary>
    /// Timestamp of when the file was last modified.
    /// </summary>
    /// <example>2022-06-14T12:45:28.6058477+03:00</example>
    public DateTime LastWriteTime { get; init; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; init; }
}
