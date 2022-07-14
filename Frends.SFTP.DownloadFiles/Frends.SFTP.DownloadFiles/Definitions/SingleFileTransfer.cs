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
        BatchContext = context;

        DestinationFileWithMacrosExpanded = Path.Combine(
            renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory), 
            renamingPolicy.CreateRemoteFileName(
                file.Name,
                context.Destination.FileName));

        _result = new SingleFileTransferResult { Success = true };
    }

    public DateTime FileTransferStartTime { get; set; }

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
            if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                RenameSourceFile();
            else 
                SourceFileDuringTransfer = SourceFile.FullPath;

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

            ExecuteSourceOperation();
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

    private bool DestinationFileExists(string filename)
    {
        SetCurrentState(
            TransferState.CheckIfDestinationFileExists,
            $"Checking if destination file {Path.GetFileName(filename)} exists.");
        var exists = File.Exists(filename);
        _logger.NotifyInformation(BatchContext, $"FILE EXISTS {filename}: {exists}.");
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
        var workdir = (!string.IsNullOrEmpty(BatchContext.Info.WorkDir) ? BatchContext.Info.WorkDir : Path.GetDirectoryName(SourceFile.FullPath));
        workdir = SourceFile.FullPath.Contains('/') ? workdir.Replace("\\", "/") : workdir;
        SourceFileDuringTransfer = (workdir.Contains('/')) ? Path.Combine(workdir, uniqueFileName).Replace("\\", "/") : Path.Combine(workdir, uniqueFileName);

        SetCurrentState(TransferState.RenameSourceFileBeforeTransfer, $"Renaming source file {SourceFile.Name} to temporary file name {uniqueFileName} before transfer.");

        Client.RenameFile(SourceFile.FullPath, SourceFileDuringTransfer);
        _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFile.Name} renamed to target {Path.GetFileName(SourceFileDuringTransfer)}.");
    }

    private void AppendDestinationFile()
    {
        var filePath = (!string.IsNullOrEmpty(SourceFileDuringTransfer)) ? SourceFileDuringTransfer : SourceFile.FullPath;
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
            var path = (!string.IsNullOrEmpty(BatchContext.Info.WorkDir)) ? BatchContext.Info.WorkDir : Path.GetDirectoryName(DestinationFile.FullPath);
            DestinationFileDuringTransfer = Path.Combine(path, Util.CreateUniqueFileName());
            SetCurrentState(TransferState.RenameDestinationFile, $"Renaming destination file {DestinationFile.Name} to temporary file name {Path.GetFileName(DestinationFileDuringTransfer)} during transfer.");
            File.Move(DestinationFile.FullPath, DestinationFileDuringTransfer);
            _logger.NotifyInformation(BatchContext, $"FILE RENAME: Destination file {DestinationFile.Name} renamed to target {Path.GetFileName(DestinationFileDuringTransfer)}.");
        }       
    }

    private void Append(string[] content, Encoding encoding)
    {
        SetCurrentState(
            TransferState.AppendToDestinationFile,
            $"Appending file {SourceFile.Name} to existing file {DestinationFile.Name}.");

        // Determine path to use to the destination file.
        var path = (BatchContext.Destination.Directory.Contains('/'))
            ? BatchContext.Destination.Directory + "/"
            : BatchContext.Destination.Directory;

        // If destination rename during transfer is enabled, use that instead 
        path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer)) 
            ? Path.Combine(path, Path.GetFileName(DestinationFileDuringTransfer))
            : Path.Combine(path, DestinationFile.Name);

        File.AppendAllLines(path, content, encoding);
        _logger.NotifyInformation(BatchContext, $"FILE APPEND: Source file appended to target {DestinationFile.Name}.");
    }

    private string[] GetSourceFileContent(string filePath, Encoding encoding)
    {
        var result = new List<string>();
        result.Add("\n");
        string[] content;
        content = Client.ReadAllLines(filePath, encoding);
            
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
        using (var fs = File.Open(DestinationFileDuringTransfer, FileMode.Create))
        {
            Client.DownloadFile(SourceFileDuringTransfer, fs);
        }
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
            DestinationFileDuringTransfer = DestinationFileWithMacrosExpanded;
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
        switch (dest.FileContentEncoding)
        {
            case FileEncoding.UTF8:
                return dest.EnableBomForContent ? new UTF8Encoding(true) : new UTF8Encoding(false);
            case FileEncoding.ASCII:
                return Encoding.ASCII;
            case FileEncoding.ANSI:
                return Encoding.Default;
            case FileEncoding.Unicode:
                return Encoding.Unicode;
            case FileEncoding.WINDOWS1252:
                return Encoding.Default;
            case FileEncoding.Other:
                return Encoding.GetEncoding(dest.FileContentEncodingInString);
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{dest.FileContentEncoding}'.");
        }
    }

    private void ExecuteSourceOperation()
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;
        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Move:
                var moveToPath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);
                SetCurrentState(TransferState.SourceOperationMove, $"Moving source file {SourceFile.FullPath} to {moveToPath}.");
                var file = Client.Get(filePath);
                if (!Client.Exists(moveToPath))
                {
                    _logger.NotifyInformation(BatchContext, $"Operation failed: Source couldn't be moved to given directory {moveToPath} because it didn't exist.");
                    break;
                }
                var destFileName = moveToPath.Contains("/") 
                    ? moveToPath + "/" + SourceFile.Name 
                    : Path.Combine(moveToPath, SourceFile.Name);

                file.MoveTo(destFileName);
                _logger.NotifyInformation(BatchContext, $"Source file {SourceFileDuringTransfer} moved to target {destFileName}.");

                if (SourceFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
                break;

            case SourceOperation.Rename:
                var rename = Path.Combine(Path.GetDirectoryName(SourceFile.FullPath), _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer));
                rename = SourceFile.FullPath.Contains('/') ? rename.Replace("\\", "/") : rename; 
                SetCurrentState(TransferState.SourceOperationRename, $"Renaming source file {Path.GetFileName(SourceFile.FullPath)} to {Path.GetFileName(rename)}.");
                Client.RenameFile(filePath, rename);
                _logger.NotifyInformation(BatchContext, $"FILE RENAME: Source file {SourceFileDuringTransfer} renamed to target {rename}.");

                if (SourceFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
                break;

            case SourceOperation.Delete:
                SetCurrentState(TransferState.SourceOperationDelete, $"Deleting source file {Path.GetFileName(SourceFile.FullPath)} after transfer.");
                Client.DeleteFile(filePath);
                _logger.NotifyInformation(BatchContext, $"FILE DELETE: Source file {SourceFile.Name} deleted.");
                break;

            case SourceOperation.Nothing:
                if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                {
                    SetCurrentState(
                        TransferState.RestoreSourceFile,
                        $"Restoring source file from temporary {Path.GetFileName(SourceFileDuringTransfer)} to the original name {Path.GetFileName(SourceFile.FullPath)}.");
                    Client.RenameFile(filePath, SourceFile.FullPath);
                    _logger.NotifyInformation(BatchContext, $"FILE RENAME: Temporary file {SourceFileDuringTransfer} restored to target {SourceFile.Name}.");
                }
                break;
        }
    }

    private void CleanUpFiles()
    {
        if (BatchContext.Options.RenameSourceFileBeforeTransfer && !Path.GetFileName(SourceFileDuringTransfer).Equals(SourceFile.FullPath))
        {
            SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary file {SourceFileDuringTransfer}.");
            TryToRemoveSourceTempFile(SourceFileDuringTransfer);
        }


        if (BatchContext.Options.RenameDestinationFileDuringTransfer && !Path.GetFileName(DestinationFileDuringTransfer).Equals(Path.GetFileName(DestinationFileWithMacrosExpanded)))
        {
            SetCurrentState(TransferState.CleanUpFiles, $"Removing temporary file {Path.GetFileName(DestinationFileDuringTransfer)}.");
            TryToRemoveDestinationTempFile();
        }
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
            if (DestinationFileExists(Path.GetFileName(DestinationFileDuringTransfer)))
            {
                SetCurrentState(TransferState.RemoveTemporaryDestinationFile, $"Removing temporary destination file {DestinationFileDuringTransfer}.");
                File.Delete(DestinationFileDuringTransfer);
            }
        }
        catch (Exception ex)
        {
            _logger.NotifyError(BatchContext, $"Could not clean up temporary destination file '{DestinationFileDuringTransfer}': {ex.Message}.", ex);
        }
    }

    private void TryToRemoveSourceTempFile(string filePath)
    {
        try
        {
            if (FileDefinedAndExists(filePath)) Client.Delete(filePath);
            _logger.NotifyInformation(BatchContext, $"Temporary source file {filePath} removed.");
        }
        catch (Exception e)
        {
            _logger.NotifyError(BatchContext, $"Could not clean up local file '{Path.GetFileName(filePath)}'.", e);
        }
    }

    private bool FileDefinedAndExists(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) && Client.Exists(filePath);
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
                    Client.RenameFile(SourceFileDuringTransfer, SourceFile.FullPath);
                    return "[Source file restored.]";
                }
            }
            catch (Exception ex)
            {
                var message = $"Could not restore original source file '{Path.GetFileName(SourceFile.FullPath)}' from temporary file '{Path.GetFileName(SourceFileDuringTransfer)}'. Error: {ex.Message}.";

                _logger.NotifyError(BatchContext, message, ex);
                return "[{message}]";
            }
        }

        return string.Empty;
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

