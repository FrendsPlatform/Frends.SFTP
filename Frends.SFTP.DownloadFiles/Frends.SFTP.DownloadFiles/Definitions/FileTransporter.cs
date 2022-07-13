using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Main class for SFTP file transfers
/// </summary>
internal class FileTransporter
{
    private readonly Guid _instanceId;
    private readonly ISFTPLogger _logger;
    private readonly BatchContext _batchContext;
    private readonly string[] _filePaths;
    private readonly RenamingPolicy _renamingPolicy;

    internal FileTransporter(ISFTPLogger logger, BatchContext context, Guid instanceId)
    {
        _logger = logger;
        _batchContext = context;
        _instanceId = instanceId;
        _renamingPolicy = new RenamingPolicy(_batchContext.Info.TransferName, _instanceId);

        _result = new List<SingleFileTransferResult>();
        _filePaths = ConvertObjectToStringArray(context.Source.FilePaths);

        SourceDirectoryWithMacrosExtended = _renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);
        DestinationDirectoryWithMacrosExtended = _renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
    }

    private List<SingleFileTransferResult> _result { get; set; }

    private string SourceDirectoryWithMacrosExtended { get; set; }

    private string DestinationDirectoryWithMacrosExtended { get; set; }

    /// <summary>
    /// Transfer state for SFTP Logger
    /// </summary>
    public TransferState State { get; set; }

    /// <summary>
    /// Executes file transfers.
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
            ConnectionInfo connectionInfo;
            // Establish connectionInfo with connection parameters
            try
            {
                connectionInfo = GetConnectionInfo(_batchContext.Destination, _batchContext.Connection);
            } 
            catch (Exception e)
            {
                userResultMessage = $"Error when initializing connection info: {e}.";
                _logger.NotifyError(null, "Error when initializing connection info: ", e);
                return FormFailedFileTransferResult(userResultMessage);
            }

            using (var client = new SftpClient(connectionInfo))
            {
                client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256");
                client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256@libssh.org");

                // Check the fingerprint of the server if given.
                if (!String.IsNullOrEmpty(_batchContext.Connection.ServerFingerPrint))
                {
                    try
                    {
                        Trace(TransferState.Connection, "Checking server fingerprint.");
                        // If this check fails then SSH.NET will throw an SshConnectionException - with a message of "Key exchange negotiation failed".
                        client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                        {
                            // First try with SHA256 typed fingerprint
                            using (SHA256 mySHA256 = SHA256.Create())
                            {
                                var sha256Fingerprint = Convert.ToBase64String(mySHA256.ComputeHash(e.HostKey));
                                // Set the userResultMessage in case checking server fingerprint failes.
                                userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                        $"Expected fingerprint: '{_batchContext.Connection.ServerFingerPrint}', but was: '{sha256Fingerprint}'.";
                                e.CanTrust = (sha256Fingerprint == _batchContext.Connection.ServerFingerPrint);
                            }

                            if (!e.CanTrust)
                            {
                                userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                        $"Expected fingerprint: '{_batchContext.Connection.ServerFingerPrint}', but was: '{BitConverter.ToString(e.FingerPrint).Replace("-", ":")}'.";
                                // If previous failed try with MD5 typed fingerprint
                                var expectedFingerprint = Util.ConvertFingerprintToByteArray(_batchContext.Connection.ServerFingerPrint);
                                e.CanTrust = (e.FingerPrint.SequenceEqual(expectedFingerprint));
                            }

                        };
                    }
                    catch (Exception e)
                    {
                        _logger.NotifyError(null, "Error when checking the server fingerprint: ", e);
                        return FormFailedFileTransferResult(userResultMessage);
                    }

                }
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout);

                client.BufferSize = _batchContext.Connection.BufferSize * 1024;

                Trace(TransferState.Connection, $"Connecting to {_batchContext.Connection.Address}:{_batchContext.Connection.Port} using SFTP.");

                client.Connect();

                if (!client.IsConnected)
                {
                    _logger.NotifyError(null, "Error while connecting to destination: ", new SshConnectionException(userResultMessage));
                    return FormFailedFileTransferResult(userResultMessage);
                }
                _logger.NotifyInformation(_batchContext, $"Connection has been stablished to target {_batchContext.Connection.Address}:{_batchContext.Connection.Port} using SFTP.");

                // Fetch source file info and check if files were returned.
                var (files, success) = GetSourceFiles(client, _batchContext.Source);

                // If source directory doesn't exist, modify userResultMessage accordingly.
                if (!success)
                {
                    userResultMessage = $"Directory '{SourceDirectoryWithMacrosExtended}' doesn't exists.";
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

                    // Check does the destination directory exists.
                    if (!Directory.Exists(DestinationDirectoryWithMacrosExtended))
                    {
                        if (_batchContext.Options.CreateDestinationDirectories)
                        {
                            try
                            {
                                Directory.CreateDirectory(DestinationDirectoryWithMacrosExtended);
                            }
                            catch (Exception ex)
                            {
                                userResultMessage = $"Error while creating destination directory '{DestinationDirectoryWithMacrosExtended}': {ex.Message}";
                                return FormFailedFileTransferResult(userResultMessage);
                            }
                        }
                        else
                        {
                            userResultMessage = $"Destination directory '{DestinationDirectoryWithMacrosExtended}' was not found.";
                            return FormFailedFileTransferResult(userResultMessage);
                        }
                    }
                    else
                        _batchContext.DestinationFiles = GetDestinationFiles(DestinationDirectoryWithMacrosExtended);

                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
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
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SocketException ex)
        {
            userResultMessage = $"Unable to establish the socket: No such host is known.";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SshAuthenticationException ex)
        {
            userResultMessage = $"Authentication of SSH session failed: {ex.Message}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }

        return FormResultFromSingleTransferResults(_result);
    }

    #region Helper methods
    private static ConnectionInfo GetConnectionInfo(Destination destination, Connection connect)
    {
        ConnectionInfo connectionInfo;
        List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
        PrivateKeyFile privateKey = null;
        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyFile || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyFile)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyFile))
                throw new ArgumentException("Private key file path was not given.");
            privateKey = (connect.PrivateKeyFilePassphrase != null)
                ? new PrivateKeyFile(connect.PrivateKeyFile, connect.PrivateKeyFilePassphrase)
                : new PrivateKeyFile(connect.PrivateKeyFile);
        }
        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyString || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyString)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyString))
                throw new ArgumentException("Private key string was not given.");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(connect.PrivateKeyString));
            privateKey = (connect.PrivateKeyFilePassphrase != null)
                ? new PrivateKeyFile(stream, connect.PrivateKeyFilePassphrase)
                : new PrivateKeyFile(stream, "");
        }
        switch (connect.Authentication)
        {
            case AuthenticationType.UsernamePassword:
                methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                break;
            case AuthenticationType.UsernamePrivateKeyFile:
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyFile:
                methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            case AuthenticationType.UsernamePrivateKeyString:
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyString:
                methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            default:
                throw new ArgumentException($"Unknown Authentication type: '{connect.Authentication}'.");
        }

        connectionInfo = new ConnectionInfo(connect.Address, connect.Port, connect.UserName, methods.ToArray());
        connectionInfo.Encoding = GetEncoding(destination);

        return connectionInfo;
    }

    /// <summary>
    /// Get encoding for the file name to be transferred.
    /// </summary>
    /// <param name="dest"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{dest.FileNameEncoding}'.");
        }
    }

    /// <summary>
    /// Get source files that fit the file name / mask
    /// </summary>
    /// <param name="client"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    private Tuple<List<FileItem>, bool> GetSourceFiles(SftpClient client, Source source)
    {
        var fileItems = new List<FileItem>();

        if (_filePaths != null)
        {
            fileItems = _filePaths.Select(p => new FileItem(p) { Name = p }).ToList();
            if (fileItems.Any()) return new Tuple<List<FileItem>, bool>(fileItems, true);
            return new Tuple<List<FileItem>, bool>(fileItems, false);
        }

        // Return empty list and success.false value if source directory doesn't exists.
        if (!client.Exists(SourceDirectoryWithMacrosExtended)) return new Tuple<List<FileItem>, bool>(fileItems, false);

        // fetch all file names in given directory
        var files = client.ListDirectory(SourceDirectoryWithMacrosExtended).ToList();

        // return Tuple with empty list and success.true if files are not found.
        if (files.Count == 0) return new Tuple<List<FileItem>, bool>(fileItems, true);

        // create List of FileItems from found files.
        foreach (var file in files)
        {
            var name = file.Name;
            if (file.Name.Equals(".") || file.Name.Equals("..")) continue;

            if (file.IsDirectory) continue;

            if (Util.FileMatchesMask(Path.GetFileName(file.FullName), source.FileName))
            {
                FileItem item = new FileItem(file);
                _logger.NotifyInformation(_batchContext, $"FILE LIST {item.FullPath}.");
                fileItems.Add(item);
            }
        }
        return new Tuple<List<FileItem>, bool>(fileItems, true);
    }

    private static List<FileItem> GetDestinationFiles(string directory)
    {
        var destFiles = Directory.GetFiles(directory);
        var result = new List<FileItem>();
        foreach (var file in destFiles)
        {
            result.Add(new FileItem(file));
        }

        return result;
    }

    private static string[] ConvertObjectToStringArray(object objectArray)
    {
        var res = objectArray as object[];
        return res?.OfType<string>().ToArray();
    }

    private static FileTransferResult FormFailedFileTransferResult(string userResultMessage)
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

    /// <summary>
    /// Forms the userResultMessage for FileTransferResult object
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private static string GetUserResultMessage(IList<SingleFileTransferResult> results)
    {
        var userResultMessage = string.Empty;

        var errorMessages = results.SelectMany(x => x.ErrorMessages).ToList();
        if (errorMessages.Any())
            userResultMessage = MessageJoin(userResultMessage,
                string.Format("{0} Errors: {1}.", errorMessages.Count, string.Join(", \n", errorMessages)));

        var transferredFiles = results.Where(x => x.Success).Select(x => x.TransferredFile).ToList();
        if (transferredFiles.Any())
            userResultMessage = MessageJoin(userResultMessage,
                string.Format("{0} files transferred: {1}.", transferredFiles.Count,
                    string.Join(", \n", transferredFiles)));
        else
            userResultMessage = MessageJoin(userResultMessage, "No files transferred.");

        return userResultMessage;
    }

    private static string MessageJoin(params string[] args)
    {
        return string.Join(" ", args.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    /// <summary>
    /// Handles source operations in case no source files were found.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private SingleFileTransferResult NoSourceOperation(BatchContext context, Source source)
    {
        string msg;

        var transferName = context.Info.TransferName ?? string.Empty;

        if (context.Source.FilePaths == null)
            msg =
                $"No source files found from directory '{SourceDirectoryWithMacrosExtended}' with file mask '{source.FileName}' for transfer '{transferName}'.";
        else
            msg =
                $"No source files found from FilePaths '{string.Join(", ", context.Source.FilePaths)}' for transfer '{transferName}'.";

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
                throw new Exception("Unknown operation in NoSourceOperation.");
        }
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

    #endregion

}

