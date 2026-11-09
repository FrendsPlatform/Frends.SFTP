namespace Frends.SFTP.UploadFiles.Definitions;

internal class WorkFileInfo
{
    public WorkFileInfo(string originalFileName, string workFileName, string workFileDir)
    {
        OriginalFileName = originalFileName ?? string.Empty;
        WorkFileName = workFileName ?? string.Empty;
        WorkFileDir = workFileDir ?? string.Empty;
    }

    /// <summary>
    /// Gets the name of the original source file.
    /// </summary>
    /// <example>invoice-2026-11.csv.</example>
    public string OriginalFileName { get; }

    /// <summary>
    /// Gets the name of the temporary working file.
    /// </summary>
    /// <example>frends_637896781104694806az33q4kf.8CO.</example>
    public string WorkFileName { get; }

    /// <summary>
    /// Gets the directory containing the temporary working file.
    /// </summary>
    /// <example>/tmp/sftp-work.</example>
    public string WorkFileDir { get; }

    /// <summary>
    /// Gets full path of the temporary working file.
    /// </summary>
    /// <example>/tmp/sftp-work/frends_637896781104694806az33q4kf.8CO.</example>
    public string WorkFilePath
    {
        get
        {
            return Path.Combine(WorkFileDir, WorkFileName);
        }
    }
}
