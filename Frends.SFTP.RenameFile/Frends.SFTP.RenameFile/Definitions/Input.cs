using System.ComponentModel.DataAnnotations;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Input
{
    /// <summary>
    /// Full path of the target file to be renamed.
    /// </summary>
    /// <example>/root/folder/example.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; set; }

    /// <summary>
    /// New name for the file.
    /// </summary>
    /// <example>newName.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string NewFileName { get; set; }

    /// <summary>
    /// How the file rename should work if a file with the new name already exists.
    /// If Rename is selected, will append a number to the new file name e.g. renamed(2).txt
    /// </summary>
    public RenameBehaviour RenameBehaviour { get; set;  }
}
