namespace Frends.SFTP.DownloadFiles.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Source transfer options
/// </summary>
public class Source
{
    /// <summary>
    /// Directory on the server.
    /// </summary>
    /// <example>/upload/</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// File name or file mask of the files to be fetched.
    /// </summary>
    /// <example>test.txt</example>
    [DefaultValue("")]
    public string FileName { get; set; }

    /// <summary>
    /// Determines if subdirectories should be included when searching source files.
    /// Depending on how many subdirectories there are, this operation can be extremely expensive.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool IncludeSubdirectories { get; set; }

    /// <summary>
    /// What to do if source file is not found. Error = alarm and fail,
    /// Info = alarm info and quit with success status, Ignore = quit
    /// with success status.
    /// </summary>
    /// <example>SourceAction.Error</example>
    [DefaultValue(SourceAction.Error)]
    public SourceAction Action { get; set; }

    /// <summary>
    /// What to do with the source file after transfer.
    /// </summary>
    /// <example>SourceOperation.Delete</example>
    [DefaultValue(SourceOperation.Delete)]
    public SourceOperation Operation { get; set; }

    /// <summary>
    /// Parameter for Rename operation. You can use file macros and also specify a directory
    /// where to move the files to, e.g. /subdir/%Date%file.txt. If you don't define a
    /// directory path, the source directory is used. When using rename, this parameter
    /// must always contain a file name.
    /// </summary>
    /// <example>transferred.txt</example>
    [UIHint(nameof(Operation), "", SourceOperation.Rename)]
    public string FileNameAfterTransfer { get; set; }

    /// <summary>
    /// Parameter for Move operation. Set the full path to the directory without the file name. You can use some macros in the directory name, e.g. /subdir/%Year%_uploaded/.
    /// </summary>
    /// <example>/upload/transferred/</example>
    [UIHint(nameof(Operation), "", SourceOperation.Move)]
    public string DirectoryToMoveAfterTransfer { get; set; }

    /// <summary>
    /// The paths to the files to transfer, mainly meant to be used with the file trigger with the syntax: #trigger.data.filePaths
    /// </summary>
    /// <example>#trigger.data.filePaths</example>
    [DefaultValue("")] // set to empty string so 4.2 shows the field as empty by default
    public object FilePaths { get; set; }
}