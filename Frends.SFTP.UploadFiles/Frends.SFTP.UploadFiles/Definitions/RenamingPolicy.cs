using System.Text.RegularExpressions;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Policies for creating names for remote files: expands macros etc.
/// </summary>
internal class RenamingPolicy
{
    private readonly IDictionary<string, Func<string, string>> macroHandlers;
    private readonly IDictionary<string, Func<string, string>> sourceFileNameMacroHandlers;

    public RenamingPolicy(string transferName, Guid transferId)
    {
        macroHandlers = InitializeMacroHandlers(transferName, transferId);
        sourceFileNameMacroHandlers = InitializeSourceFileNameMacroHandlers();
    }

    /// <summary>
    /// Creates a destination filename by applying the configured name pattern and macros.
    /// </summary>
    /// <param name="originalFileName">Original name of the source file.</param>
    /// <param name="remoteFileDefinition">Destination filename pattern.</param>
    /// <returns>Destination filename after applying macros and masks.</returns>
    public string CreateRemoteFileName(string originalFileName, string remoteFileDefinition)
    {
        if (!string.IsNullOrEmpty(remoteFileDefinition) && remoteFileDefinition.Contains('?'))
            throw new ArgumentException("Character '?' not allowed in remote filename.", nameof(remoteFileDefinition));

        if (string.IsNullOrEmpty(originalFileName))
            throw new ArgumentException("Original filename must be set.", nameof(originalFileName));

        var originalFileNameWithoutPath = Path.GetFileName(originalFileName);

        if (string.IsNullOrEmpty(remoteFileDefinition)) return originalFileNameWithoutPath;

        if (!IsFileMask(remoteFileDefinition) &&
            !IsFileMacro(remoteFileDefinition, macroHandlers) &&
            !IsFileMacro(remoteFileDefinition, sourceFileNameMacroHandlers))
        {
            // remoteFileDefination does not have macros
            var remoteFileName = Path.GetFileName(remoteFileDefinition);

            if (string.IsNullOrEmpty(remoteFileName))
                remoteFileDefinition = Path.Combine(remoteFileDefinition, originalFileNameWithoutPath);

            return remoteFileDefinition;
        }

        var result = ExpandMacrosAndMasks(originalFileName, remoteFileDefinition);

        if (result.EndsWith("\\")) result = Path.Combine(result, originalFileNameWithoutPath);

        return result;
    }

    /// <summary>
    /// Expands supported macros in a source or destination directory.
    /// </summary>
    /// <param name="directory">Directory path that may contain macros.</param>
    /// <returns>Directory path with supported macros expanded.</returns>
    public string ExpandDirectoryForMacros(string directory)
    {
        if (directory.Contains("%SourceFileName%") || directory.Contains("%SourceFileExtension%"))
            throw new Exception("'%SourceFileName%' and '%SourceFileExtension%' are not supported macros for source and destination directories.");

        return ExpandFileMacros(directory);
    }

    /// <summary>
    /// Replaces the source-relative-directory macro with the file's relative directory.
    /// </summary>
    /// <param name="path">Path that may contain the source-relative-directory macro.</param>
    /// <param name="relativeDirectoryPath">Directory path relative to the source root.</param>
    /// <returns>Path with the macro replaced.</returns>
    public string ExpandSourceRelativeDirectoryMacro(string path, string relativeDirectoryPath)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return Regex.Replace(
            path,
            Regex.Escape("%SourceRelativeDirectory%"),
            relativeDirectoryPath ?? string.Empty,
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Normalizes path separators and rejects paths containing invalid characters.
    /// </summary>
    /// <param name="path">Path to normalize and validate.</param>
    /// <returns>Validated path using forward slashes.</returns>
    public string CanonizeAndCheckPath(string path)
    {
        path = path.Replace(Path.DirectorySeparatorChar, '/'); // make all the paths use forward slashes - this should be supported on File, FTP, and SFTP

        if (path.IndexOfAny(GetInvalidChars()) != -1)
            throw new ArgumentException("Illegal characters in path: " + path);
        return path;
    }

    /// <summary>
    /// Creates and validates the destination path for a source-file rename operation.
    /// </summary>
    /// <param name="originalFilePath">Path of the source file.</param>
    /// <param name="sourceOperationTo">Configured destination path for the rename.</param>
    /// <returns>Validated destination path.</returns>
    internal string CreateRemoteFileNameForRename(string originalFilePath, string sourceOperationTo)
    {
        if (string.IsNullOrEmpty(sourceOperationTo))
            throw new ArgumentException("When using rename as a source operation, you need to define the new name");

        string filePath = sourceOperationTo;
        filePath = ExpandMacrosAndMasks(originalFilePath, filePath);

        return CanonizeAndCheckPath(filePath);
    }

    private char[] GetInvalidChars()
    {
        List<char> invalidCharacters = new List<char>(Path.GetInvalidFileNameChars());
        invalidCharacters.Remove('/'); // remove the forward slash, as it is supported
        invalidCharacters.Remove(':'); // also the colon is supported
        return invalidCharacters.ToArray();
    }

    private string ExpandMacrosAndMasks(string originalFilePath, string filePath)
    {
        var expandedPath = ExpandFileMacros(filePath);
        expandedPath = ExpandSourceFileNameMacros(expandedPath, originalFilePath);
        expandedPath = ExpandFileMasks(expandedPath, originalFilePath);

        return expandedPath;
    }

    private string ExpandFileMacros(string filePath)
    {
        string filename = filePath;
        if (IsFileMacro(filename, macroHandlers))
            filename = ReplaceMacro(filename);

        return filename;
    }

    private string ExpandSourceFileNameMacros(string filePath, string originalFile)
    {
        string filename = filePath;
        if (IsFileMacro(filename, sourceFileNameMacroHandlers))
            filename = ReplaceSourceFileMacro(filename, originalFile);

        return filename;
    }

    private string ExpandFileMasks(string filePath, string originalFileName)
    {
        string filename = filePath;
        if (IsFileMask(filename))
            filename = NameByMask(originalFileName, filename);

        return filename;
    }

    private string NameByMask(string filename, string mask)
    {
        // remove extension if it is wanted to be changed, new extension is added later on to new filename
        if (mask.Contains("*."))
            if (Path.HasExtension(filename)) filename = Path.GetFileNameWithoutExtension(filename);

        int i = mask.IndexOf("*");
        if (i >= 0)
        {
            string tmp = mask.Substring(0, i);
            return string.Concat(tmp + filename + mask.Substring(i + 1, mask.Length - (i + 1)));
        }

        // Not an mask return mask.
        return mask;
    }

    private bool IsFileMacro(string s, IDictionary<string, Func<string, string>> macroDictionary)
    {
        if (s == null) return false;

        foreach (var key in macroDictionary.Keys)
        {
            if (s.ToUpperInvariant().Contains(key.ToUpperInvariant())) return true;
        }

        return false;
    }

    private bool IsFileMask(string s)
    {
        bool b = false;

        if (s == null) return false;

        if (s.Contains('*')) b = true;

        return b;
    }

    private IDictionary<string, Func<string, string>> InitializeSourceFileNameMacroHandlers()
    {
        return new Dictionary<string, Func<string, string>>
        {
            {
                "%SourceFileName%", s =>
                {
                    var filenameWithoutExtension = Path.GetFileNameWithoutExtension(s);
                    if (string.IsNullOrEmpty(filenameWithoutExtension))
                        filenameWithoutExtension = Path.GetFileName(s);

                    return filenameWithoutExtension;
                }
            },
            {
                "%SourceFileExtension%",
                s =>
                {
                    var extension = Path.GetExtension(s);

                    return extension;
                }
            },
        };
    }

    private IDictionary<string, Func<string, string>> InitializeMacroHandlers(
        string transferName,
        Guid transferId)
    {
        return new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "%Ticks%", (s) => DateTime.Now.Ticks.ToString() },
            { "%DateTime%", (s) => DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") },
            { "%DateTimeMs%", (s) => DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") },
            { "%Date%", (s) => DateTime.Now.ToString("yyyy-MM-dd") },
            { "%Time%", (s) => DateTime.Now.ToString("HH-mm-ss") },
            { "%Year%", (s) => DateTime.Now.Year.ToString() },
            { "%Month%", (s) => DateTime.Now.Month.ToString("00") },
            { "%Day%", (s) => DateTime.Now.Day.ToString("00") },
            { "%Hour%", (s) => DateTime.Now.Hour.ToString("00") },
            { "%Minute%", (s) => DateTime.Now.Minute.ToString("00") },
            { "%Second%", (s) => DateTime.Now.Second.ToString("00") },
            { "%Millisecond%", (s) => DateTime.Now.Millisecond.ToString("000") },
            { "%WeekDay%", (s) => ((int)DateTime.Now.DayOfWeek == 0 ? 7 : (int)DateTime.Now.DayOfWeek).ToString() },
            { "%Guid%", (s) => Guid.NewGuid().ToString() },
            { "%TransferId%", (s) => transferId.ToString() },
            { "%TransferName%", (s) => transferName },
        };
    }

    private string ReplaceMacro(string fileName)
    {
        foreach (var key in macroHandlers.Keys)
        {
            while (fileName.ToUpperInvariant().Contains(key.ToUpperInvariant()))
            {
                fileName = Regex.Replace(fileName, Regex.Escape(key), macroHandlers[key](fileName), RegexOptions.IgnoreCase);
            }
        }

        return fileName;
    }

    private string ReplaceSourceFileMacro(string fileName, string originalFilePath)
    {
        foreach (var key in sourceFileNameMacroHandlers.Keys)
        {
            while (fileName.ToUpperInvariant().Contains(key.ToUpperInvariant()))
            {
                fileName = Regex.Replace(
                    fileName,
                    Regex.Escape(key),
                    sourceFileNameMacroHandlers[key].Invoke(originalFilePath),
                    RegexOptions.IgnoreCase);
            }
        }

        return fileName;
    }
}
