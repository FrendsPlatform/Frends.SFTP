using System.Text;
using Renci.SshNet;

namespace Frends.SFTP.DownloadFiles.Definitions;

internal class SingleFileTransfer
{
    private readonly RenamingPolicy _renamingPolicy;
    private readonly ISFTPLogger _logger;
    private readonly SingleFileTransferResult _result;

    public SingleFileTransfer(FileItem file, BatchContext context, SftpClient client, RenamingPolicy renamingPolicy, ISFTPLogger logger)
    {
        _renamingPolicy = renamingPolicy;
        _logger = logger;

        FileTransferStartTime = DateTime.UtcNow;
        Client = client;
        SourceFile = file;
        WorkFile = file;
        BatchContext = context;

        DestinationFileWithMacrosExpanded = Path.Combine(
            renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory), 
            renamingPolicy.CreateRemoteFileName(
                file.Name,
                context.Destination.FileName));
        WorkFileInfo = new WorkFileInfo(file.Name, Path.GetFileName(DestinationFileWithMacrosExpanded), BatchContext.TempWorkDir);

        _result = new SingleFileTransferResult { Success = true };
    }

    public DateTime FileTransferStartTime { get; set; }

    public WorkFileInfo WorkFileInfo { get; set; }

    public FileItem WorkFile { get; set; }

    public SftpClient Client { get; set; }

    public FileItem SourceFile { get; set; }

    public FileItem DestinationFile { get; set; }

    public string DestinationFileWithMacrosExpanded { get; set; }

    private string SourceFileDuringTransfer { get; set; }

    public string DestinationFileDuringTransfer { get; set; }

    public BatchContext BatchContext { get; set; }

    /// <summary>
    /// Transfer state for SFTP Logger
    /// </summary>
    public TransferState State { get; set; }

    internal SingleFileTransferResult TransferSingleFile()
    {
        try
        {
            _result.TransferredFile = SourceFile.Name;
            _result.TransferredFilePath = SourceFile.FullPath;

            GetSourceFile();

            ExecuteSourceOperationMoveOrRename();

            if (DestinationFileExists(DestinationFileWithMacrosExpanded))
            {
                DestinationFile = new FileItem(DestinationFileWithMacrosExpanded);
                switch (BatchContext.Destination.Action)
                {
                    case DestinationAction.Append:
                        AppendDestinationFile();
                        break;
                    case DestinationAction.Overwrite:
                        PutDestinationFile(removeExisting: true);
                        break;
                    case DestinationAction.Error:
                        throw new DestinationFileExistsException(Path.GetFileName(DestinationFileWithMacrosExpanded));
                }
            }
            else PutDestinationFile();

            if (BatchContext.Options.PreserveLastModified) RestoreModified();

            ExecuteSourceOperationNothingOrDelete();

            _logger.LogTransferSuccess(this, BatchContext);
            CleanUpFiles();
        }
        catch (Exception ex)
        {
            var sourceFileRestoreMessage = RestoreSourceFileAfterErrorIfItWasRenamed();
            HandleTransferError(ex, sourceFileRestoreMessage);

            var destinationFileRestoreMessage = RestoreDestinationFileAfterErrorIfItWasRenamed();
            if (!string.IsNullOrEmpty(destinationFileRestoreMessage))
                HandleTransferError(ex, destinationFileRestoreMessage);
        }
        return _result;
    }

    private void GetSourceFile()
    {
        if (BatchContext.Options.RenameSourceFileBeforeTransfer)
            RenameSourceFile();
        else
            SourceFileDuringTransfer = SourceFile.FullPath;

        SetCurrentState(TransferState.GetFile, $"Downloading temporary source file {Path.GetFileName(SourceFileDuringTransfer)} to local temp folder {WorkFileInfo.WorkFileDir}");
        using (var fs = File.Open(Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer)), FileMode.Create))
        {
            Client.DownloadFile(SourceFileDuringTransfer, fs);
        }
    }

    private bool DestinationFileExists(string path)
    {
        SetCurrentState(
            TransferState.CheckIfDestinationFileExists,
            $"Checking if destination file {Path.GetFileName(path)} exists.");
        var exists = File.Exists(path);
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {path}: {exists}.");

        return exists;
    }

    private void RenameSourceFile()
    {
        if (!string.IsNullOrEmpty(SourceFileDuringTransfer))
        {
            Client.RenameFile(SourceFileDuringTransfer, SourceFile.FullPath);
            return;
        }

        var uniqueFileName = Util.CreateUniqueFileName(); 
        var directory = Path.GetDirectoryName(SourceFile.FullPath);

        SourceFileDuringTransfer = (SourceFile.FullPath.Contains('/')) 
            ? Path.Combine(directory, uniqueFileName).Replace("\\", "/") 
            : Path.Combine(directory, uniqueFileName);

        SetCurrentState(TransferState.RenameSourceFileBeforeTransfer, $"Renaming source file {SourceFile.Name} to temporary file name {uniqueFileName} before transfer.");

        Client.RenameFile(SourceFile.FullPath, SourceFileDuringTransfer);
        _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFile.Name} renamed to target {Path.GetFileName(SourceFileDuringTransfer)}.");
    }

    private void AppendDestinationFile()
    {
        var filePath = Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer));
        Encoding encoding;
        try
        {
            encoding = GetEncoding(BatchContext.Destination);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in initializing file content encoding: ", ex);
        }

        if (BatchContext.Options.RenameDestinationFileDuringTransfer)
            RenameDestinationFile();

        Append(GetSourceFileContent(filePath, encoding), encoding);

        if (BatchContext.Options.RenameDestinationFileDuringTransfer)
            RenameDestinationFile();
    }

    private void RenameDestinationFile()
    {
        if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
        {
            SetCurrentState(TransferState.RenameDestinationFile, $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {DestinationFile.Name}.");
            File.Move(DestinationFileDuringTransfer, DestinationFile.FullPath);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {DestinationFile.Name}.");
        }
        else
        {
            DestinationFileDuringTransfer = Path.Combine(Path.GetDirectoryName(DestinationFile.FullPath), Util.CreateUniqueFileName());
            SetCurrentState(TransferState.RenameDestinationFile, $"Renaming destination file {DestinationFile.Name} to temporary file name {Path.GetFileName(DestinationFileDuringTransfer)} during transfer.");
            File.Move(DestinationFile.FullPath, DestinationFileDuringTransfer);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Destination file {DestinationFile.Name} renamed to target {Path.GetFileName(DestinationFileDuringTransfer)}.");
        }       
    }

    private void Append(string[] content, Encoding encoding)
    {
        SetCurrentState(
            TransferState.AppendToDestinationFile,
            $"Appending file {Path.GetFileName(SourceFileDuringTransfer)} to existing file {DestinationFile.Name}.");

        // If destination rename during transfer is enabled, use that instead 
        var path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer)) 
            ? DestinationFileDuringTransfer
            : DestinationFile.FullPath;

        File.AppendAllLines(path, content, encoding);
        _logger.NotifyInformation(BatchContext, $"FILE APPEND: Source file appended to target {DestinationFile.Name}.");
    }

    private static string[] GetSourceFileContent(string filePath, Encoding encoding)
    {
        var result = new List<string>();
        result.Add("\n");
        string[] content;
        content = File.ReadAllLines(filePath, encoding);
            
        foreach (var line in content)
            result.Add(line);

        return result.ToArray();

    }

    /// <summary>
    /// Downloads source file to local directory. Overwrites the destination file if enabled.
    /// </summary>
    /// <param name="removeExisting"></param>
    private void PutDestinationFile(bool removeExisting = false)
    {
        var doRename = BatchContext.Options.RenameDestinationFileDuringTransfer;

        DestinationFileDuringTransfer = doRename
            ? Path.Combine(Path.GetDirectoryName(DestinationFileWithMacrosExpanded), Util.CreateUniqueFileName())
            : DestinationFileWithMacrosExpanded;

        var helper = doRename ? "temporary " : string.Empty;
        SetCurrentState(
            TransferState.PutFile,
            $"Downloading {helper}destination file {Path.GetFileName(DestinationFileDuringTransfer)}.");

        File.Copy(Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer)), DestinationFileDuringTransfer, removeExisting);

        _logger.NotifyInformation(BatchContext, $"FILE COPY {SourceFileDuringTransfer} to {DestinationFileDuringTransfer}.");

        if (doRename)
        {
            if (removeExisting)
            {
                SetCurrentState(
                    TransferState.DeleteDestinationFile,
                    $"Deleting destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} that is to be overwritten.");

                File.Delete(DestinationFileWithMacrosExpanded);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} deleted.");
            }

            SetCurrentState(
                TransferState.RenameDestinationFile,
                $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {Path.GetFileName(DestinationFileWithMacrosExpanded)}.");

            File.Move(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {Path.GetFileName(DestinationFileWithMacrosExpanded)}.");
        }
    }

    /// <summary>
    /// Restores the LastWriteTime stamp to the destination file.
    /// </summary>
    private void RestoreModified()
    {
        var date = SourceFile.Modified;
       SetCurrentState(
            TransferState.RestoreModified,
            $"Restoring the modified datetime of transferred file {Path.GetFileName(DestinationFileWithMacrosExpanded)}");
        File.SetLastWriteTime(DestinationFileWithMacrosExpanded, date);
        _logger.NotifyInformation(BatchContext, $"SET MODIFIED {date.ToString("dd.MM.yyyy hh:mm:ss")}");
    }

    private static Encoding GetEncoding(Destination dest)
    {
        switch (dest.FileNameEncoding)
        {
            case FileEncoding.UTF8:
                return dest.EnableBomForFileName ? new UTF8Encoding(true) : new UTF8Encoding(false);
            case FileEncoding.ASCII:
                return new ASCIIEncoding();
            case FileEncoding.ANSI:
                return Encoding.Default;
            case FileEncoding.WINDOWS1252:
                return CodePagesEncodingProvider.Instance.GetEncoding("windows-1252");
            case FileEncoding.Other:
                return CodePagesEncodingProvider.Instance.GetEncoding(dest.FileNameEncodingInString);
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{dest.FileContentEncoding}'.");
        }
    }

    private void ExecuteSourceOperationNothingOrDelete()
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;
        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Delete:
                SetCurrentState(TransferState.SourceOperationDelete, $"Deleting source file {Path.GetFileName(SourceFile.FullPath)} after transfer.");
                Client.DeleteFile(filePath);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Source file {filePath} deleted.");
                break;

            case SourceOperation.Nothing:
                if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                {
                    SetCurrentState(
                        TransferState.RestoreSourceFile,
                        $"Restoring source file from temporary {Path.GetFileName(SourceFileDuringTransfer)} to the original name {Path.GetFileName(SourceFile.FullPath)}.");
                    Client.RenameFile(filePath, SourceFile.FullPath);
                    _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary file {SourceFileDuringTransfer} restored to target {SourceFile.FullPath}.");
                }
                break;
        }
    }

    private void ExecuteSourceOperationMoveOrRename()
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;
 
        if (BatchContext.Source.Operation == SourceOperation.Move) 
        {
            var moveToPath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);
            SetCurrentState(TransferState.SourceOperationMove, $"Moving source file {SourceFile.FullPath} to {moveToPath}.");
            var file = Client.Get(filePath);
            if (!Client.Exists(moveToPath))
            {
                var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be moved to given directory {moveToPath} because the directory didn't exist.";
                _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in moving the source file."));
                _result.ErrorMessages.Add($"Failure in source operation: {msg}");
            }
            var destFileName = Path.Combine(moveToPath, SourceFile.Name);
            if (Client.Exists(destFileName)) throw new Exception($"Failure in source operation: File {Path.GetFileName(destFileName)} exists in move to directory.");
            destFileName = (moveToPath.Contains("/")) ? destFileName.Replace("\\", "/") : destFileName;

            file.MoveTo(destFileName);
            if (!Client.Exists(destFileName)) throw new Exception($"Failure in source operation: Failure in moving the source file.");

            _logger.NotifyInformation(BatchContext, $"Source file {SourceFileDuringTransfer} moved to target {destFileName}.");
            SourceFile = new FileItem(file);

            if (SourceFile.FullPath == null)
                _logger.NotifyInformation(BatchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
        }

        else if (BatchContext.Source.Operation == SourceOperation.Rename)
        { 
            var rename = Path.Combine(Path.GetDirectoryName(SourceFile.FullPath), _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer));
            rename = SourceFile.FullPath.Contains('/') ? rename.Replace("\\", "/") : rename;
            SetCurrentState(TransferState.SourceOperationRename, $"Renaming source file {Path.GetFileName(SourceFile.FullPath)} to {Path.GetFileName(rename)}.");

            Client.RenameFile(filePath, rename);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFileDuringTransfer} renamed to target {rename}.");

            if (!Client.Exists(rename))
            {
                var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be renamed to given name {Path.GetFileName(rename)}";
                _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in renaming source file."));
                _result.ErrorMessages.Add($"Failure in source operation: {msg}");
            }

            var file = Client.Get(rename);
            WorkFile = new FileItem(file);

            if (WorkFile.FullPath == null)
                _logger.NotifyInformation(BatchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
        }
    }

    private void CleanUpFiles()
    {
        var temporarySourceFile = Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer));
        SetCurrentState(TransferState.CleanUpFiles, $"Checking if temporary source file {temporarySourceFile} exists.");
        var exists = !string.IsNullOrEmpty(temporarySourceFile) && File.Exists(temporarySourceFile);
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {temporarySourceFile}: {exists}");
        if (exists)
        {
            SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary source file {temporarySourceFile}.");
            TryToRemoveLocalTempFile(temporarySourceFile);
        }

        exists = !string.IsNullOrEmpty(DestinationFileDuringTransfer) && File.Exists(DestinationFileDuringTransfer) && BatchContext.Options.RenameDestinationFileDuringTransfer;
        if (!exists) return;
        SetCurrentState(TransferState.CleanUpFiles, $"Checking if temporary destination file {DestinationFileDuringTransfer} exists.");
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {DestinationFileDuringTransfer}: {exists}");
        SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary destination file {WorkFileInfo.WorkFilePath}.");
        TryToRemoveDestinationTempFile();
    }

    private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
    {
        _result.Success = false; // the routine instance should be marked as failed if even one transfer fails
        var errorMessage = $"Failure in {State}: File '{SourceFile.Name}' could not be transferred to '{BatchContext.Destination.Directory}'. Error: {exception.Message}.";
        if (!string.IsNullOrEmpty(sourceFileRestoreMessage))
            errorMessage += " " + sourceFileRestoreMessage;

        _result.ErrorMessages.Add(errorMessage);

        _logger.LogTransferFailed(this, BatchContext, errorMessage, exception);
    }

    private void TryToRemoveDestinationTempFile()
    {
        // If DestinationFileNameDuringTransfer is not set,
        // the destination file already exists and DestinationFileExistAction=Error
        if (string.IsNullOrEmpty(DestinationFileDuringTransfer)) return;

        // If RenameDestinationFileDuringTransfer=False, there is no temporary file that could be deleted
        if (!BatchContext.Options.RenameDestinationFileDuringTransfer) return;

        try
        {
            SetCurrentState(TransferState.RemoveTemporaryDestinationFile, $"Removing temporary destination file {DestinationFileDuringTransfer}.");
            File.Delete(DestinationFileDuringTransfer);
            _logger.NotifyInformation(BatchContext, $"FILE DELETE: Temporary destination file {DestinationFileDuringTransfer} removed.");
        }
        catch (Exception ex)
        {
            _logger.NotifyError(BatchContext, $"Could not clean up temporary destination file '{DestinationFileDuringTransfer}': {ex.Message}.", ex);
        }
    }

    private void TryToRemoveLocalTempFile(string filePath)
    {
        try
        {
            if (FileDefinedAndExists(filePath))
            {
                if ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Temporary source file {filePath} removed.");
            }
        }
        catch (Exception e)
        {
            _logger.NotifyError(BatchContext, $"Could not clean up local file '{Path.GetFileName(filePath)}'", e);
        }
    }

    private static bool FileDefinedAndExists(string path)
    {
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    private string RestoreSourceFileAfterErrorIfItWasRenamed()
    {
        // restore the source file so we can retry the operations
        // - but only if the source file has been renamed in the first place
        if (!string.IsNullOrEmpty(SourceFileDuringTransfer))
        {
            try
            {
                if (ShouldSourceFileBeRestoredOnError())
                {
                    if (BatchContext.Source.Operation == SourceOperation.Move)
                        RestoreSourceFileIfItWasMoved();
                    if (!Path.GetFileName(WorkFile.Name).Equals(SourceFile.Name) || !SourceFileDuringTransfer.Equals(SourceFile.FullPath)) 
                        Client.RenameFile(SourceFileDuringTransfer, SourceFile.FullPath);
                    return "[Source file restored.]";
                }
            }
            catch (Exception ex)
            {
                var message = $"Could not restore original source file '{Path.GetFileName(SourceFile.FullPath)}' from temporary file '{Path.GetFileName(SourceFileDuringTransfer)}'. Error: {ex.Message}.";

                _logger.NotifyError(BatchContext, message, ex);
                return $"[{message}]";
            }
        }

        return string.Empty;
    }

    private void RestoreSourceFileIfItWasMoved()
    {
        var filePath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);

        var destFileName = Path.Combine(filePath, SourceFile.Name);
        destFileName = (filePath.Contains('/')) ? destFileName.Replace("\\", "/") : destFileName;

        if (!Client.Exists(destFileName)) return;

        var file = Client.Get(destFileName);

        var restorePath = Path.Combine(BatchContext.Source.Directory, SourceFile.Name);
        restorePath = (BatchContext.Source.Directory.Contains('/')) ? restorePath.Replace("\\", "/") : restorePath;

        file.MoveTo(restorePath);

        var path = file.FullName;

        if (!Client.Exists(path)) throw new ArgumentException("Failure in restoring moved source file.");
    }

    private string RestoreDestinationFileAfterErrorIfItWasRenamed()
    {
        if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
        {
            try
            {
                if (BatchContext.Options.RenameDestinationFileDuringTransfer)
                {
                    File.Move(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
                    return string.Empty;
                }
            } catch (Exception ex)
            {
                var message = 
                    $"Could not restore original destination file '{Path.GetFileName(DestinationFileWithMacrosExpanded)}' from temporary file '{Path.GetFileName(DestinationFileDuringTransfer)}'. Error: {ex.Message}.";

                _logger.NotifyError(BatchContext, message, ex);
                return $"[{message}]";
            }
        }

        return string.Empty;
    }

    private bool ShouldSourceFileBeRestoredOnError()
    {
        if (BatchContext.Options.RenameSourceFileBeforeTransfer) return true;

        if (BatchContext.Source.Operation == SourceOperation.Move) return true;

        if (BatchContext.Source.Operation == SourceOperation.Rename) return true;

        return false;
    }

    private void SetCurrentState(TransferState state, string msg)
    {
        State = state;
        _logger.NotifyTrace($"{state}: {msg}");
    }

    /// <summary>
    /// Exception class for more specific Exception name.
    /// </summary>
    public class DestinationFileExistsException : Exception
    {
        /// <summary>
        /// Exception message.
        /// </summary>
        /// <param name="fileName"></param>
        public DestinationFileExistsException(string fileName) : base($"Unable to transfer file. Destination file already exists: {fileName}.") { }
    }
}

