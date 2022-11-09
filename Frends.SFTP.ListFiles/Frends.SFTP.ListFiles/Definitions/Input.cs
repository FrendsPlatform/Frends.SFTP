using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.ListFiles.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Input
{
    /// <summary>
    /// Directory on the server.
    /// </summary>
    /// <example>/directory/test/</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; } = "/";

    /// <summary>
    /// Pattern to match (Optional).
    /// </summary>
    /// <example>*.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string FileMask { get; set; }

    /// <summary>
    /// Types to include in the directory listing.
    /// </summary>
    /// <example>IncludeType.File</example>
    [DefaultValue(IncludeType.File)]
    public IncludeType IncludeType { get; set; } = IncludeType.File;

    /// <summary>
    /// Include subdirectories?
    /// </summary>
    /// <example>true</example>
    public bool IncludeSubdirectories { get; set; }

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
    /// <example>utf-8</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }
}

