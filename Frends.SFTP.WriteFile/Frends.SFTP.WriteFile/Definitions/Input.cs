using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Input
{
    /// <summary>
    /// Full path of the target file to be written.
    /// </summary>
    /// <example>/destination</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; set; }

    /// <summary>
    /// Text content to be written.
    /// </summary>
    /// <example>This is test file</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Content { get; set; }

    /// <summary>
    /// If set, this ecoding will be used to encode and decode command 
    /// parameters and server responses, such as file names. 
    /// By selecting 'Other' you can use any encoding.
    /// </summary>
    /// <example>FileEncoding.ANSI</example>
    [DefaultValue(FileEncoding.ANSI)]
    public FileEncoding FileEncoding { get; set; }

    /// <summary>
    /// Additional option for UTF-8 encoding to enable bom.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example>utf-8</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }

    /// <summary>
    /// How the file write should work if a file with the new name already exists.
    /// </summary>
    /// <example>WriteOperation.Append</example>
    public WriteOperation WriteBehaviour { get; set; }
}

