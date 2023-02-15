﻿using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Security;

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

        if (_filePaths == null || !_filePaths.Any())
            SourceDirectoryWithMacrosExtended = string.IsNullOrEmpty(context.Source.Directory) ? "/" : _renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);

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
                if (_batchContext.Connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                    ForceHostKeyAlgorithm(client, _batchContext.Connection.HostKeyAlgorithm);

                var expectedServerFingerprint = _batchContext.Connection.ServerFingerPrint;

                // Check the fingerprint of the server if given.
                if (!string.IsNullOrEmpty(expectedServerFingerprint))
                {
                    SetCurrentState(TransferState.Connection, "Checking server fingerprint.");
                    try
                    {
                        CheckServerFingerprint(client, expectedServerFingerprint);
                    }
                    catch (Exception e)
                    {
                        _logger.NotifyError(null, $"Error when checking the server fingerprint", e);
                        return FormFailedFileTransferResult(userResultMessage);
                    }

                }

                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout);
                client.KeepAliveInterval = TimeSpan.FromMilliseconds(_batchContext.Connection.KeepAliveInterval);

                client.BufferSize = _batchContext.Connection.BufferSize * 1024;

                SetCurrentState(TransferState.Connection, $"Connecting to {_batchContext.Connection.Address}:{_batchContext.Connection.Port} using SFTP.");

                client.Connect();

                if (!client.IsConnected)
                {
                    _logger.NotifyError(null, "Error while connecting to destination: ", new SshConnectionException(userResultMessage));
                    return FormFailedFileTransferResult(userResultMessage);
                }
                _logger.NotifyInformation(_batchContext, $"Connection has been stablished to target {_batchContext.Connection.Address}:{_batchContext.Connection.Port} using SFTP.");

                // Fetch source file info and check if files were returned.
                var (files, success) = ListSourceFiles(client, _batchContext.Source, cancellationToken);

                // If source directory doesn't exist, modify userResultMessage accordingly.
                if (!success)
                {
                    userResultMessage = $"Directory '{SourceDirectoryWithMacrosExtended}' doesn't exists.";
                    _logger.NotifyInformation(_batchContext, userResultMessage);
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

                    _batchContext.DestinationFiles = GetDestinationFiles(DestinationDirectoryWithMacrosExtended);

                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Check that the connection is alive and if not try to connect again
                        if (!client.IsConnected)
                            client.Connect();

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
        catch (SftpPathNotFoundException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (FileNotFoundException ex)
        {
            userResultMessage = $"Error when fetching source files: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (Exception ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        finally
        {
            CleanTempFiles(_batchContext);
        }

        return FormResultFromSingleTransferResults(_result);
    }

    #region Helper methods
    private ConnectionInfo GetConnectionInfo(Destination destination, Connection connect)
    {
        ConnectionInfo connectionInfo;
        List<AuthenticationMethod> methods = new List<AuthenticationMethod>();

        if (connect.UseKeyboardInteractiveAuthentication)
        {
            // Construct keyboard-interactive authentication method
            var kauth = new KeyboardInteractiveAuthenticationMethod(connect.UserName);
            kauth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
            methods.Add(kauth);
        }

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

        connectionInfo.Encoding = Util.GetEncoding(destination.FileNameEncoding, destination.FileNameEncodingInString, destination.EnableBomForFileName);

        return connectionInfo;
    }

    private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
    {
        foreach (var prompt in e.Prompts)
            if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                prompt.Response = _batchContext.Connection.Password;
    }

    private void ForceHostKeyAlgorithm(SftpClient client, HostKeyAlgorithms algorithm)
    {
        client.ConnectionInfo.HostKeyAlgorithms.Clear();

        switch (algorithm)
        {
            case HostKeyAlgorithms.RSA:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); });
                break;
            case HostKeyAlgorithms.Ed25519:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-ed25519", (data) => { return new KeyHostAlgorithm("ssh-ed25519", new ED25519Key(), data); });
                break;
            case HostKeyAlgorithms.DSS:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-dss", (data) => { return new KeyHostAlgorithm("ssh-dss", new DsaKey(), data); });
                break;
            case HostKeyAlgorithms.nistp256:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp256", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp256", new EcdsaKey(), data); });
                break;
            case HostKeyAlgorithms.nistp384:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp384", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp384", new EcdsaKey(), data); });
                break;
            case HostKeyAlgorithms.nistp521:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp521", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp521", new EcdsaKey(), data); });
                break;
        }

        return;
    }

    private void CheckServerFingerprint(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = "";

        client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
        {
            if (Util.IsMD5(expectedServerFingerprint.Replace(":", "").Replace("-", "")))
            {
                if (!expectedServerFingerprint.Contains(':'))
                {
                    var serverFingerprint = BitConverter.ToString(e.FingerPrint).Replace("-", "").Replace(":", "");
                    e.CanTrust = expectedServerFingerprint.ToLower() == serverFingerprint.ToLower();
                    if (!e.CanTrust)
                        userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{serverFingerprint}'.";
                }
                else
                {
                    var serverFingerprint = BitConverter.ToString(e.FingerPrint).Replace('-', ':');
                    e.CanTrust = e.FingerPrint.SequenceEqual(Util.ConvertFingerprintToByteArray(expectedServerFingerprint));
                    if (!e.CanTrust)
                        userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{serverFingerprint}'.";
                }

            }
            else if (Util.IsSha256(expectedServerFingerprint))
            {
                if (Util.TryConvertHexStringToHex(expectedServerFingerprint))
                {
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        var sha256Fingerprint = Util.ToHex(mySHA256.ComputeHash(e.HostKey));

                        e.CanTrust = (sha256Fingerprint == expectedServerFingerprint);
                        if (!e.CanTrust)
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sha256Fingerprint}'.";
                    }
                }
                else
                {
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        var sha256Fingerprint = Convert.ToBase64String(mySHA256.ComputeHash(e.HostKey));
                        e.CanTrust = (sha256Fingerprint == expectedServerFingerprint || sha256Fingerprint.Replace("=", "") == expectedServerFingerprint);
                        if (!e.CanTrust)
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sha256Fingerprint}'.";
                    }
                }
            }
            else
            {
                userResultMessage = "Expected server fingerprint was given in unsupported format.";
                e.CanTrust = false;
            }

            if (!e.CanTrust)
                _logger.NotifyError(_batchContext, userResultMessage, new SshConnectionException());
        };
    }

    private Tuple<List<FileItem>, bool> ListSourceFiles(SftpClient client, Source source, CancellationToken cancellationToken)
    {
        SetCurrentState(TransferState.CheckSourceFiles, "Checking source files.");

        var fileItems = new List<FileItem>();

        if (_filePaths != null)
        {
            var items = _filePaths.Select(p => new FileItem(p) { Name = p }).ToList();
            foreach (var file in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!client.Exists(file.FullPath))
                    _logger.NotifyError(_batchContext, $"File does not exist: '{file.FullPath}", new FileNotFoundException());
                else
                    fileItems.Add(file);
            }

            if (fileItems.Any()) return new Tuple<List<FileItem>, bool>(fileItems, true);

            return new Tuple<List<FileItem>, bool>(fileItems, true);
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
            cancellationToken.ThrowIfCancellationRequested();

            var name = file.Name;
            if (file.Name.Equals(".") || file.Name.Equals("..")) continue;

            if (file.IsDirectory) continue;

            if (file.Name.Equals(source.FileName) || Util.FileMatchesMask(Path.GetFileName(file.FullName), source.FileName))
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

        var transferErrors = singleResults.Where(r => r.ErrorMessages.Any()).GroupBy(r => r.TransferredFile ?? "--unknown--")
                .ToDictionary(rg => rg.Key, rg => (IList<string>)rg.SelectMany(r => r.ErrorMessages).ToList());

        var transferredFileResults = singleResults.Where(r => r.Success && !r.ActionSkipped).ToList();

        return new FileTransferResult
        {
            ActionSkipped = actionSkipped,
            Success = success,
            UserResultMessage = userResultMessage,
            SuccessfulTransferCount = singleResults.Count(s => s.Success && !s.ActionSkipped),
            FailedTransferCount = singleResults.Count(s => !s.Success && !s.ActionSkipped),
            TransferredFileNames = transferredFileResults.Select(r => r.TransferredFile ?? "--unknown--").ToList(),
            TransferErrors = transferErrors,
            TransferredFilePaths = transferredFileResults.Select(r => r.TransferredFilePath ?? "--unknown--").ToList(),
            OperationsLog = (singleResults.Any(x => !x.EnableOperationsLog)) ? null : new Dictionary<string, string>()
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
                $"{errorMessages.Count} Errors: {string.Join(", \n", errorMessages)}.");

        var transferredFiles = results.Where(x => x.Success && !x.ActionSkipped).Select(x => x.TransferredFile).ToList();
        if (transferredFiles.Any())
            userResultMessage = MessageJoin(userResultMessage,
                $"{transferredFiles.Count} files transferred: {string.Join(", \n", transferredFiles)}.");
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
                return new SingleFileTransferResult { Success = true, ActionSkipped = true, ErrorMessages = { msg }, EnableOperationsLog = false };
            default:
                throw new Exception("Unknown operation in NoSourceOperation.");
        }
    }

    private void CleanTempFiles(BatchContext context)
    {
        try
        {
            if (!string.IsNullOrEmpty(context.TempWorkDir) && Directory.Exists(context.TempWorkDir))
                Directory.Delete(context.TempWorkDir, true);
        }
        catch (Exception ex)
        {
            _logger.NotifyError(context, "Temp workdir cleanup failed.", ex);
        }
    }

    private void SetCurrentState(TransferState state, string msg)
    {
        State = state;
        _logger.NotifyTrace($"{state}: {msg}");
    }

    #endregion
}

