namespace Frends.SFTP.DeleteDirectory.Definitions;

using Frends.SFTP.DeleteDirectory.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path of the target file to be deleted.
    /// </summary>
    /// <example>/destination</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// If set, this ecoding will be used to encode and decode command parameters and server responses, such as file names.
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
    /// File encoding to be used.
    /// Encoding don't support any unicode encoding. It only support the code page encodings.
    /// A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example>utf-8</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }
}