namespace Frends.SFTP.DownloadFiles.Definitions;

internal class WorkFileInfo
{
    internal readonly string OriginalFileName;

    internal readonly string WorkFileName;

    internal readonly string WorkFileDir;

    internal readonly string SafeTempFileName;

    public WorkFileInfo(string originalFileName, string workFileName, string workFileDir, string safeTempFileName)
    {
        OriginalFileName = originalFileName ?? string.Empty;
        WorkFileName = workFileName ?? string.Empty;
        WorkFileDir = workFileDir ?? string.Empty;
        SafeTempFileName = safeTempFileName;
    }

    public string WorkFilePath
    {
        get
        {
            return Path.Combine(WorkFileDir, WorkFileName);
        }
    }
}