namespace Frends.SFTP.DownloadFiles.Definitions;

internal class WorkFileInfo
{
    internal readonly string OriginalFileName;

    internal readonly string WorkFileName;

    internal readonly string WorkFileDir;

    public WorkFileInfo(string originalFileName, string workFileName, string workFileDir)
    {
        OriginalFileName = originalFileName ?? string.Empty;
        WorkFileName = workFileName ?? string.Empty;
        WorkFileDir = workFileDir ?? string.Empty;
    }

    public string WorkFilePath
    {
        get
        {
            return Path.Combine(WorkFileDir, WorkFileName);
        }
    }
}