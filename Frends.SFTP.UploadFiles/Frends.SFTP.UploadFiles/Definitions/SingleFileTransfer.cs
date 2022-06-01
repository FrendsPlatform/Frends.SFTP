using System.Text;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Class for executing single file transfer.
    /// </summary>
    internal class SingleFileTransfer
    {
        private readonly RenamingPolicy _renamingPolicy;
        private readonly ISFTPLogger _logger;
        private SingleFileTransferResult _result;

        /// <summary>
        /// Contructor for single file transfer class.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="context"></param>
        /// <param name="client"></param>
        /// <param name="renamingPolicy"></param>
        /// <param name="logger"></param>
        public SingleFileTransfer(FileItem file, BatchContext context, SftpClient client, RenamingPolicy renamingPolicy, ISFTPLogger logger)
        {
            _renamingPolicy = renamingPolicy;
            _logger = logger;

            FileTransferStartTime = DateTime.UtcNow;
            Client = client;
            SourceFile = file;
            BatchContext = context;

            DestinationFileNameWithMacrosExpanded = renamingPolicy.CreateRemoteFileName(
                    file.Name,
                    context.Destination.FileName);

            _result = new SingleFileTransferResult { Success = true };
        }

        public DateTime FileTransferStartTime { get; set; }

        public SftpClient Client { get; set; }
        public FileItem SourceFile { get; set; }
        public FileItem DestinationFile { get; set; }
        public string DestinationFileNameWithMacrosExpanded { get; set; }
        private string SourceFileDuringTransfer { get; set; }
        public string DestinationFileDuringTransfer { get; set; }
        internal BatchContext BatchContext { get; set; }

        /// <summary>
        /// Transfer state for SFTP Logger.
        /// </summary>
        internal TransferState State { get; set; }

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

                if (DestinationFileExists(DestinationFileNameWithMacrosExpanded))
                {
                    switch (BatchContext.Destination.Action)
                    {
                        case DestinationAction.Append:
                            AppendDestinationFile();
                            break;
                        case DestinationAction.Overwrite:
                            PutDestinationFile(removeExisting: true);
                            break;
                        case DestinationAction.Error:
                            throw new DestinationFileExistsException(DestinationFileNameWithMacrosExpanded);
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
            }
            CleanUpFiles();
            return _result;
        }

        private bool DestinationFileExists(string filename)
        {
            Trace(
                TransferState.CheckIfDestinationFileExists,
                "Checking if destination file {0} exists",
                SourceFile.NameWithMacrosExtended);
            return Client.Exists(filename);
        }

        private void RenameSourceFile()
        {
            var uniqueFileName = Util.CreateUniqueFileName();
            var workdir = (!string.IsNullOrEmpty(BatchContext.Info.WorkDir) ? BatchContext.Info.WorkDir : Path.GetDirectoryName(SourceFile.FullPath));
            var renamedFile = Path.Combine(workdir, uniqueFileName);

            Trace(TransferState.RenameSourceFileBeforeTransfer, "Renaming source file {0} to temporary file name {1} before transfer", SourceFile.Name, uniqueFileName);
            File.Move(SourceFile.FullPath, renamedFile);

            SourceFileDuringTransfer = renamedFile;
        }

        private void AppendDestinationFile()
        {
            var fullPath = SourceFile.FullPath;
            if (!string.IsNullOrEmpty(SourceFileDuringTransfer))
                fullPath = SourceFileDuringTransfer;
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

            Append(GetSourceFileContent(fullPath, encoding), encoding);

            if (BatchContext.Options.RenameDestinationFileDuringTransfer)
                RenameDestinationFile();
        }

        private void RenameDestinationFile()
        {
            SftpFileAttributes fileAttributes;
            if (!string.IsNullOrEmpty(DestinationFileDuringTransfer))
            {
                fileAttributes = Client.GetAttributes(DestinationFileDuringTransfer);
                if (fileAttributes.OthersCanWrite)
                {
                    Client.RenameFile(DestinationFileDuringTransfer, DestinationFileNameWithMacrosExpanded);
                    return;
                }
                fileAttributes.OthersCanWrite = true;
                Client.SetAttributes(DestinationFileDuringTransfer, fileAttributes);
                Client.RenameFile(DestinationFileDuringTransfer, DestinationFileNameWithMacrosExpanded);
                fileAttributes.OthersCanWrite = false;
                Client.SetAttributes(DestinationFileNameWithMacrosExpanded, fileAttributes);
                return;
            }

            DestinationFileDuringTransfer = Util.CreateUniqueFileName();
            Trace(TransferState.RenameSourceFileBeforeTransfer, "Renaming destination file {0} to temporary file name {1} during transfer", DestinationFileNameWithMacrosExpanded, DestinationFileDuringTransfer);
            fileAttributes = Client.GetAttributes(DestinationFileNameWithMacrosExpanded);
            if (fileAttributes.OthersCanWrite)
            {
                Client.RenameFile(DestinationFileNameWithMacrosExpanded, DestinationFileDuringTransfer);
                return;
            }
            fileAttributes.OthersCanWrite = true;
            Client.SetAttributes(DestinationFileNameWithMacrosExpanded, fileAttributes);
            Client.RenameFile(DestinationFileNameWithMacrosExpanded, DestinationFileDuringTransfer);
            fileAttributes.OthersCanWrite = false;
            Client.SetAttributes(DestinationFileDuringTransfer, fileAttributes);
        }

        private void Append(string[] content, Encoding encoding)
        {
            Trace(
                TransferState.AppendToDestinationFile,
                "Appending file {0} to existing file {1}",
                SourceFile.Name,
                DestinationFileNameWithMacrosExpanded);

            // Determine path to use to the destination file.
            var path = (BatchContext.Destination.Directory.Contains("/")) 
                ? BatchContext.Destination.Directory + "/"
                : BatchContext.Destination.Directory;

            // If destination rename during transfer is enabled, use that instead 
            path = (!string.IsNullOrEmpty(DestinationFileDuringTransfer)) 
                ? Path.Combine(path, DestinationFileDuringTransfer) 
                : Path.Combine(path, DestinationFileNameWithMacrosExpanded);

            var fileAttributes = Client.GetAttributes(path);
            if (fileAttributes.OthersCanWrite)
            {
                Client.AppendAllLines(path, content, encoding);
            }
            else
            {
                fileAttributes.OthersCanWrite = true;
                Client.SetAttributes(path, fileAttributes);
                Client.AppendAllLines(path, content, encoding);
                fileAttributes.OthersCanWrite = false;
                Client.SetAttributes(path, fileAttributes);
            }
        }

        private string[] GetSourceFileContent(string fullPath, Encoding encoding)
        {
            var result = new List<string>();
            result.Add("\n");
            var content = File.ReadAllLines(fullPath, encoding);
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

            DestinationFileDuringTransfer = doRename ? Util.CreateUniqueFileName() : DestinationFileNameWithMacrosExpanded;
            Trace(
                TransferState.PutFile,
                "Uploading {0}destination file {1}",
                doRename ? "temporary " : string.Empty,
                DestinationFileDuringTransfer);

            using (FileStream fs = new FileStream(SourceFileDuringTransfer, FileMode.Open))
            {
                Client.UploadFile(fs, DestinationFileDuringTransfer, removeExisting);
            }

            if (doRename)
            {
                if (removeExisting)
                {
                    Trace(
                        TransferState.DeleteDestinationFile,
                        "Deleting destination file {0} that is to be overwritten",
                        DestinationFileNameWithMacrosExpanded);

                    Client.DeleteFile(DestinationFileNameWithMacrosExpanded);
                }

                Trace(
                    TransferState.RenameDestinationFile,
                    "Renaming temporary destination file {0} to target file {1}",
                    DestinationFileDuringTransfer,
                    DestinationFileNameWithMacrosExpanded);

                Client.RenameFile(DestinationFileDuringTransfer, DestinationFileNameWithMacrosExpanded);
            }
        }

        /// <summary>
        /// Restores the LastWriteTime stamp to the destination file.
        /// </summary>
        private void RestoreModified()
        {
            var fileAttributes = Client.GetAttributes(DestinationFileNameWithMacrosExpanded);
            fileAttributes.LastWriteTime = SourceFile.Modified;
            Client.SetAttributes(DestinationFileNameWithMacrosExpanded, fileAttributes);
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ExecuteSourceOperation()
        {
            var filePath = (string.IsNullOrEmpty(SourceFileDuringTransfer) ? SourceFile.FullPath : SourceFileDuringTransfer);
            switch (BatchContext.Source.Operation)
            {
                case SourceOperation.Move:
                    var moveToPath = _renamingPolicy.CreateRemoteFilePathForMove(BatchContext.Source.DirectoryToMoveAfterTransfer, SourceFile.FullPath);
                    Trace(TransferState.SourceOperationMove, "Moving source file {0} to {1}", SourceFile.FullPath, moveToPath);
                    File.Move(filePath, moveToPath);

                    if (SourceFile.FullPath == null)
                    {
                        _logger.NotifyInformation(BatchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
                    }
                    break;

                case SourceOperation.Rename:
                    var renameToPath = _renamingPolicy.CreateRemoteFileNameForRename(SourceFile.FullPath, BatchContext.Source.FileNameAfterTransfer);
                    Trace(TransferState.SourceOperationRename, "Renaming source file {0} to {1}", SourceFile.FullPath, renameToPath);
                    File.Move(filePath, renameToPath);

                    if (SourceFile.FullPath == null)
                    {
                        _logger.NotifyInformation(BatchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
                    }
                    break;

                case SourceOperation.Delete:
                    Trace(TransferState.SourceOperationDelete, "Deleting source file {0} after transfer", Path.GetFileName(SourceFile.FullPath));
                    File.Delete(filePath);
                    break;

                case SourceOperation.Nothing:
                    if (BatchContext.Options.RenameSourceFileBeforeTransfer)
                    {
                        Trace(
                            TransferState.RestoreSourceFile,
                            "Restoring source file from {0} to the original name {1}",
                            Path.GetFileName(SourceFileDuringTransfer),
                            Path.GetFileName(SourceFile.FullPath));

                        File.Move(filePath, SourceFile.FullPath);
                    }
                    break;
            }
        }

        private void CleanUpFiles()
        {
            Trace(TransferState.CleanUpFiles, "Removing temporary file {0}", SourceFileDuringTransfer);
            TryToRemoveLocalTempFile(SourceFileDuringTransfer);

            TryToRemoveDestinationTempFile();
        }

        private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
        {
            _result.Success = false; // the routine instance should be marked as failed if even one transfer fails
            var errorMessage = string.Format("Failure in {0}: File '{1}' could not be transferred to '{2}'. Error: {3}", State, SourceFile.Name, BatchContext.Destination.Directory, exception.Message);
            if (!string.IsNullOrEmpty(sourceFileRestoreMessage))
            {
                errorMessage += " " + sourceFileRestoreMessage;
            }

            _result.ErrorMessages.Add(errorMessage);

            _logger.LogTransferFailed(this, BatchContext, errorMessage, exception);
        }

        private void TryToRemoveDestinationTempFile()
        {
            // If DestinationFileNameDuringTransfer is not set,
            // the destination file already exists and DestinationFileExistAction=Error
            if (string.IsNullOrEmpty(DestinationFileDuringTransfer))
            {
                return;
            }

            // If RenameDestinationFileDuringTransfer=False, there is no temporary file that could be deleted
            if (!BatchContext.Options.RenameDestinationFileDuringTransfer)
            {
                return;
            }

            try
            {
                if (DestinationFileExists(Path.GetFileName(DestinationFileDuringTransfer)))
                {
                    Trace(TransferState.RemoveTemporaryDestinationFile, "Removing temporary destination file {0}", DestinationFileDuringTransfer);
                    Client.DeleteFile(DestinationFileDuringTransfer);
                }
            }
            catch (Exception ex)
            {
                _logger.NotifyError(BatchContext, string.Format("Could not clean up temporary destination file '{0}': {1}", DestinationFileDuringTransfer, ex.Message), ex);
            }
        }

        private void TryToRemoveLocalTempFile(string fileName)
        {
            try
            {
                if (FileDefinedAndExists(fileName))
                {
                    if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal); // Clear flags so readonly doesn't cause any problems CO-469
                    }
                    File.Delete(fileName);
                }
            }
            catch (Exception e)
            {
                _logger.NotifyError(BatchContext, string.Format("Could not clean up local file '{0}'", fileName), e);
            }
        }

        private bool FileDefinedAndExists(string filename)
        {
            return !string.IsNullOrEmpty(filename) && File.Exists(filename);
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

        private bool ShouldSourceFileBeRestoredOnError()
        {
            if (BatchContext.Options.RenameSourceFileBeforeTransfer)
            {
                return true;
            }

            if (BatchContext.Source.Operation == SourceOperation.Move)
            {
                return true;
            }

            if (BatchContext.Source.Operation == SourceOperation.Rename)
            {
                return true;
            }

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
}
