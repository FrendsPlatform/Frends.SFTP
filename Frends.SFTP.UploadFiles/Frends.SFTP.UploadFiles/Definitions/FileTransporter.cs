using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net.Sockets;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    ///     Main class for SFTP file transfers
    /// </summary>
    public class FileTransporter
    {
        private readonly Guid _instanceId;
        private readonly ISFTPLogger _logger;
        private readonly BatchContext _batchContext;
        private readonly string[] _filePaths;
        private RenamingPolicy _renamingPolicy;

        /// <summary>
        ///     Constructor for SFTP file transfers
        /// </summary>
        public FileTransporter(ISFTPLogger logger, BatchContext context, Guid instanceId)
        {
            _logger = logger;
            _batchContext = context;
            _instanceId = instanceId;
            _renamingPolicy = new RenamingPolicy(_batchContext.Info.TransferName, _instanceId);

            _result = new List<SingleFileTransferResult>();
            _filePaths = ConvertObjectToStringArray(context.Source.FilePaths);
        }

        /// <summary>
        /// List of transfer results.
        /// </summary>
        private List<SingleFileTransferResult> _result { get; set; }

        /// <summary>
        /// Executes file transfers
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FileTransferResult Run(CancellationToken cancellationToken)
        {
            var userResultMessage = "";
            try
            {
                // Fetch source file info and check if files were returned.
                var (files, success) = GetSourceFiles(_batchContext.Source);

                // If source directory doesn't exist, modify userResultMessage accordingly.
                if (!success)
                {
                    userResultMessage = $"Directory '{_batchContext.Source.Directory}' doesn't exists.";
                    return FormFailedFileTransferResult(userResultMessage);
                }

                if (files.Count == 0)
                {
                    if (files == null)
                        _logger.NotifyInformation(_batchContext,
                            "Source end point returned null list for file list. If there are no files to transfer, the result should be an empty list.");

                    var noSourceResult = NoSourceOperation(_batchContext, _batchContext.Source);
                    _result.Add(noSourceResult);
                }
                else
                {
                    _batchContext.SourceFiles = files;

                    // Establish connectionInfo with connection parameters
                    var connectionInfo = GetConnectionInfo(_batchContext.Connection);

                    using (var client = new SftpClient(connectionInfo))
                    {
                        // Check the fingerprint of the server if given.
                        if (!String.IsNullOrEmpty(_batchContext.Connection.ServerFingerPrint))
                        {
                            try
                            {
                                // If this check fails then SSH.NET will throw an SshConnectionException - with a message of "Key exchange negotiation faild".
                                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                                {

                                    var expectedFingerprint = Util.ConvertFingerprintToByteArray(_batchContext.Connection.ServerFingerPrint);
                                    userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                        $"Expected fingerprint: '{_batchContext.Connection.ServerFingerPrint}', but was: '{e.FingerPrint}'";
                                    if (e.FingerPrint.SequenceEqual(expectedFingerprint))
                                        e.CanTrust = true;
                                    else
                                    {
                                        e.CanTrust = false;
                                    }
                                };
                            }
                            catch (Exception e)
                            {
                                _logger.NotifyError(null, "Error when initializing connection info: ", e);
                                return FormFailedFileTransferResult(userResultMessage);
                            }

                        }
                        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout);

                        // TODO: Chnage BufferSize to something meaningful
                        client.BufferSize = _batchContext.Connection.BufferSize;

                        client.Connect();

                        if (!client.IsConnected)
                        {
                            _logger.NotifyError(null, "Error while connecting to destination: ", new SshConnectionException(userResultMessage));
                            return FormFailedFileTransferResult(userResultMessage);
                        }

                        // Check does the destination directory exists.
                        if (!client.Exists(_batchContext.Destination.Directory))
                        {
                            if (_batchContext.Options.CreateDestinationDirectories)
                            {
                                try
                                {
                                    client.CreateDirectory(_batchContext.Destination.Directory);
                                }
                                catch (Exception ex)
                                {
                                    userResultMessage = $"Error while creating destination directory '{_batchContext.Destination.Directory}': {ex.Message}";
                                    return FormFailedFileTransferResult(userResultMessage);
                                }
                            }
                            else
                            {
                                userResultMessage = $"Destination directory '{_batchContext.Destination.Directory}' was not found.";
                                return FormFailedFileTransferResult(userResultMessage);
                            }
                        }

                        client.ChangeDirectory(_batchContext.Destination.Directory);

                        _batchContext.DestinationFiles = client.ListDirectory(".");

                        foreach (var file in files)
                        {
                            var singleTransfer = new SingleFileTransfer(file, _batchContext, client, _renamingPolicy, _logger);
                            var result = singleTransfer.TransferSingleFile();
                            _result.Add(result);
                        }
                        client.Disconnect();
                    }
                }   
            }
            catch (SshConnectionException ex)
            {
                userResultMessage = $"Error when establishing connection to the Server: {ex.Message}";
                return FormFailedFileTransferResult(userResultMessage);
            }
            catch (SocketException)
            {
                userResultMessage = $"Unable to establish the socket: No such host is known.";
                return FormFailedFileTransferResult(userResultMessage);
            }
            catch (SshAuthenticationException ex)
            {
                userResultMessage = $"Authentication of SSH session failed: {ex.Message}";
                return FormFailedFileTransferResult(userResultMessage);
            }

            return FormResultFromSingleTransferResults(_result);
        }

        #region Helper methods
        private static ConnectionInfo GetConnectionInfo(Connection connect)
        {
            List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
            PrivateKeyFile privateKey = null;
            if (connect.Authentication == AuthenticationType.UsernamePrivateKey || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKey)
                privateKey = (connect.PrivateKeyFilePassphrase != null)
                    ? new PrivateKeyFile(connect.PrivateKeyFileName, connect.PrivateKeyFilePassphrase)
                    : new PrivateKeyFile(connect.PrivateKeyFileName);

            switch (connect.Authentication)
            {
                case AuthenticationType.UsernamePassword:
                    methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                    break;
                case AuthenticationType.UsernamePrivateKey:
                    methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                    break;
                case AuthenticationType.UsernamePasswordPrivateKey:
                    methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                    methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                    break;
                default:
                    throw new ArgumentException($"Unknown Authentication type: '{connect.Authentication}'.");
            }

            return new ConnectionInfo(connect.Address, connect.Port, connect.UserName, methods.ToArray());
        }

        private Tuple<List<FileItem>, bool> GetSourceFiles(Source source)
        {
            var fileItems = new List<FileItem>();

            if (_filePaths != null)
            {
                fileItems = (List<FileItem>)_filePaths.Select(p => new FileItem(p) { Name = p }).ToList();
                if (fileItems.Any())
                    return new Tuple<List<FileItem>, bool>(fileItems, true);
                return new Tuple<List<FileItem>, bool>(fileItems, false);
            }

            // Return empty list if source directory doesn't exists.
            if (!Directory.Exists(source.Directory))
                return new Tuple<List<FileItem>, bool>(fileItems, false);

            // fetch all file names in given directory
            var files = Directory.GetFiles(source.Directory);

            // return Tuple with empty list and success.true if files are not found.
            if (files.Count() == 0)
                return new Tuple<List<FileItem>, bool>(fileItems, true);

            // create List of FileItems from found files.
            foreach (var file in files)
            {
                if (Util.FileMatchesMask(Path.GetFileName(file), source.FileName))
                {
                    FileItem item = new FileItem(Path.GetFullPath(file));
                    _logger.NotifyInformation(_batchContext, $"FILE LIST {item.FullPath}");
                    fileItems.Add(item);
                }
                    
            }

            return new Tuple<List<FileItem>, bool>(fileItems, true);
        }

        private static string[] ConvertObjectToStringArray(object objectArray)
        {
            var res = objectArray as object[];
            return res?.OfType<string>().ToArray();
        }

        private FileTransferResult FormFailedFileTransferResult(string userResultMessage)
        {
            return new FileTransferResult
            {
                ActionSkipped = true,
                Success = false,
                UserResultMessage = userResultMessage,
                SuccessfulTransferCount = 0,
                FailedTransferCount = 0,
                TransferredFileNames = new List<string>(),
                TransferErrors = new Dictionary<string, IList<string>>(),
                TransferredFilePaths = new List<string>(),
                OperationsLog = new Dictionary<string, string>()
            };
        }

        private FileTransferResult FormResultFromSingleTransferResults(List<SingleFileTransferResult> singleResults)
        {
            var success = singleResults.All(x => x.Success);
            var actionSkipped = success && singleResults.All(x => x.ActionSkipped);
            var userResultMessage = GetUserResultMessage(singleResults.ToList());

            _logger.LogBatchFinished(_batchContext, userResultMessage, success, actionSkipped);

            var transferErrors = singleResults.Where(r => !r.Success).GroupBy(r => r.TransferredFile ?? "--unknown--")
                    .ToDictionary(rg => rg.Key, rg => (IList<string>)rg.SelectMany(r => r.ErrorMessages).ToList());

            var transferredFileResults = singleResults.Where(r => r.Success && !r.ActionSkipped).ToList();

            var fileNames = from result in singleResults
                            where result.Success
                            select result.TransferredFile;

            var filePaths = from result in singleResults
                            where result.Success
                            select result.TransferredFilePath;

            return new FileTransferResult {
                ActionSkipped = actionSkipped,
                Success = success,
                UserResultMessage = userResultMessage,
                SuccessfulTransferCount = singleResults.Count(s => s.Success && !s.ActionSkipped),
                FailedTransferCount = singleResults.Count(s => !s.Success && !s.ActionSkipped),
                TransferredFileNames = transferredFileResults.Select(r => r.TransferredFile ?? "--unknown--").ToList(),
                TransferErrors = transferErrors,
                TransferredFilePaths = transferredFileResults.Select(r => r.TransferredFilePath ?? "--unknown--").ToList(),
                OperationsLog = new Dictionary<string, string>() 
            };
        }

        private string GetUserResultMessage(IList<SingleFileTransferResult> results)
        {
            var userResultMessage = string.Empty;

            var errorMessages = results.SelectMany(x => x.ErrorMessages).ToList();
            if (errorMessages.Any())
                userResultMessage = MessageJoin(userResultMessage,
                    string.Format("{0} Errors: {1}", errorMessages.Count, string.Join(", ", errorMessages)));

            var transferredFiles = results.Select(x => x.TransferredFile).Where(x => x != null).ToList();
            if (transferredFiles.Any())
                userResultMessage = MessageJoin(userResultMessage,
                    string.Format("{0} files transferred: {1}", transferredFiles.Count,
                        string.Join(", ", transferredFiles)));
            else
                userResultMessage = MessageJoin(userResultMessage, "No files transferred.");

            return userResultMessage;
        }

        private string MessageJoin(params string[] args)
        {
            return string.Join(" ", args.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private SingleFileTransferResult NoSourceOperation(BatchContext context, Source source)
        {
            string msg;

            var transferName = context.Info.TransferName ?? string.Empty;

            if (context.Source.FilePaths == null)
                msg =
                    $"No source files found from directory '{source.Directory}' with file mask '{source.FileName}' for transfer '{transferName}'";
            else
                msg =
                    $"No source files found from FilePaths '{string.Join(", ", context.Source.FilePaths)}' for transfer '{transferName}'";

            switch (_batchContext.Source.Action)
            {
                case SourceAction.Error:
                    _logger.NotifyError(context, msg, new FileNotFoundException());
                    return new SingleFileTransferResult { Success = false, ErrorMessages = { msg } };
                case SourceAction.Info:
                    _logger.NotifyInformation(context, msg);
                    return new SingleFileTransferResult { Success = true, ActionSkipped = true, ErrorMessages = { msg } };
                case SourceAction.Ignore:
                    return new SingleFileTransferResult { Success = true, ActionSkipped = true, ErrorMessages = { msg } };
                default:
                    throw new Exception("Unknown operation in NoSourceOperation");
            }
        }

        #endregion

    }
}
