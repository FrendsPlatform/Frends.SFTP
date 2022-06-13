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
            else
            {
                PutDestinationFile();
            }

            if (BatchContext.Options.PreserveLastModified)
                RestoreModified();

            ExecuteSourceOperation();
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

    private bool DestinationFileExists(string filename)
    {
        Trace(
            TransferState.CheckIfDestinationFileExists,
            "Checking if destination file {0} exists",
            Path.GetFileName(filename));
        return File.Exists(filename);
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

        Trace(TransferState.RenameSourceFileBeforeTransfer, "Renaming source file {0} to temporary file name {1} before transfer", SourceFile.Name, uniqueFileName);

        Client.RenameFile(SourceFile.FullPath, SourceFileDuringTransfer);
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
            File.Move(DestinationFileDuringTransfer, DestinationFile.FullPath);
        else
        {
            var path = (!string.IsNullOrEmpty(BatchContext.Info.WorkDir)) ? BatchContext.Info.WorkDir : Path.GetDirectoryName(DestinationFile.FullPath);
            DestinationFileDuringTransfer = Path.Combine(path, Util.CreateUniqueFileName());
            Trace(TransferState.RenameDestinationFile, "Renaming destination file {0} to temporary file name {1} during transfer", Path.GetFileName(DestinationFileWithMacrosExpanded), Path.GetFileName(DestinationFileDuringTransfer));
            File.Move(DestinationFile.FullPath, DestinationFileDuringTransfer);
        }       
    }

    private void Append(string[] content, Encoding encoding)
    {
        Trace(
            TransferState.AppendToDestinationFile,
            "Appending file {0} to existing file {1}",
            SourceFile.Name,
            DestinationFile.Name);

        // Determine path to use to the destination file.
        var path = (BatchContext.Destination.Directory.Contains("/"))
            ? BatchContext.Destination.Directory + "/"
            : BatchContext.Destination.Directory;

        // If destination rename during transfer is enabled, use that instead 
        path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer)) 
            ? Path.Combine(path, Path.GetFileName(DestinationFileDuringTransfer))
            : Path.Combine(path, DestinationFile.Name);

        File.AppendAllLines(path, content);
    }

    private string[] GetSourceFileContent(string filePath, Encoding encoding)
    {
        var result = new List<string>();
        result.Add("\n");
        string[] content;
        content = Client.ReadAllLines(filePath, encoding);
            
        foreach (var line in content)
        {
            result.Add(line);
        }

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
        Trace(
            TransferState.PutFile,
            "Downloading {0}destination file {1}",
            doRename ? "temporary " : string.Empty,
            Path.GetFileName(DestinationFileDuringTransfer));
        var fileMode = removeExisting ? FileMode.Truncate : FileMode.Create;
        using (var fs = File.Open(DestinationFileDuringTransfer, fileMode))
        {
            Client.DownloadFile(SourceFileDuringTransfer, fs);
        }

        if (doRename)
        {
            if (removeExisting)
            {
                Trace(
                    TransferState.DeleteDestinationFile,
                    "Deleting destination file {0} that is to be overwritten",
                    Path.GetFileName(DestinationFileWithMacrosExpanded));

                File.Delete(DestinationFileWithMacrosExpanded);
            }

            Trace(
                TransferState.RenameDestinationFile,
                "Renaming temporary destination file {0} to target file {1}",
                Path.GetFileName(DestinationFileDuringTransfer),
                Path.GetFileName(DestinationFileWithMacrosExpanded));

            File.Move(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
        }
    }

    /// <summary>
    /// Restores the LastWriteTime stamp to the destination file.
    /// </summary>
    private void RestoreModified()
    {
        DestinationFile.Modified = Client.GetAttributes(SourceFile.FullPath).LastWriteTime;
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
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ExecuteSourceOperation()
    {
        var filePath = string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer;
        switch (BatchContext.Source.Operation)
        {
            case SourceOperation.Move:
                var moveToPath = _renamingPolicy.ExpandDirectoryForMacros(BatchContext.Source.DirectoryToMoveAfterTransfer);
                Trace(TransferState.SourceOperationMove, "Moving source file {0} to {1}", SourceFile.FullPath, moveToPath);
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

                if (SourceFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
                break;

            case SourceOperation.Rename:
                var rename = Path.Combine(Path.GetDirectoryName(SourceFile.FullPath), _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer));
                rename = SourceFile.FullPath.Contains('/') ? rename.Replace("\\", "/") : rename; 
                Trace(TransferState.SourceOperationRename, "Renaming source file {0} to {1}", Path.GetFileName(SourceFile.FullPath), Path.GetFileName(rename));
                Client.RenameFile(filePath, rename);

                if (SourceFile.FullPath == null)
                    _logger.NotifyInformation(BatchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
                break;

            case SourceOperation.Delete:
                Trace(TransferState.SourceOperationDelete, "Deleting source file {0} after transfer", Path.GetFileName(SourceFile.FullPath));
                Client.DeleteFile(filePath);
                break;

            case SourceOperation.Nothing:
                if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                {
                    Trace(
                        TransferState.RestoreSourceFile,
                        "Restoring source file from {0} to the original name {1}",
                        Path.GetFileName(SourceFileDuringTransfer),
                        Path.GetFileName(SourceFile.FullPath));

                    Client.RenameFile(filePath, SourceFile.FullPath);
                }
                break;
        }
    }

    private void CleanUpFiles()
    {
        if (BatchContext.Options.RenameSourceFileBeforeTransfer && !Path.GetFileName(SourceFileDuringTransfer).Equals(SourceFile.FullPath))
        {
            Trace(TransferState.CleanUpFiles, "Removing temporary file {0}", SourceFileDuringTransfer);
            TryToRemoveSourceTempFile(SourceFileDuringTransfer);
        }


        if (BatchContext.Options.RenameDestinationFileDuringTransfer && !Path.GetFileName(DestinationFileDuringTransfer).Equals(Path.GetFileName(DestinationFileWithMacrosExpanded)))
        {
            Trace(TransferState.CleanUpFiles, "Removing temporary file {0}", Path.GetFileName(DestinationFileDuringTransfer));
            TryToRemoveDestinationTempFile();
        }
    }

    private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
    {
        _result.Success = false; // the routine instance should be marked as failed if even one transfer fails
        var errorMessage = string.Format("Failure in {0}: File '{1}' could not be transferred to '{2}'. Error: {3}", State, SourceFile.Name, BatchContext.Destination.Directory, exception.Message);
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
                Trace(TransferState.RemoveTemporaryDestinationFile, "Removing temporary destination file {0}", DestinationFileDuringTransfer);
                File.Delete(DestinationFileDuringTransfer);
            }
        }
        catch (Exception ex)
        {
            _logger.NotifyError(BatchContext, string.Format("Could not clean up temporary destination file '{0}': {1}", DestinationFileDuringTransfer, ex.Message), ex);
        }
    }

    private void TryToRemoveSourceTempFile(string filePath)
    {
        try
        {
            if (FileDefinedAndExists(filePath)) Client.Delete(filePath);
        }
        catch (Exception e)
        {
            _logger.NotifyError(BatchContext, string.Format("Could not clean up local file '{0}'", Path.GetFileName(filePath)), e);
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
                var message = string.Format(
                    "Could not restore original source file '{0}' from temporary file '{1}'. Error: {2}.",
                    Path.GetFileName(SourceFile.FullPath),
                    Path.GetFileName(SourceFileDuringTransfer),
                    ex.Message);

                _logger.NotifyError(BatchContext, message, ex);
                return string.Format("[{0}]", message);
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
                    File.Move(DestinationFileDuringTransfer, DestinationFileWithMacrosExpanded);
                    return string.Empty;
                }
            } catch (Exception ex)
            {
                var message = string.Format(
                    "Could not restore original destination file '{0}' from temporary file '{1}'. Error: {2}.",
                    Path.GetFileName(DestinationFileWithMacrosExpanded),
                    Path.GetFileName(DestinationFileDuringTransfer),
                    ex.Message);

                _logger.NotifyError(BatchContext, message, ex);
                return string.Format("[{0}]", message);
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

    /// <summary>
    /// Handles logging of actions.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    private void Trace(TransferState state, string format, params object[] args)
    {
        State = state;
        _logger.NotifyTrace(string.Format("{0}: {1}", state, string.Format(format, args)));
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
        public DestinationFileExistsException(string fileName) : base(String.Format("Unable to transfer file. Destination file already exists: {0}", fileName)) { }
    }
}

