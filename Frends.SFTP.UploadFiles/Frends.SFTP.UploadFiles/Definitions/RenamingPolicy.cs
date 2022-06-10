﻿using System.Text.RegularExpressions;

namespace Frends.SFTP.UploadFiles.Definitions;

///<summary>
/// Policies for creating names for remote files: expands macros etc.
///</summary>
internal class RenamingPolicy
{
    private IDictionary<string, Func<string, string>> MacroHandlers;
    private IDictionary<string, Func<string, string>> SourceFileNameMacroHandlers;

    public RenamingPolicy() : this("", Guid.Empty)
    {
    }

    public RenamingPolicy(string transferName, Guid transferId)
    {
        MacroHandlers = InitializeMacroHandlers(transferName, transferId);
        SourceFileNameMacroHandlers = InitializeSourceFileNameMacroHandlers();
    }

    public string CreateRemoteFileName(string originalFileName, string remoteFileDefinition)
    {
        if (!string.IsNullOrEmpty(remoteFileDefinition) && remoteFileDefinition.Contains("?"))
            throw new ArgumentException("Character '?' not allowed in remote filename.", "remoteFileDefinition");

        if (string.IsNullOrEmpty(originalFileName))
            throw new ArgumentException("Original filename must be set.", "originalFileName");

        var originalFileNameWithoutPath = Path.GetFileName(originalFileName);

        if (string.IsNullOrEmpty(remoteFileDefinition)) return originalFileNameWithoutPath;

        if (!IsFileMask(remoteFileDefinition) &&
            !IsFileMacro(remoteFileDefinition, MacroHandlers) &&
            !IsFileMacro(remoteFileDefinition, SourceFileNameMacroHandlers))
        {
            // remoteFileDefination does not have macros
            var remoteFileName = Path.GetFileName(remoteFileDefinition);

            if (string.IsNullOrEmpty(remoteFileName))
                remoteFileDefinition = Path.Combine(remoteFileDefinition, originalFileNameWithoutPath);

            return remoteFileDefinition;
        }

        var result = this.ExpandMacrosAndMasks(originalFileName, remoteFileDefinition);

        if (result.EndsWith("\\")) result = Path.Combine(result, originalFileNameWithoutPath);

        return result;
    }

    public string ExpandDirectoryForMacros(string directory)
    {
        if (directory.Contains("%SourceFileName%") || directory.Contains("%SourceFileExtension%"))
            throw new Exception("'%SourceFileName%' and '%SourceFileExtension%' are not supported macros for source and destination directories.");

        return ExpandFileMacros(directory);
    }
       
    public string CreateRemoteFilePathForMove(string sourceOperationTo, string sourceFilePath)
    {
        var directoryName = sourceOperationTo;
        if (string.IsNullOrEmpty(directoryName))
            throw new ArgumentException("When using move as a source operation, you should always define a directory", "sourceOperationTo");


        directoryName = CanonizeAndCheckPath(directoryName);

        // this should always be a directory
        if (!directoryName.EndsWith("/"))
            directoryName = directoryName + "/";
        var sourceFileName = Path.GetFileName(sourceFilePath);
        return Path.Combine(directoryName, sourceFileName);
    }

    private static string CanonizeAndCheckPath(string path)
    {
        path = path.Replace(Path.DirectorySeparatorChar, '/'); // make all the paths use forward slashes - this should be supported on File, FTP, and SFTP

        if (path.IndexOfAny(GetInvalidChars()) != -1)
            throw new ArgumentException("Illegal characters in path: " + path);
        return path;
    }

    private static char[] GetInvalidChars()
    {
        List<char> invalidCharacters = new List<char>(Path.GetInvalidFileNameChars());
        invalidCharacters.Remove('/'); // remove the forward slash, as it is supported
        invalidCharacters.Remove(':'); // also the colon is supported
        return invalidCharacters.ToArray();
    }

    public string CreateRemoteFileNameForRename(string originalFilePath, string sourceOperationTo)
    {
        if (String.IsNullOrEmpty(sourceOperationTo))
            throw new ArgumentException("When using rename as a source operation, you need to define the new name");

        string filePath = sourceOperationTo;
        filePath = ExpandMacrosAndMasks(originalFilePath, filePath);

        return CanonizeAndCheckPath(filePath);
    }

    public bool IsMacro(string macro)
    {
        return IsFileMacro(macro, MacroHandlers) || IsFileMacro(macro, SourceFileNameMacroHandlers);
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
        if (IsFileMacro(filename, MacroHandlers))
            filename = ReplaceMacro(filename);

        return filename;
    }

    private string ExpandSourceFileNameMacros(string filePath, string originalFile)
    {
        string filename = filePath;
        if (IsFileMacro(filename, SourceFileNameMacroHandlers))
            filename = ReplaceSourceFileMacro(filename, originalFile);

        return filename;
    }

    private static string ExpandFileMasks(string filePath, string originalFileName)
    {
        string filename = filePath;
        if (IsFileMask(filename))
            filename = NameByMask(originalFileName, filename);

        return filename;
    }

    private static string NameByMask(string filename, string mask)
    {
        //remove extension if it is wanted to be changed, new extension is added later on to new filename
        if (mask.Contains("*."))
            if (Path.HasExtension(filename)) filename = Path.GetFileNameWithoutExtension(filename);

        int i = mask.IndexOf("*");
        if (i >= 0)
        {
            string tmp = mask.Substring(0, i);
            return String.Concat(tmp + filename + mask.Substring(i + 1, (mask.Length - (i + 1))));
        }

        //Not an mask return mask.
        return mask;
    }

    private static bool IsFileMacro(string s, IDictionary<string, Func<string, string>> macroDictionary)
    {
        if (s == null) return false;

        foreach (var key in macroDictionary.Keys)
        {
            if (s.ToUpperInvariant().Contains(key.ToUpperInvariant())) return true;
        }

        return false;
    }

    private static bool IsFileMask(string s)
    {
        bool b = false;
        if (s == null) return false;
        if (s.IndexOf("*") >= 0) b = true;
        if (s.IndexOf("?") >= 0) b = true;
        return b;
    }

    private static IDictionary<string, Func<string, string>> InitializeSourceFileNameMacroHandlers()
    {
        return new Dictionary<string, Func<string, string>>
            {
                {"%SourceFileName%", Path.GetFileNameWithoutExtension},
                {"%SourceFileExtension%", (originalFile) => Path.HasExtension(originalFile) ? Path.GetExtension(originalFile) : String.Empty},
            };
    }

    private static IDictionary<string, Func<string, string>> InitializeMacroHandlers(string transferName, Guid transferId)
    {
        return new Dictionary<string, Func<string, string>>
            {
                {"%Ticks%", (s) => DateTime.Now.Ticks.ToString()},
                {"%DateTimeMs%", (s) => DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")},
                {"%DateTime%", (s) => DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")},
                {"%Date%", (s) => DateTime.Now.ToString("yyyy-MM-dd")},
                {"%Time%", (s) => DateTime.Now.ToString("HH-mm-ss")},
                {"%Year%", (s) => DateTime.Now.ToString("yyyy")},
                {"%Month%", (s) => DateTime.Now.ToString("MM")},
                {"%Day%", (s) => DateTime.Now.ToString("dd")},
                {"%Hour%", (s) => DateTime.Now.ToString("HH")},
                {"%Minute%", (s) => DateTime.Now.ToString("mm")},
                {"%Second%", (s) => DateTime.Now.ToString("ss")},
                {"%Millisecond%", (s) => DateTime.Now.ToString("fff")},
                {"%Guid%", (s) => Guid.NewGuid().ToString()},
                {"%TransferName%", (s) => !String.IsNullOrEmpty(transferName) ? transferName : String.Empty},
                {"%TransferId%", (s) => transferId.ToString().ToUpper()},
                {"%WeekDay%", (s) => (DateTime.Now.DayOfWeek > 0 ? (int)DateTime.Now.DayOfWeek : 7).ToString()}
            };
    }

    private string ReplaceSourceFileMacro(string fileDefinition, string originalFile)
    {
        return ExpandMacrosFromDictionary(fileDefinition, SourceFileNameMacroHandlers, originalFile); ;
    }

    private string ReplaceMacro(string fileDefinition)
    {
        return ExpandMacrosFromDictionary(fileDefinition, MacroHandlers, "");
    }

    private static string ExpandMacrosFromDictionary(string fileDefinition, IDictionary<string, Func<string, string>> macroHandlers, string originalFile)
    {
        foreach (var macroHandler in macroHandlers)
        {
            fileDefinition = Regex.Replace(fileDefinition, Regex.Escape(macroHandler.Key), macroHandler.Value.Invoke(originalFile), RegexOptions.IgnoreCase);
        }

        return fileDefinition;
    }
}

