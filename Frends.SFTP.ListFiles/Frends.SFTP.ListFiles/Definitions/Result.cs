namespace Frends.SFTP.ListFiles.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
	/// <summary>
	/// Count of files found from the directory.
	/// </summary>
	/// <example>2</example>
	public int Count { get; private set; }

    /// <summary>
    /// List of file items found from the directory.
    /// </summary>
    /// <example>
    /// <code>
    /// [
	///		{
	///			"FullPath": "/upload/down/test2.txt",
	///			"IsDirectory": false,
	///			"IsFile": true,
	///			"Length": 54,
	///			"Name": "test2.txt",
	///			"LastWriteTimeUtc": "2022-11-07T12:17:36Z",
	///			"LastAccessTimeUtc": "2022-11-07T12:17:37Z",
	///			"LastWriteTime": "2022-11-07T14:17:36+02:00",
	///			"LastAccessTime": "2022-11-07T14:17:37+02:00"
	///		},
	///		{
	///			"FullPath": "/upload/down/test1.txt",
	///			"IsDirectory": false,
	///			"IsFile": true,
	///			"Length": 0,
	///			"Name": "test1.txt",
	///			"LastWriteTimeUtc": "2022-11-07T12:17:36Z",
	///			"LastAccessTimeUtc": "2022-11-07T12:17:37Z",
	///			"LastWriteTime": "2022-11-07T14:17:36+02:00",
	///			"LastAccessTime": "2022-11-07T14:17:37+02:00"
	///		},
	/// ]
    /// </code>
    /// </example>
    public List<FileItem> Files { get; private set; }

    internal Result(List<FileItem> files)
    {
		Count = files.Count;
        Files = files;
    }
}

