namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Content format to return from the task.
/// </summary>
public enum ContentType
{
    /// <summary>
    /// Return the file content as bytes.
    /// </summary>
    Binary = 1,

    /// <summary>
    /// Return the file content as text.
    /// </summary>
    Text = 2,
}
