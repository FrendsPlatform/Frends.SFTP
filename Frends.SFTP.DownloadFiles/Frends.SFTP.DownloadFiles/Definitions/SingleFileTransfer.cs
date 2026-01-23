namespace Frends.SFTP.DownloadFiles.Definitions;

using Renci.SshNet;
using Renci.SshNet.Sftp;

internal class SingleFileTransfer
{
    private readonly RenamingPolicy _renamingPolicy;
    private readonly ISFTPLogger _logger;
    private readonly SingleFileTransferResult _result;

    public SingleFileTransfer(FileItem file, string destinationDirectoryWithMacrosExtended, BatchContext context,
        SftpClient client, RenamingPolicy renamingPolicy, ISFTPLogger logger)
    {
        _renamingPolicy = renamingPolicy;
        _logger = logger;

        FileTransferStartTime = DateTime.UtcNow;
        Client = client;
        SourceFile = file;
        WorkFile = file;
        BatchContext = context;

        DestinationFileWithMacrosExpanded = Path.Combine(
            destinationDirectoryWithMacrosExtended,
            renamingPolicy.CreateRemoteFileName(
                file.Name,
                context.Destination.FileName));
        WorkFileInfo = new WorkFileInfo(file.Name, Path.GetFileName(DestinationFileWithMacrosExpanded),
            BatchContext.TempWorkDir, Guid.NewGuid().ToString() + ".tmp");

        _result = new SingleFileTransferResult { Success = true };
    }

    public DateTime FileTransferStartTime { get; set; }

    public WorkFileInfo WorkFileInfo { get; set; }

    public FileItem WorkFile { get; set; }

    public SftpClient Client { get; set; }

    public FileItem SourceFile { get; set; }

    public FileItem DestinationFile { get; set; }

    public string DestinationFileWithMacrosExpanded { get; set; }

    public string DestinationFileDuringTransfer { get; set; }

    public BatchContext BatchContext { get; set; }

    /// <summary>
    /// Transfer state for SFTP Logger
    /// </summary>
    public TransferState State { get; set; }

    private string SourceFileDuringTransfer { get; set; }

    private FileItem OriginalDestinationFileMetadata;
    private string OriginalDestinationFileCopyPath;

    internal async Task<SingleFileTransferResult> TransferSingleFile(CancellationToken cancellationToken)
    {
        OriginalDestinationFileCopyPath = string.Empty;
        var originalDestinationFileExists = DestinationFileExists(DestinationFileWithMacrosExpanded);

        try
        {
            if (originalDestinationFileExists)
            {
                OriginalDestinationFileMetadata = new FileItem(DestinationFileWithMacrosExpanded);
                OriginalDestinationFileCopyPath = Path.Combine(
                    Path.GetTempPath(),
                    $"{Guid.NewGuid()}_{OriginalDestinationFileMetadata.Name}");
                File.Copy(OriginalDestinationFileMetadata.FullPath, OriginalDestinationFileCopyPath);
            }

            _result.TransferredFile = SourceFile.Name;
            _result.TransferredFilePath = SourceFile.FullPath;

            await GetSourceFile(cancellationToken);

            if (originalDestinationFileExists)
            {
                DestinationFile = new FileItem(DestinationFileWithMacrosExpanded);

                switch (BatchContext.Destination.Action)
                {
                    case DestinationAction.Append:
                        await AppendDestinationFile(cancellationToken);

                        break;
                    case DestinationAction.Overwrite:
                        await PutDestinationFile(removeExisting: true, cancellationToken);

                        break;
                    case DestinationAction.Error:
                        throw new DestinationFileExistsException(Path.GetFileName(DestinationFileWithMacrosExpanded));
                }
            }
            else
            {
                await PutDestinationFile(false, cancellationToken);
            }

            if (BatchContext.Options.PreserveLastModified) RestoreModified();

            await ExecuteSourceOperation(cancellationToken);

            _logger.LogTransferSuccess(this, BatchContext);
            CleanUpSourceFiles();
            CleanUpDestinationFiles();
        }
        catch (Exception ex)
        {
            var (restoreSourceSuccess, sourceFileRestoreMessage) = RestoreSourceFileAfterErrorIfItWasRenamed();
            HandleTransferError(ex, sourceFileRestoreMessage);

            if (restoreSourceSuccess)
            {
                CleanUpSourceFiles();
            }
            else
            {
                _logger.NotifyInformation(BatchContext, "Restoring the source file failed, hence it won’t be deleted.");
            }

            var (restoreDestinationSuccess, destinationFileRestoreMessage) = RestoreDestinationFile();
            HandleTransferError(ex, destinationFileRestoreMessage);

            if (restoreDestinationSuccess)
            {
                CleanUpDestinationFiles();
            }
            else
            {
                _logger.NotifyInformation(
                    BatchContext,
                    "Restoring the destination file failed, hence it won’t be deleted.");
            }
        }

        _result.DestinationFilePath = DestinationFileWithMacrosExpanded;

        return _result;
    }

    private static bool FileDefinedAndExists(string path)
    {
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    private async Task GetSourceFile(CancellationToken cancellationToken)
    {
        if (BatchContext.Options.RenameSourceFileBeforeTransfer)
            await RenameSourceFile(cancellationToken);
        else
            SourceFileDuringTransfer = SourceFile.FullPath;

        SetCurrentState(TransferState.GetFile,
            $"Downloading temporary source file {Path.GetFileName(SourceFileDuringTransfer)} to local temp folder {WorkFileInfo.WorkFileDir}");

        using var fs = File.Open(Path.Combine(WorkFileInfo.WorkFileDir, WorkFileInfo.SafeTempFileName),
            FileMode.Create);
        var asynch = Client.BeginDownloadFile(SourceFileDuringTransfer, fs);

        var sftpAsynch = asynch as SftpDownloadAsyncResult;

        while (sftpAsynch != null && !sftpAsynch.IsCompleted)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                sftpAsynch.IsDownloadCanceled = true;
                _logger.NotifyError(BatchContext, "Operation was cancelled from UI.", new OperationCanceledException());
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        Client.EndDownloadFile(asynch);
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

    private async Task RenameSourceFile(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(SourceFileDuringTransfer))
        {
            await Client.RenameFileAsync(SourceFileDuringTransfer, SourceFile.FullPath, cancellationToken);

            return;
        }

        var uniqueFileName = Util.CreateUniqueFileName(BatchContext.Options.SourceFileExtension);
        var directory = Path.GetDirectoryName(SourceFile.FullPath);

        SourceFileDuringTransfer = SourceFile.FullPath.Contains('/')
            ? Path.Combine(directory, uniqueFileName).Replace("\\", "/")
            : Path.Combine(directory, uniqueFileName);

        SetCurrentState(TransferState.RenameSourceFileBeforeTransfer,
            $"Renaming source file {SourceFile.Name} to temporary file name {uniqueFileName} before transfer.");
        await Client.RenameFileAsync(SourceFile.FullPath, SourceFileDuringTransfer, cancellationToken);
        _logger.NotifyInformation(BatchContext,
            $"FILE RENAME: Source file {SourceFile.Name} renamed to target {uniqueFileName}.");
    }

    private async Task AppendDestinationFile(CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(WorkFileInfo.WorkFileDir, WorkFileInfo.SafeTempFileName);

        if (BatchContext.Options.RenameDestinationFileDuringTransfer)
            await RenameDestinationFile(cancellationToken);

        await Append(filePath, BatchContext.Destination.AddNewLine, cancellationToken);

        if (BatchContext.Options.RenameDestinationFileDuringTransfer)
            await RenameDestinationFile(cancellationToken);
    }

    private async Task RenameDestinationFile(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
        {
            SetCurrentState(TransferState.RenameDestinationFile,
                $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {DestinationFile.Name}.");
            await FileOperations.MoveAsync(DestinationFileDuringTransfer, DestinationFile.FullPath, cancellationToken);
            _logger.NotifyInformation(BatchContext,
                $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {DestinationFile.Name}.");
        }
        else
        {
            DestinationFileDuringTransfer = Path.Combine(Path.GetDirectoryName(DestinationFile.FullPath),
                Util.CreateUniqueFileName(BatchContext.Options.DestinationFileExtension));
            SetCurrentState(TransferState.RenameDestinationFile,
                $"Renaming destination file {DestinationFile.Name} to temporary file name {Path.GetFileName(DestinationFileDuringTransfer)} during transfer.");
            await FileOperations.MoveAsync(DestinationFile.FullPath, DestinationFileDuringTransfer, cancellationToken);
            _logger.NotifyInformation(BatchContext,
                $"FILE RENAME: Destination file {DestinationFile.Name} renamed to target {Path.GetFileName(DestinationFileDuringTransfer)}.");
        }
    }

    private async Task Append(string sourcePath, bool addNewLine, CancellationToken cancellationToken)
    {
        SetCurrentState(
            TransferState.AppendToDestinationFile,
            $"Appending file {Path.GetFileName(SourceFileDuringTransfer)} to existing file {DestinationFile.Name}.");

        // If destination rename during transfer is enabled, use that instead
        var path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
            ? DestinationFileDuringTransfer
            : DestinationFile.FullPath;

        await FileOperations.AppendAsync(sourcePath, path, addNewLine, cancellationToken);
        _logger.NotifyInformation(BatchContext, $"FILE APPEND: Source file appended to target {DestinationFile.Name}.");
    }

    private async Task PutDestinationFile(bool removeExisting, CancellationToken cancellationToken)
    {
        var doRename = BatchContext.Options.RenameDestinationFileDuringTransfer;

        DestinationFileDuringTransfer = doRename
            ? Path.Combine(Path.GetDirectoryName(DestinationFileWithMacrosExpanded),
                Util.CreateUniqueFileName(BatchContext.Options.DestinationFileExtension))
            : DestinationFileWithMacrosExpanded;

        var helper = doRename ? "temporary " : string.Empty;
        SetCurrentState(
            TransferState.PutFile,
            $"Downloading {helper}destination file {Path.GetFileName(DestinationFileDuringTransfer)}.");

        await FileOperations.CopyAsync(
            Path.Combine(WorkFileInfo.WorkFileDir, WorkFileInfo.SafeTempFileName),
            DestinationFileDuringTransfer, removeExisting, cancellationToken);

        _logger.NotifyInformation(BatchContext,
            $"FILE COPY {SourceFileDuringTransfer} to {DestinationFileDuringTransfer}.");

        if (doRename)
        {
            if (removeExisting)
            {
                SetCurrentState(
                    TransferState.DeleteDestinationFile,
                    $"Deleting destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} that is to be overwritten.");

                File.Delete(DestinationFileWithMacrosExpanded);
                _logger.NotifyInformation(BatchContext,
                    $"FILE DELETE: Destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} deleted.");
            }

            SetCurrentState(
                TransferState.RenameDestinationFile,
                $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {Path.GetFileName(DestinationFileWithMacrosExpanded)}.");

            await FileOperations.MoveAsync(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded,
                cancellationToken);
            _logger.NotifyInformation(BatchContext,
                $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {Path.GetFileName(DestinationFileWithMacrosExpanded)}.");
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
        _logger.NotifyInformation(BatchContext, $"SET MODIFIED {date:dd.MM.yyyy hh:mm:ss}");
    }

    private async Task ExecuteSourceOperation(CancellationToken cancellationToken)
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;
        ISftpFile file;

        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Delete:
                SetCurrentState(TransferState.SourceOperationDelete,
                    $"Deleting source file {Path.GetFileName(SourceFile.FullPath)} after transfer.");
                await Client.DeleteFileAsync(filePath, cancellationToken);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Source file {filePath} deleted.");

                break;

            case SourceOperation.Nothing:
                if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                {
                    SetCurrentState(
                        TransferState.RestoreSourceFile,
                        $"Restoring source file from temporary {Path.GetFileName(SourceFileDuringTransfer)} to the original name {Path.GetFileName(SourceFile.FullPath)}.");
                    await Client.RenameFileAsync(filePath, SourceFile.FullPath, cancellationToken);
                    _logger.NotifyInformation(BatchContext,
                        $"FILE RENAME: Temporary file {SourceFileDuringTransfer} restored to target {SourceFile.FullPath}.");
                }

                break;
            case SourceOperation.Move:
                file = Client.Get(filePath);
                var moveToPath =
                    _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);
                SetCurrentState(TransferState.SourceOperationMove,
                    $"Moving source file {SourceFile.FullPath} to {moveToPath}.");

                if (!Client.Exists(moveToPath))
                {
                    var msg =
                        $"Operation failed: Source file {SourceFile.Name} couldn't be moved to given directory {moveToPath} because the directory didn't exist.";
                    _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in moving the source file."));
                    _result.ErrorMessages.Add($"Failure in source operation: {msg}");
                }

                var destFileName = Path.Combine(moveToPath, SourceFile.Name).Replace("\\", "/");

                if (Client.Exists(destFileName))
                    throw new Exception(
                        $"Failure in source operation: File {Path.GetFileName(destFileName)} exists in move to directory.");

                try
                {
                    file.MoveTo(destFileName);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failure in source operation: {ex.GetType().Name}", ex);
                }

                if (!Client.Exists(destFileName))
                {
                    throw new Exception(
                        "Failure in source operation: Source file couldn't be moved to move to directory.");
                }

                _logger.NotifyInformation(BatchContext,
                    $"Source file {SourceFileDuringTransfer} moved to target {destFileName}.");
                WorkFile = new FileItem(file);

                if (SourceFile.FullPath == null)
                {
                    _logger.NotifyInformation(BatchContext,
                        "Source end point returned null as the moved file. It should return the name of the moved file.");
                }

                break;
            case SourceOperation.Rename:
                file = Client.Get(filePath);
                var path = string.IsNullOrEmpty(Path.GetDirectoryName(BatchContext.Source.FileNameAfterTransfer))
                    ? Path.GetDirectoryName(SourceFile.FullPath).Replace("\\", "/")
                    : Path.GetDirectoryName(_renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath,
                        BatchContext.Source.FileNameAfterTransfer)).Replace("\\", "/");

                if (!Client.Exists(path))
                {
                    var msg =
                        $"Operation failed: Source file {SourceFile.Name} couldn't be moved to given directory {path} because the directory didn't exist.";
                    _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in moving the source file."));
                    _result.ErrorMessages.Add($"Failure in source operation: {msg}");
                }

                var rename = Path.Combine(path,
                    _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.Name,
                        Path.GetFileName(BatchContext.Source.FileNameAfterTransfer))).Replace("\\", "/");
                SetCurrentState(TransferState.SourceOperationRename,
                    $"Renaming source file {SourceFile.FullPath} to {rename}.");

                try
                {
                    file.MoveTo(rename);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failure in source operation: {ex.GetType().Name}", ex);
                }

                _logger.NotifyInformation(BatchContext,
                    $"FILE RENAME: Source file {SourceFileDuringTransfer} renamed to target {rename}.");

                if (!Client.Exists(rename))
                {
                    var msg =
                        $"Operation failed: Source file {SourceFile.Name} couldn't be renamed to given name {Path.GetFileName(rename)}";
                    _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in renaming source file."));
                    _result.ErrorMessages.Add($"Failure in source operation: {msg}");
                }

                file = Client.Get(rename);
                WorkFile = new FileItem(file);

                if (WorkFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext,
                        "Source end point returned null as the renamed file. It should return the name of the renamed file.");

                break;
        }
    }

    private void CleanUpSourceFiles()
    {
        var temporarySourceFile = Path.Combine(WorkFileInfo.WorkFileDir, WorkFileInfo.SafeTempFileName);
        SetCurrentState(TransferState.CleanUpFiles, $"Checking if temporary source file {temporarySourceFile} exists.");
        var exists = !string.IsNullOrEmpty(temporarySourceFile) && File.Exists(temporarySourceFile);
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {temporarySourceFile}: {exists}");

        if (!exists) return;
        SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary source file {temporarySourceFile}.");
        TryToRemoveLocalTempFile(temporarySourceFile);
    }

    private void CleanUpDestinationFiles()
    {
        SetCurrentState(
            TransferState.CleanUpFiles,
            $"Checking if temporary destination file {DestinationFileDuringTransfer} exists.");
        var exists = !string.IsNullOrEmpty(DestinationFileDuringTransfer) &&
                     File.Exists(DestinationFileDuringTransfer) &&
                     BatchContext.Options.RenameDestinationFileDuringTransfer;
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {DestinationFileDuringTransfer}: {exists}");

        if (!exists) return;
        SetCurrentState(
            TransferState.CleanUpFiles,
            $"Removing temporary destination file {DestinationFileDuringTransfer}.");
        TryToRemoveDestinationTempFile();
    }

    private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
    {
        _result.Success = false; // the routine instance should be marked as failed if even one transfer fails
        var errorMessage =
            $"Failure in {State}: File '{SourceFile.Name}' could not be transferred to '{DestinationFileWithMacrosExpanded}'. Error: {exception.Message}.";
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
            SetCurrentState(TransferState.RemoveTemporaryDestinationFile,
                $"Removing temporary destination file {DestinationFileDuringTransfer}.");
            File.Delete(DestinationFileDuringTransfer);
            File.Delete(OriginalDestinationFileCopyPath);
            _logger.NotifyInformation(BatchContext,
                $"FILE DELETE: Temporary destination file {DestinationFileDuringTransfer} removed.");
        }
        catch (Exception ex)
        {
            _logger.NotifyError(BatchContext,
                $"Could not clean up temporary destination file '{DestinationFileDuringTransfer}': {ex.Message}.", ex);
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

    private (bool, string) RestoreSourceFileAfterErrorIfItWasRenamed()
    {
        // Check that the connection is alive and if not try to connect again
        if (!Client.IsConnected)
            Client.Connect();

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
                    if (BatchContext.Source.Operation == SourceOperation.Rename && !Client.Exists(SourceFile.FullPath))
                        Client.RenameFile(SourceFileDuringTransfer, SourceFile.FullPath);
                    if (BatchContext.Options.RenameSourceFileBeforeTransfer && !Client.Exists(SourceFile.FullPath))
                        Client.RenameFile(SourceFileDuringTransfer, SourceFile.FullPath);

                    return (true, "[Source file restored.]");
                }
            }
            catch (Exception ex)
            {
                var message =
                    $"Could not restore original source file '{Path.GetFileName(SourceFile.FullPath)}' from temporary file '{Path.GetFileName(SourceFileDuringTransfer)}'. Error: {ex.Message}.";

                _logger.NotifyError(BatchContext, message, ex);

                return (false, $"[{message}]");
            }
        }

        return (true, string.Empty);
    }

    private void RestoreSourceFileIfItWasMoved()
    {
        var filePath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);

        var destFileName = Path.Combine(filePath, SourceFile.Name);
        destFileName = filePath.Contains('/') ? destFileName.Replace("\\", "/") : destFileName;

        if (!Client.Exists(destFileName)) return;

        var file = Client.Get(destFileName);

        var restorePath = Path.Combine(BatchContext.Source.Directory, SourceFile.Name);
        restorePath = BatchContext.Source.Directory.Contains('/') ? restorePath.Replace("\\", "/") : restorePath;

        file.MoveTo(restorePath);

        var path = file.FullName;

        if (!Client.Exists(path)) throw new ArgumentException("Failure in restoring moved source file.");
    }

    private (bool, string) RestoreDestinationFile()
    {
        string message;
        bool success = true;

        try
        {
            if (OriginalDestinationFileMetadata is null)
            {
                File.Delete(DestinationFileWithMacrosExpanded);
                message = "Destination file deleted.";
            }
            else
            {
                File.Copy(OriginalDestinationFileCopyPath, DestinationFileWithMacrosExpanded, true);
                File.SetLastWriteTime(DestinationFileWithMacrosExpanded, OriginalDestinationFileMetadata.Modified);
                message = "Destination file restored.";
            }
        }
        catch (Exception e)
        {
            message =
                $"Could not restore original destination file '{Path.GetFileName(DestinationFileWithMacrosExpanded)}' from temporary file '{Path.GetFileName(OriginalDestinationFileCopyPath)}'. Error: {e.Message}.";
            success = false;
        }

        return (success, message);
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
        public DestinationFileExistsException(string fileName)
            : base($"Unable to transfer file. Destination file already exists: {fileName}.")
        {
        }
    }
}
