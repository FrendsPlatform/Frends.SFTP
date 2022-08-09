namespace Frends.SFTP.UploadFiles.Definitions;

internal class WorkFileInfo
{
    public WorkFileInfo(string originalFileName, string workFileName, string workFileDir)
    {
        OriginalFileName = originalFileName ?? string.Empty;
        WorkFileName = workFileName ?? string.Empty;
        WorkFileDir = workFileDir ?? string.Empty;
    }

    public readonly string OriginalFileName;

    public readonly string WorkFileName;

    public readonly string WorkFileDir;

    public string WorkFilePath
    {
        get
        {
            return Path.Combine(WorkFileDir, WorkFileName);
        }
    }
}

