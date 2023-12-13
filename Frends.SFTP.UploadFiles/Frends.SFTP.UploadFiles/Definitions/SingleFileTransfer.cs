using System.Text;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions;

internal class SingleFileTransfer
{
    private readonly RenamingPolicy _renamingPolicy;
    private readonly ISFTPLogger _logger;
    private readonly SingleFileTransferResult _result;

    public SingleFileTransfer(FileItem file, string destinationDirectory, BatchContext context, SftpClient client, RenamingPolicy renamingPolicy, ISFTPLogger logger)
    {
        _renamingPolicy = renamingPolicy;
        _logger = logger;

        FileTransferStartTime = DateTime.UtcNow;
        Client = client;
        SourceFile = file;
        BatchContext = context;

        DestinationFileWithMacrosExpanded = RenamingPolicy.CanonizeAndCheckPath(Path.Combine(destinationDirectory, renamingPolicy.CreateRemoteFileName(
                file.Name,
                context.Destination.FileName)));
        if (destinationDirectory.Contains('/')) DestinationFileWithMacrosExpanded = DestinationFileWithMacrosExpanded.Replace("\\", "/");
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
    internal BatchContext BatchContext { get; set; }

    /// <summary>
    /// Transfer state for SFTP Logger
    /// </summary>
    internal TransferState State { get; set; }

    internal async Task<SingleFileTransferResult> TransferSingleFile(CancellationToken cancellationToken)
    {
        try
        {
            _result.TransferredFile = SourceFile.Name;
            _result.TransferredFilePath = SourceFile.FullPath;

            await GetSourceFile(cancellationToken);

            await ExecuteSourceOperationRenameMove(cancellationToken);

            if (BatchContext.Options.AssumeFileExistence)
            {
                await PutDestinationFile(Client, removeExisting: true, cancellationToken);
            }
            else if (DestinationFileExists(DestinationFileWithMacrosExpanded))
            {
                DestinationFile = new FileItem(Client.Get(DestinationFileWithMacrosExpanded));
                switch (BatchContext.Destination.Action)
                {
                    case DestinationAction.Append:
                        await AppendDestinationFile(cancellationToken);
                        break;
                    case DestinationAction.Overwrite:
                        await PutDestinationFile(Client, removeExisting: true, cancellationToken);
                        break;
                    case DestinationAction.Error:
                        throw new DestinationFileExistsException(Path.GetFileName(DestinationFileWithMacrosExpanded));
                }
            }
            else
            {
                await PutDestinationFile(Client, removeExisting: false, cancellationToken);
            }

            if (BatchContext.Options.PreserveLastModified) RestoreModified();

            await ExecuteSourceOperationNothingDelete(cancellationToken);

            _logger.LogTransferSuccess(this, BatchContext);
            CleanUpFiles();
        }
        catch (Exception ex)
        {
            var sourceFileRestoreMessage = RestoreSourceFileAfterError();
            HandleTransferError(ex, sourceFileRestoreMessage);

            var destinationFileRestoreMessage = RestoreDestinationFileAfterErrorIfItWasRenamed(Client);
            if (!string.IsNullOrEmpty(destinationFileRestoreMessage))
                HandleTransferError(ex, destinationFileRestoreMessage);
        }
        _result.TransferredDestinationFilePath = DestinationFileWithMacrosExpanded;
        return _result;
    }

    private async Task GetSourceFile(CancellationToken cancellationToken)
    {
        if (BatchContext.Options.RenameSourceFileBeforeTransfer)
            await RenameSourceFile(cancellationToken);
        else
            SourceFileDuringTransfer = SourceFile.FullPath;

        SetCurrentState(TransferState.GetFile, $"Downloading source file {Path.GetFileName(SourceFileDuringTransfer)} to local temp file {WorkFileInfo.WorkFileDir}");
        await FileOperations.CopyAsync(SourceFileDuringTransfer, Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer)), false, cancellationToken);
        _logger.NotifyInformation(BatchContext, $"FILE COPY: {SourceFileDuringTransfer} to {WorkFileInfo.WorkFileDir}");
    }

    private bool DestinationFileExists(string path)
    {
        SetCurrentState(
            TransferState.CheckIfDestinationFileExists,
            $"Checking if destination file {path} exists");
        var exists = Client.Exists(path);
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {path}: {exists}.");
        return exists;
    }

    private async Task RenameSourceFile(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(SourceFileDuringTransfer))
        {
            await FileOperations.MoveAsync(SourceFileDuringTransfer, SourceFile.FullPath, true, cancellationToken);
            return;
        }

        var uniqueFileName = Util.CreateUniqueFileName(BatchContext.Options.SourceFileExtension);
        var directory = Path.GetDirectoryName(SourceFile.FullPath);
        SourceFileDuringTransfer = Path.Combine(directory, uniqueFileName);

        SetCurrentState(TransferState.RenameSourceFileBeforeTransfer, $"Renaming source file {SourceFile.Name} to temporary file name {uniqueFileName} before transfer");
        await FileOperations.MoveAsync(SourceFile.FullPath, SourceFileDuringTransfer, true, cancellationToken);
        _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFile.Name} renamed to target {uniqueFileName}.");
    }

    private async Task AppendDestinationFile(CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer));
        Encoding encoding;
        try
        {
            encoding = Util.GetEncoding(BatchContext.Destination.FileContentEncoding, BatchContext.Destination.FileContentEncodingInString, BatchContext.Destination.EnableBomForContent);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in initializing file content encoding: ", ex);
        }

        if (BatchContext.Options.RenameDestinationFileDuringTransfer)
            await RenameDestinationFile(cancellationToken);

        Append(GetSourceFileContent(filePath, BatchContext.Destination.AddNewLine, encoding), encoding);

        if (BatchContext.Options.RenameDestinationFileDuringTransfer)
            await RenameDestinationFile(cancellationToken);
    }

    private async Task RenameDestinationFile(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
        {
            SetCurrentState(TransferState.RenameDestinationFile, $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {DestinationFile.Name}.");
            await Client.RenameFileAsync(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded, cancellationToken);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {DestinationFile.Name}.");
        }
        else
        {
            var path = Path.Combine(Path.GetDirectoryName(DestinationFileWithMacrosExpanded), Util.CreateUniqueFileName(BatchContext.Options.DestinationFileExtension));
            DestinationFileDuringTransfer = (DestinationFileWithMacrosExpanded.Contains('/')) ? path.Replace("\\", "/") : path;
            SetCurrentState(TransferState.RenameDestinationFile, $"Renaming destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} to temporary file name {Path.GetFileName(DestinationFileDuringTransfer)} during transfer");
            await Client.RenameFileAsync(DestinationFileWithMacrosExpanded, DestinationFileDuringTransfer, cancellationToken);
        }
    }

    private void Append(string content, Encoding encoding)
    {
        SetCurrentState(
            TransferState.AppendToDestinationFile,
            $"Appending file {SourceFile.Name} to existing file {DestinationFile.Name}");

        // If destination rename during transfer is enabled, use that instead 
        var path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
            ? DestinationFileDuringTransfer
            : DestinationFileWithMacrosExpanded;

        Client.AppendAllText(path, content, encoding);
        _logger.NotifyInformation(BatchContext, $"FILE APPEND: Source file appended to target {DestinationFile.Name}.");
    }

    private static string GetSourceFileContent(string filePath, bool addNewLine, Encoding encoding)
    {
        var content = File.ReadAllText(filePath, encoding);
        if (addNewLine)
            content = Environment.NewLine + content;
        return content;
    }

    private async Task PutDestinationFile(SftpClient client, bool removeExisting, CancellationToken cancellationToken)
    {
        var doRename = BatchContext.Options.RenameDestinationFileDuringTransfer;

        DestinationFileDuringTransfer = doRename ? Path.Combine(Path.GetDirectoryName(DestinationFileWithMacrosExpanded), Util.CreateUniqueFileName(BatchContext.Options.DestinationFileExtension)) : DestinationFileWithMacrosExpanded;
        if (DestinationFileWithMacrosExpanded.Contains('/')) DestinationFileDuringTransfer = DestinationFileDuringTransfer.Replace("\\", "/");

        var helper = doRename ? "temporary " : string.Empty;
        SetCurrentState(
            TransferState.PutFile,
            $"Uploading {helper}destination file { Path.GetFileName(DestinationFileDuringTransfer)} to destination {DestinationFileWithMacrosExpanded}.");


        using (var fs = File.OpenRead(Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer))))
        {
            var asynch = Client.BeginUploadFile(fs, DestinationFileDuringTransfer, removeExisting, null, null, null);

            var sftpAsynch = asynch as SftpUploadAsyncResult;

            while (!sftpAsynch.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    sftpAsynch.IsUploadCanceled = true;
                    // This will remove partially uploaded file from the SFTP server.
                    Client.DeleteFile(DestinationFileDuringTransfer);
                    _logger.NotifyError(BatchContext, "Operation was cancelled from UI.", new OperationCanceledException());
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            Client.EndUploadFile(asynch);
        }

        _logger.NotifyInformation(BatchContext, $"FILE PUT: Source file {SourceFile.FullPath} uploaded to target {DestinationFileWithMacrosExpanded}.");


        if (!doRename) return;

        if (removeExisting)
        {
            SetCurrentState(
                TransferState.DeleteDestinationFile,
                $"Deleting destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} that is to be overwritten");

            await client.DeleteFileAsync(DestinationFileWithMacrosExpanded, cancellationToken);
            _logger.NotifyInformation(BatchContext, $"FILE DELETE: Destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} deleted.");
        }

        SetCurrentState(
            TransferState.RenameDestinationFile,
            $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {Path.GetFileName(DestinationFileWithMacrosExpanded)}");

        await client.RenameFileAsync(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded, cancellationToken);
        _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {Path.GetFileName(DestinationFileWithMacrosExpanded)}.");
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
        var fileAttributes = Client.GetAttributes(DestinationFileWithMacrosExpanded);
        fileAttributes.LastWriteTime = date;
        Client.SetAttributes(DestinationFileWithMacrosExpanded, fileAttributes);
    }

    private async Task ExecuteSourceOperationRenameMove(CancellationToken cancellationToken)
    {
        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Move:
                var moveToPath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);
                SetCurrentState(TransferState.SourceOperationMove, $"Moving source file {SourceFile.FullPath} to {moveToPath}");
                if (!Directory.Exists(moveToPath))
                {
                    var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be moved to given directory {moveToPath} because the directory didn't exist.";
                    _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in moving the source file."));
                    _result.ErrorMessages.Add($"Failure in source operation: {msg}");
                }

                var destFileName = Path.Combine(moveToPath, SourceFile.Name);

                try
                {
                    await FileOperations.MoveAsync(SourceFileDuringTransfer, destFileName, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failure in source operation: {ex.GetType().Name}: {ex.Message}", ex);
                }

                _logger.NotifyInformation(BatchContext, $"FILE MOVE: Source file {SourceFileDuringTransfer} moved to target {destFileName}.");
                WorkFile = new FileItem(destFileName);

                if (WorkFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
                break;
            case SourceOperation.Rename:
                var path = string.IsNullOrEmpty(Path.GetDirectoryName(BatchContext.Source.FileNameAfterTransfer))
                    ? Path.GetDirectoryName(SourceFile.FullPath)
                    : Path.GetDirectoryName(_renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer));

                if (!Directory.Exists(path))
                {
                    var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be moved to given directory {path} because the directory didn't exist.";
                    _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in moving the source file."));
                    _result.ErrorMessages.Add($"Failure in source operation: {msg}");
                }

                var renameToPath = Path.Combine(path, _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer));
                SetCurrentState(TransferState.SourceOperationRename, $"Renaming source file {Path.GetFileName(SourceFile.FullPath)} to {renameToPath}");

                File.Move(SourceFileDuringTransfer, renameToPath);
                _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFileDuringTransfer} renamed to target {renameToPath}.");

                WorkFile = new FileItem(renameToPath);
                if (!File.Exists(renameToPath))
                {
                    var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be renamed to given name {Path.GetFileName(renameToPath)}";
                    _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in renaming source file."));
                    _result.ErrorMessages.Add($"Failure in source operation: {msg}");
                }

                if (WorkFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
                break;
        }
    }

    private async Task ExecuteSourceOperationNothingDelete(CancellationToken cancellationToken)
    {
        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Delete:
                SetCurrentState(TransferState.SourceOperationDelete, $"Deleting source file {Path.GetFileName(SourceFile.FullPath)} after transfer");
                File.Delete(SourceFileDuringTransfer);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Source file {SourceFileDuringTransfer} deleted.");
                break;

            case SourceOperation.Nothing:
                if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                {
                    SetCurrentState(
                        TransferState.RestoreSourceFile,
                        $"Restoring source file from {Path.GetFileName(SourceFileDuringTransfer)} to the original name {Path.GetFileName(SourceFile.FullPath)}");

                    await FileOperations.MoveAsync(SourceFileDuringTransfer, SourceFile.FullPath, true, cancellationToken);
                    _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary file {SourceFileDuringTransfer} restored to target {SourceFile.FullPath}.");
                }
                break;
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
        SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary destination file {DestinationFileDuringTransfer}.");
        TryToRemoveDestinationTempFile();
    }

    private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
    {
        _result.Success = false; // the routine instance should be marked as failed if even one transfer fails
        var directory = (DestinationFileWithMacrosExpanded.Contains('/'))
            ? Path.GetDirectoryName(DestinationFileWithMacrosExpanded).Replace("\\", "/")
            : Path.GetDirectoryName(DestinationFileWithMacrosExpanded);
        var errorMessage = $"Failure in {State}: File '{SourceFile.Name}' could not be transferred to '{directory}'. Error: {exception.Message}";
        if (!string.IsNullOrEmpty(sourceFileRestoreMessage)) errorMessage += " " + sourceFileRestoreMessage;

        _result.ErrorMessages.Add(errorMessage);

        _logger.LogTransferFailed(this, BatchContext, errorMessage, exception);
    }

    private void TryToRemoveDestinationTempFile()
    {
        // If DestinationFileNameDuringTransfer is not set,
        // the destination file already exists and DestinationFileExistAction=Error
        if (string.IsNullOrEmpty(DestinationFileDuringTransfer)) return;

        // If RenameDestinationFileDuringTransfer=cd reposFalse, there is no temporary file that could be deleted
        if (!BatchContext.Options.RenameDestinationFileDuringTransfer) return;

        try
        {
            if (DestinationFileExists(Path.GetFileName(DestinationFileDuringTransfer)))
            {
                SetCurrentState(TransferState.RemoveTemporaryDestinationFile, $"Removing temporary destination file {DestinationFileDuringTransfer}");
                Client.DeleteFile(DestinationFileDuringTransfer);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Temporary destination file {DestinationFileDuringTransfer} removed.");
            }
        }
        catch (Exception ex)
        {
            _logger.NotifyError(BatchContext, $"Could not clean up temporary destination file '{DestinationFileDuringTransfer}': {ex.Message}", ex);
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
            _logger.NotifyError(BatchContext, $"Could not clean up local file '{filePath}'", e);
        }
    }

    private static bool FileDefinedAndExists(string path)
    {
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    private string RestoreSourceFileAfterError()
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
                if (ShouldSourceFileBeRestoredOnError() && !File.Exists(SourceFile.FullPath))
                {
                    if (BatchContext.Options.RenameSourceFileBeforeTransfer && File.Exists(SourceFileDuringTransfer))
                        File.Move(SourceFileDuringTransfer, SourceFile.FullPath);
                    if (BatchContext.Source.Operation == SourceOperation.Move && !File.Exists(SourceFile.FullPath))
                        RestoreSourceFileIfItWasMoved();
                    if (BatchContext.Source.Operation == SourceOperation.Rename && !File.Exists(SourceFile.FullPath))
                        if (WorkFile != null)
                            File.Move(WorkFile.FullPath, SourceFile.FullPath);
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
        var filePath = Path.Combine(_renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer), SourceFile.Name);

        if (!File.Exists(filePath)) return;

        File.Move(filePath, SourceFile.FullPath);

        if (!File.Exists(SourceFile.FullPath)) throw new ArgumentException("Failure in restoring moved source file.");
    }

    private string RestoreDestinationFileAfterErrorIfItWasRenamed(SftpClient client)
    {
        if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
        {
            // Check that the connection is alive and if not try to connect again
            if (!Client.IsConnected)
                Client.Connect();

            try
            {
                if (BatchContext.Options.RenameDestinationFileDuringTransfer)
                {
                    if (client.Exists(DestinationFileDuringTransfer))
                        client.RenameFile(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
                }
            }
            catch (Exception ex)
            {
                var message = $"Could not restore original destination file '{Path.GetFileName(DestinationFileWithMacrosExpanded)}' from temporary file '{Path.GetFileName(DestinationFileDuringTransfer)}'. Error: {ex.Message}.";

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
        public DestinationFileExistsException(string fileName) : base($"Unable to transfer file. Destination file already exists: {fileName}") { }
    }
}

