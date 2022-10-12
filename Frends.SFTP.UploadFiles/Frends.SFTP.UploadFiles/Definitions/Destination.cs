using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions;
/// <summary>
/// Destination transfer options.
/// </summary>
public class Destination
{
    /// <summary>
    /// Directory on the server.
    /// </summary>
    /// <example>/upload/</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// File name of the destination file with possible macros.
    /// </summary>
    /// <example>test.txt</example>
    [DefaultValue("")]
    [DisplayFormat(DataFormatString = "Text")]
    public string FileName { get; set; }

    /// <summary>
    /// If set, this ecoding will be used to encode and decode command 
    /// parameters and server responses, such as file names. 
    /// By selecting 'Other' you can use any encoding.
    /// </summary>
    /// <example>FileEncoding.ANSI</example>
    [DefaultValue(FileEncoding.ANSI)]
    public FileEncoding FileNameEncoding { get; set; }

    /// <summary>
    /// Additional option for UTF-8 encoding to enable bom.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(FileNameEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBomForFileName { get; set; }

    /// <summary>
    /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example>"utf-64"</example>
    [UIHint(nameof(FileNameEncoding), "", FileEncoding.Other)]
    public string FileNameEncodingInString { get; set; }

    /// <summary>
    /// Operation to determine what to do if destination file exists. Appending is not supported to Azure Blob Storage.
    /// </summary>
    /// <example>DestinationAction.Error</example>
    [DefaultValue(DestinationAction.Error)]
    public DestinationAction Action { get; set; }

    /// <summary>
    /// If enabled new line is added to the existing file before appending the content.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    [UIHint(nameof(Action), "", DestinationAction.Append)]
    public bool AddNewLine { get; set; }

    /// <summary>
    /// Encoding for the appending content. By selecting 'Other' you can use any encoding.
    /// </summary>
    /// <example>FileEncoding.ANSI</example>
    [DefaultValue(FileEncoding.ANSI)]
    [UIHint(nameof(Action), "", DestinationAction.Append)]
    public FileEncoding FileContentEncoding { get; set; }

    /// <summary>
    /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example>utf-64</example>
    [UIHint(nameof(FileContentEncoding), "", FileEncoding.Other)]
    public string FileContentEncodingInString { get; set; }

    /// <summary>
    /// Additional option for UTF-8 encoding to enable bom.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(FileContentEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBomForContent { get; set; }
}

