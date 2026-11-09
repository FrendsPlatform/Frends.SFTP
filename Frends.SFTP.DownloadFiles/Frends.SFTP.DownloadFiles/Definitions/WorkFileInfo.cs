namespace Frends.SFTP.DownloadFiles.Definitions;

internal class WorkFileInfo
{
    internal WorkFileInfo(string originalFileName, string workFileName, string workFileDir, string safeTempFileName)
    {
        OriginalFileName = originalFileName ?? string.Empty;
        WorkFileName = workFileName ?? string.Empty;
        WorkFileDir = workFileDir ?? string.Empty;
        SafeTempFileName = safeTempFileName;
    }

    internal string OriginalFileName { get; }

    internal string WorkFileName { get; }

    internal string WorkFileDir { get; }

    internal string SafeTempFileName { get; }

    internal string WorkFilePath
    {
        get
        {
            return Path.Combine(WorkFileDir, WorkFileName);
        }
    }
}
