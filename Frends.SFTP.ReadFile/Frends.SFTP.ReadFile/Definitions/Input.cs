using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Input
{
    /// <summary>
    /// Full path of the target file to be read.
    /// </summary>
    /// <example>/destination</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; set; }

    /// <summary>
    /// Encoding for the read content.
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
    /// <example>iso-8859-1</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }
}
