﻿using System.Text;
using Renci.SshNet;

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

        DestinationFileWithMacrosExpanded = Path.Combine(destinationDirectory, renamingPolicy.CreateRemoteFileName(
                file.Name,
                context.Destination.FileName));
        if (destinationDirectory.Contains('/')) DestinationFileWithMacrosExpanded = DestinationFileWithMacrosExpanded.Replace("\\", "/"); 
        WorkFileInfo = new WorkFileInfo(file.Name, Path.GetFileName(DestinationFileWithMacrosExpanded), BatchContext.TempWorkDir);

        _result = new SingleFileTransferResult { Success = true };
    }

    public DateTime FileTransferStartTime { get; set; }

    public WorkFileInfo WorkFileInfo { get; set; } 

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
                DestinationFile = new FileItem(Client.Get(DestinationFileWithMacrosExpanded));
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

            var destinationFileRestoreMessage = RestoreDestinationFileAfterErrorIfItWasRenamed(Client);
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

        SetCurrentState(TransferState.GetFile, $"Downloading source file {Path.GetFileName(SourceFileDuringTransfer)} to local temp file {WorkFileInfo.WorkFilePath}");
        File.Copy(SourceFileDuringTransfer, Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer)));
        _logger.NotifyInformation(BatchContext, $"FILE COPY: {SourceFileDuringTransfer} to {WorkFileInfo.WorkFilePath}");
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

    private void RenameSourceFile()
    {
        if (!string.IsNullOrEmpty(SourceFileDuringTransfer))
        {
            File.Move(SourceFileDuringTransfer, SourceFile.FullPath);
            return;
        }

        var uniqueFileName = Util.CreateUniqueFileName();
        var directory = Path.GetDirectoryName(SourceFile.FullPath);
        SourceFileDuringTransfer = Path.Combine(directory, uniqueFileName);

        SetCurrentState(TransferState.RenameSourceFileBeforeTransfer, $"Renaming source file {SourceFile.Name} to temporary file name {uniqueFileName} before transfer");
        File.Move(SourceFile.FullPath, SourceFileDuringTransfer);
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
            Client.RenameFile(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} renamed to target {DestinationFile.Name}.");
        }
        else
        {
            var path = Path.Combine(Path.GetDirectoryName(DestinationFileWithMacrosExpanded), Util.CreateUniqueFileName());
            DestinationFileDuringTransfer = (DestinationFileWithMacrosExpanded.Contains("/")) ? path.Replace("\\", "/") : path;
            SetCurrentState(TransferState.RenameDestinationFile, $"Renaming destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} to temporary file name {Path.GetFileName(DestinationFileDuringTransfer)} during transfer");
            Client.RenameFile(DestinationFileWithMacrosExpanded, DestinationFileDuringTransfer);
        }  
    }

    private void Append(string[] content, Encoding encoding)
    {
        SetCurrentState(
            TransferState.AppendToDestinationFile,
            $"Appending file {SourceFile.Name} to existing file {DestinationFile.Name}");

        // If destination rename during transfer is enabled, use that instead 
        var path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer)) 
            ? DestinationFileDuringTransfer
            : DestinationFileWithMacrosExpanded;

        Client.AppendAllLines(path, content, encoding);
        _logger.NotifyInformation(BatchContext, $"FILE APPEND: Source file appended to target {DestinationFile.Name}.");
    }

    private static string[] GetSourceFileContent(string filePath, Encoding encoding)
    {
        var result = new List<string>();
        result.Add("\n");
        var content = File.ReadAllLines(filePath, encoding);
        foreach (var line in content)
            result.Add(line);

        return result.ToArray();

    }

    /// <summary>
    /// Uploads source file to destination. Overwrites the destination file if enabled.
    /// </summary>
    /// <param name="removeExisting"></param>
    private void PutDestinationFile(bool removeExisting = false)
    {
        var doRename = BatchContext.Options.RenameDestinationFileDuringTransfer;

        DestinationFileDuringTransfer = doRename ? Path.Combine(Path.GetDirectoryName(DestinationFileWithMacrosExpanded), Util.CreateUniqueFileName()): DestinationFileWithMacrosExpanded;
        if (DestinationFileWithMacrosExpanded.Contains('/')) DestinationFileDuringTransfer = DestinationFileDuringTransfer.Replace("\\", "/");

        var helper = doRename ? "temporary " : string.Empty;
        SetCurrentState(
            TransferState.PutFile,
            $"Uploading {helper}destination file { Path.GetFileName(DestinationFileDuringTransfer)} to destination {DestinationFileWithMacrosExpanded}.");

        using (var fs = new FileStream(Path.Combine(WorkFileInfo.WorkFileDir, Path.GetFileName(SourceFileDuringTransfer)), FileMode.Open))
        {
            Client.UploadFile(fs, DestinationFileDuringTransfer, removeExisting);
            _logger.NotifyInformation(BatchContext, $"FILE PUT: Source file {SourceFile.FullPath} uploaded to target {DestinationFileWithMacrosExpanded}.");
        }

        if (!doRename) return;

        if (removeExisting)
        {
            SetCurrentState(
                TransferState.DeleteDestinationFile,
                $"Deleting destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} that is to be overwritten");

            Client.DeleteFile(DestinationFileWithMacrosExpanded);
            _logger.NotifyInformation(BatchContext, $"FILE DELETE: Destination file {Path.GetFileName(DestinationFileWithMacrosExpanded)} deleted.");
        }

        SetCurrentState(
            TransferState.RenameDestinationFile,
            $"Renaming temporary destination file {Path.GetFileName(DestinationFileDuringTransfer)} to target file {Path.GetFileName(DestinationFileWithMacrosExpanded)}");

        Client.RenameFile(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
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
        return;
    }

    private static Encoding GetEncoding(Destination dest)
    {
        switch (dest.FileNameEncoding)
        {
            case FileEncoding.UTF8:
                return dest.EnableBomForFileName ? new UTF8Encoding(true) : new UTF8Encoding(false);
            case FileEncoding.ASCII:
                return Encoding.ASCII;
            case FileEncoding.ANSI:
                return Encoding.Default;
            case FileEncoding.Unicode:
                return Encoding.Unicode;
            case FileEncoding.WINDOWS1252:
                return Encoding.Default;
            case FileEncoding.Other:
                return Encoding.GetEncoding(dest.FileNameEncodingInString);
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{dest.FileContentEncoding}'.");
        }
    }

    private void ExecuteSourceOperationMoveOrRename()
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;

        if (BatchContext.Source.Operation == SourceOperation.Move)
        {
            var moveToPath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);
            SetCurrentState(TransferState.SourceOperationMove, $"Moving source file {SourceFile.FullPath} to {moveToPath}");
            if (!Directory.Exists(moveToPath))
            {
                var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be moved to given directory {moveToPath} because the directory didn't exist.";
                _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in moving the source file."));
                _result.ErrorMessages.Add($"Failure in source operation: {msg}");
            }

            var destFileName = moveToPath.Contains('/')
                ? moveToPath + "/" + SourceFile.Name
                : Path.Combine(moveToPath, SourceFile.Name);

            try { File.Move(filePath, destFileName); }
            catch (Exception ex) { throw new Exception($"Failure in source operation: {ex.Message}"); }

            _logger.NotifyInformation(BatchContext, $"FILE MOVE: Source file {SourceFileDuringTransfer} moved to target {destFileName}.");
            SourceFile = new FileItem(destFileName);

            if (SourceFile.FullPath == null)
                _logger.NotifyInformation(BatchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
        }
        else if (BatchContext.Source.Operation == SourceOperation.Rename)
        {
            var renameToPath = Path.Combine(Path.GetDirectoryName(SourceFile.FullPath), _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer));
            SetCurrentState(TransferState.SourceOperationRename, $"Renaming source file {Path.GetFileName(SourceFile.FullPath)} to {renameToPath}");
            
            File.Move(filePath, renameToPath);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFileDuringTransfer} renamed to target {renameToPath}.");

            SourceFile = new FileItem(renameToPath);
            if (!File.Exists(renameToPath))
            {
                var msg = $"Operation failed: Source file {SourceFile.Name} couldn't be renamed to given name {Path.GetFileName(renameToPath)}";
                _logger.NotifyError(BatchContext, msg, new ArgumentException("Failure in renaming source file."));
                _result.ErrorMessages.Add($"Failure in source operation: {msg}");
            }

            if (SourceFile.FullPath == null)
                _logger.NotifyInformation(BatchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
        }
    }

    private void ExecuteSourceOperationNothingOrDelete()
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;
        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Delete:
                SetCurrentState(TransferState.SourceOperationDelete, $"Deleting source file {Path.GetFileName(SourceFile.FullPath)} after transfer");
                File.Delete(filePath);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Source file {filePath} deleted.");
                break;

            case SourceOperation.Nothing:
                if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                {
                    SetCurrentState(
                        TransferState.RestoreSourceFile,
                        $"Restoring source file from {Path.GetFileName(SourceFileDuringTransfer)} to the original name {Path.GetFileName(SourceFile.FullPath)}");

                    File.Move(filePath, SourceFile.FullPath);
                    _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary file {SourceFileDuringTransfer} restored to target {SourceFile.FullPath}.");
                }
                break;
        }
    }

    private void CleanUpFiles()
    {
        SetCurrentState(TransferState.CleanUpFiles, $"Checking if temporary source file {WorkFileInfo.WorkFilePath} exists.");
        var exists = !string.IsNullOrEmpty(WorkFileInfo.WorkFilePath) && File.Exists(WorkFileInfo.WorkFilePath);
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {WorkFileInfo.WorkFilePath}: {exists}");
        if (exists)
        {
            SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary source file {WorkFileInfo.WorkFilePath}.");
            TryToRemoveLocalTempFile(WorkFileInfo.WorkFilePath);
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
                    File.Move(SourceFileDuringTransfer, SourceFile.FullPath);
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

    private string RestoreDestinationFileAfterErrorIfItWasRenamed(SftpClient client)
    {
        if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
        {
            try
            {
                if (BatchContext.Options.RenameDestinationFileDuringTransfer)
                {
                    client.RenameFile(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
                    return string.Empty;
                }
            } catch (Exception ex)
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

