﻿using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

namespace Frends.SFTP.UploadFiles.Definitions;

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
        _filePaths = (context.Source.FilePaths != null || !string.IsNullOrEmpty((string)context.Source.FilePaths)) ? ConvertObjectToStringArray(context.Source.FilePaths) : null;

        SourceDirectoryWithMacrosExtended = _renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);
        DestinationDirectoryWithMacrosExtended = string.IsNullOrEmpty(context.Destination.Directory) ? "/" : _renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
    }

    private List<SingleFileTransferResult> _result { get; set; }

    private string SourceDirectoryWithMacrosExtended { get; set; }

    private string DestinationDirectoryWithMacrosExtended { get; set; }

    /// <summary>
    /// Transfer state for SFTP Logger
    /// </summary>
    public TransferState State { get; set; }

    /// <summary>
    /// Executes file transfers
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<FileTransferResult> Run(CancellationToken cancellationToken)
    {
        var userResultMessage = "";
        try
        {
            // Fetch source file info and check if files were returned.
            var (files, success) = ListSourceFiles(_batchContext.Source, cancellationToken);

            // If source directory doesn't exist, modify userResultMessage accordingly.
            if (!success)
            {
                userResultMessage = $"Directory '{_batchContext.Source.Directory}' doesn't exists.";
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
                _batchContext.SourceFiles = files;

                ConnectionInfo connectionInfo;
                try
                {
                    connectionInfo = GetConnectionInfo(_batchContext.Destination, _batchContext.Connection);
                }
                catch (Exception e)
                {
                    userResultMessage = $"Error when initializing connection info: {e}";
                    _logger.NotifyError(null, "Error when initializing connection info: ", e);
                    return FormFailedFileTransferResult(userResultMessage);
                }

                LogSourceSystemInfo(_batchContext, _logger);

                _logger.NotifyInformation(_batchContext, "Negotiation started.");

                using (var client = new SftpClient(connectionInfo))
                {
                    try
                    {
                        if (_batchContext.Connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                            ForceHostKeyAlgorithm(client, _batchContext.Connection.HostKeyAlgorithm);

                        var expectedServerFingerprint = _batchContext.Connection.ServerFingerPrint;

                        try
                        {
                            CheckServerFingerprint(client, expectedServerFingerprint);
                        }
                        catch (Exception ex)
                        {
                            _logger.NotifyError(null, $"Error when checking the server fingerprint: {ex.Message}", ex);
                            return FormFailedFileTransferResult(userResultMessage);
                        }

                        client.KeepAliveInterval = TimeSpan.FromMilliseconds(_batchContext.Connection.KeepAliveInterval);
                        client.OperationTimeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout);

                        client.BufferSize = _batchContext.Connection.BufferSize * 1024;

                        SetCurrentState(TransferState.Connection, $"Connecting to {_batchContext.Connection.Address}:{_batchContext.Connection.Port} using SFTP.");

                        await client.ConnectAsync(cancellationToken);

                        if (!client.IsConnected)
                        {
                            _logger.NotifyError(null, "Error while connecting to destination: ", new SshConnectionException(userResultMessage));
                            return FormFailedFileTransferResult(userResultMessage);
                        }

                        _logger.NotifyInformation(_batchContext, "Negotiation finished.");

                        // Check does the destination directory exists.
                        if (!client.Exists(DestinationDirectoryWithMacrosExtended))
                        {
                            if (_batchContext.Options.CreateDestinationDirectories)
                            {
                                try
                                {
                                    SetCurrentState(TransferState.CreateDestinationDirectories, $"Creating destination directory {DestinationDirectoryWithMacrosExtended}.");
                                    CreateDestinationDirectories(client, DestinationDirectoryWithMacrosExtended, cancellationToken);
                                    _logger.NotifyInformation(_batchContext, $"DIRECTORY CREATE: Destination directory {DestinationDirectoryWithMacrosExtended} created.");
                                }
                                catch (Exception ex)
                                {
                                    userResultMessage = $"Error while creating destination directory '{DestinationDirectoryWithMacrosExtended}': {ex.Message}";
                                    _logger.NotifyError(_batchContext, userResultMessage, ex);
                                    return FormFailedFileTransferResult(userResultMessage);
                                }
                            }
                            else
                            {
                                userResultMessage = $"Destination directory '{DestinationDirectoryWithMacrosExtended}' was not found.";
                                _logger.NotifyError(_batchContext, userResultMessage, new ArgumentException("No such directory."));
                                return FormFailedFileTransferResult(userResultMessage);
                            }
                        }

                        foreach (var file in files)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return FormResultFromSingleTransferResults(_result);

                            // Check that the connection is alive and if not try to connect again.
                            if (!client.IsConnected)
                                await client.ConnectAsync(cancellationToken);

                            var singleTransfer = new SingleFileTransfer(file, DestinationDirectoryWithMacrosExtended, _batchContext, client, _renamingPolicy, _logger);
                            var result = await singleTransfer.TransferSingleFile(cancellationToken);
                            _result.Add(result);
                        }
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            userResultMessage = $"Error when fetching source files: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SshConnectionException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SocketException ex)
        {
            userResultMessage = $"Unable to establish the socket: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SshAuthenticationException ex)
        {
            userResultMessage = $"Authentication of SSH session failed: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SftpPathNotFoundException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (ArgumentException ex)
        {
            userResultMessage = $"{ex.Message} {userResultMessage}";
            _logger.NotifyError(_batchContext, userResultMessage, ex);
            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (Exception ex)
        {
            userResultMessage = $"Error when executing file transfer: {ex.Message}, {userResultMessage}";
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
        var methods = new List<AuthenticationMethod>();

        if (connect.UseKeyboardInteractiveAuthentication)
        {
            try
            {
                // Construct keyboard-interactive authentication method
                var kauth = new KeyboardInteractiveAuthenticationMethod(connect.UserName);
                kauth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
                methods.Add(kauth);
            }
            catch (Exception ex)
            {
                _logger.NotifyError(_batchContext, "Failure in Keyboard-Interactive authentication: ", ex);
            }
        }

        PrivateKeyFile privateKey = null;
        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyFile || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyFile)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyFile))
                throw new ArgumentException("Private key file path was not given.");
            privateKey = (connect.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(connect.PrivateKeyFile, connect.PrivateKeyPassphrase)
                : new PrivateKeyFile(connect.PrivateKeyFile);
        }
        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyString || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyString)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyString))
                throw new ArgumentException("Private key string was not given.");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(connect.PrivateKeyString));
            privateKey = (connect.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(stream, connect.PrivateKeyPassphrase)
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

        connectionInfo = new ConnectionInfo(connect.Address, connect.Port, connect.UserName, methods.ToArray())
        {
            Encoding = Util.GetEncoding(destination.FileNameEncoding, destination.FileNameEncodingInString, destination.EnableBomForFileName),
            ChannelCloseTimeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout),
            Timeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout)
        };

        return connectionInfo;
    }

    private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
    {
        if (e.Prompts.Any())
        {
            _logger.NotifyInformation(_batchContext, $"Keyboard-Interactive negotiation started with the server {_batchContext.Connection.Address}.");

            foreach (var serverPrompt in e.Prompts)
            {
                _logger.NotifyInformation(_batchContext, $"Prompt: {serverPrompt.Request.Replace(":", "")}");

                if (!string.IsNullOrEmpty(_batchContext.Connection.Password) && serverPrompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                    serverPrompt.Response = _batchContext.Connection.Password;
                else
                {
                    if (!_batchContext.Connection.PromptAndResponse.Any() || !_batchContext.Connection.PromptAndResponse.Select(p => p.Prompt.ToLower()).ToList().Contains(serverPrompt.Request.Replace(":", "").Trim().ToLower()))
                    {
                        var errorMsg = $"Failure in Keyboard-interactive authentication: No response given for server prompt request --> {serverPrompt.Request.Replace(":", "").Trim()}";
                        throw new ArgumentException(errorMsg);
                    }

                    foreach (var prompt in _batchContext.Connection.PromptAndResponse)
                    {
                        if (serverPrompt.Request.IndexOf(prompt.Prompt, StringComparison.InvariantCultureIgnoreCase) != -1)
                            serverPrompt.Response = prompt.Response;
                    }
                }
            }
            _logger.NotifyInformation(_batchContext, "Keyboard-Interactive negotiation finished.");
        }
    }

    private static void ForceHostKeyAlgorithm(SftpClient client, HostKeyAlgorithms algorithm)
    {
        client.ConnectionInfo.HostKeyAlgorithms.Clear();

        switch (algorithm)
        {
            case HostKeyAlgorithms.RSA:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ssh-rsa", new RsaKey(sshKeyData));
                });
                break;
            case HostKeyAlgorithms.Ed25519:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-ed25519", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ssh-ed25519", new ED25519Key(sshKeyData));
                });
                break;
            case HostKeyAlgorithms.DSS:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-dss", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ssh-dss", new DsaKey(sshKeyData));
                });
                break;
            case HostKeyAlgorithms.nistp256:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp256", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ecdsa-sha2-nistp256", new EcdsaKey(sshKeyData));
                });
                break;
            case HostKeyAlgorithms.nistp384:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp384", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ecdsa-sha2-nistp384", new EcdsaKey(sshKeyData));
                });
                break;
            case HostKeyAlgorithms.nistp521:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp521", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ecdsa-sha2-nistp521", new EcdsaKey(sshKeyData));
                });
                break;
        }

        return;
    }

    private void CheckServerFingerprint(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;
        var MD5serverFingerprint = string.Empty;
        var SHAServerFingerprint = string.Empty;

        client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
        {
            MD5serverFingerprint = e.FingerPrintMD5;
            SHAServerFingerprint = e.FingerPrintSHA256;

            if (!string.IsNullOrEmpty(expectedServerFingerprint))
            {
                if (Util.IsMD5(expectedServerFingerprint.Replace(":", "").Replace("-", "")))
                {
                    if (!expectedServerFingerprint.Contains(':'))
                    {
                        e.CanTrust = expectedServerFingerprint.ToLower() == MD5serverFingerprint.Replace(":", "").ToLower();
                        if (!e.CanTrust)
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{MD5serverFingerprint}'.";
                    }
                    else
                    {
                        e.CanTrust = e.FingerPrint.SequenceEqual(Util.ConvertFingerprintToByteArray(expectedServerFingerprint));
                        if (!e.CanTrust)
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{MD5serverFingerprint}'.";
                    }

                }
                else if (Util.IsSha256(expectedServerFingerprint))
                {
                    if (Util.TryConvertHexStringToHex(expectedServerFingerprint))
                    {
                        using (SHA256 mySHA256 = SHA256.Create())
                        {
                            SHAServerFingerprint = Util.ToHex(mySHA256.ComputeHash(e.HostKey));
                        }
                        e.CanTrust = (SHAServerFingerprint == expectedServerFingerprint);
                        if (!e.CanTrust)
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{SHAServerFingerprint}'.";
                    }
                    else
                    {
                        e.CanTrust = (SHAServerFingerprint == expectedServerFingerprint || SHAServerFingerprint.Replace("=", "") == expectedServerFingerprint);
                        if (!e.CanTrust)
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{SHAServerFingerprint}'.";
                    }
                }
                else
                {
                    userResultMessage = "Expected server fingerprint was given in unsupported format.";
                    e.CanTrust = false;
                }

                if (!e.CanTrust)
                    _logger.NotifyError(_batchContext, userResultMessage, new SshConnectionException());
            }

            _logger.NotifyInformation(_batchContext, $"Server: {client.ConnectionInfo.ServerVersion}");
            _logger.NotifyInformation(_batchContext, $"Fingerprint (MD5): {MD5serverFingerprint.ToLower()}");
            _logger.NotifyInformation(_batchContext, $"Fingerprint (SHA-256): {SHAServerFingerprint.Replace("=", "")}");
            _logger.NotifyInformation(_batchContext, $"Cipher info: {client.ConnectionInfo.CurrentKeyExchangeAlgorithm}, {client.ConnectionInfo.CurrentHostKeyAlgorithm}, {client.ConnectionInfo.CurrentServerEncryption}");
        };
    }

    private Tuple<List<FileItem>, bool> ListSourceFiles(Source source, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        if (_filePaths != null)
        {
            foreach (var path in _filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(path))
                {
                    var item = new FileItem(Path.GetFullPath(path));
                    _logger.NotifyInformation(_batchContext, $"FILE LIST {item.FullPath}.");
                    fileItems.Add(item);
                }
                else
                {
                    var msg = $"FILE LIST File '{path}' not found.";
                    _logger.NotifyInformation(_batchContext, msg);
                    _result.Add(new SingleFileTransferResult
                    {
                        ActionSkipped = true,
                        ErrorMessages = new List<string>(),
                        Success = true,
                        TransferredFilePath = string.Empty,
                        TransferredDestinationFilePath = string.Empty,
                        TransferredFile = string.Empty
                    });
                }
            }

            return new Tuple<List<FileItem>, bool>(fileItems, true);
        }

        // Return empty list if source directory doesn't exists.
        if (!Directory.Exists(SourceDirectoryWithMacrosExtended))
            return new Tuple<List<FileItem>, bool>(fileItems, false);

        // fetch all file names in given directory
        var files = Directory.GetFiles(SourceDirectoryWithMacrosExtended);

        // return Tuple with empty list and success.true if files are not found.
        if (files.Length == 0)
            return new Tuple<List<FileItem>, bool>(fileItems, true);

        // create List of FileItems from found files.
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Path.GetFileName(file).Equals(source.FileName) || Util.FileMatchesMask(Path.GetFileName(file), source.FileName))
            {
                var item = new FileItem(Path.GetFullPath(file));
                _logger.NotifyInformation(_batchContext, $"FILE LIST {item.FullPath}.");
                fileItems.Add(item);
            }
        }

        return new Tuple<List<FileItem>, bool>(fileItems, true);
    }

    public static void CreateDirectoriesRecursively(SftpClient client, string path)
    {
        path = path.Replace(@"\", "/");
        if (client.Exists(path)) return;
        try
        {
            client.CreateDirectory(path);
        }
        catch
        {
            var parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                CreateDirectoriesRecursively(client, parent);
                client.CreateDirectory(path);
            }
        }
    }

    private static void CreateDestinationDirectories(SftpClient client, string path, CancellationToken cancellationToken)
    {
        CreateDirectoriesRecursively(client, path);
    }

    // Check whether the directory exists by trying to change workingDirectory into it.
    private static bool TryToChangeDir(SftpClient client, string dir)
    {
        try
        {
            client.ChangeDirectory(dir);
            return true;
        }
        catch { return false; }
    }

    private static string[] ConvertObjectToStringArray(object objectArray)
    {
        if (!objectArray.GetType().IsArray)
            throw new ArgumentException($"Invalid type for parameter FilePaths. Expected array but was {objectArray.GetType()}");
        var res = objectArray as object[];
        return res.OfType<string>().ToArray();
    }

    private static FileTransferResult FormFailedFileTransferResult(string userResultMessage)
    {
        userResultMessage += "No files transferred.";
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
            TransferredDestinationFilePaths = Array.Empty<string>(),
            OperationsLog = new Dictionary<string, string>()
        };
    }

    private FileTransferResult FormResultFromSingleTransferResults(List<SingleFileTransferResult> singleResults)
    {
        var success = singleResults.All(x => x.Success);
        var actionSkipped = success && singleResults.Any(x => x.ActionSkipped);
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
            TransferredDestinationFilePaths = transferredFileResults.Select(r => r.TransferredDestinationFilePath ?? "--unknown--").ToArray(),
            OperationsLog = new Dictionary<string, string>()
        };
    }

    private static string GetUserResultMessage(IList<SingleFileTransferResult> results)
    {
        var userResultMessage = string.Empty;

        var errorMessages = results.SelectMany(x => x.ErrorMessages).ToList();
        if (errorMessages.Any())
            userResultMessage = MessageJoin(userResultMessage,
                $"{errorMessages.Count} Errors: {string.Join(",\n", errorMessages)}.");

        var transferredFiles = results.Where(x => x.Success && !x.ActionSkipped).Select(x => x.TransferredFile).ToList();
        if (transferredFiles.Any())
            userResultMessage = MessageJoin(userResultMessage,
                $"{transferredFiles.Count} files transferred:\n{string.Join(",\n", transferredFiles)}\n");
        else
            userResultMessage = MessageJoin(userResultMessage, "No files transferred.\n");

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
                $"No source files found from directory '{SourceDirectoryWithMacrosExtended}' with file mask '{source.FileName}' for transfer '{transferName}'";
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

    private static void LogSourceSystemInfo(BatchContext context, ISFTPLogger logger)
    {
        logger.NotifyInformation(context, $"Assembly: {Assembly.GetAssembly(typeof(SftpClient)).GetName().Name} {Assembly.GetAssembly(typeof(SftpClient)).GetName().Version}");
        var bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        logger.NotifyInformation(context, $"Platform: {RuntimeInformation.OSDescription} {bit}; CLR: {RuntimeInformation.FrameworkDescription}");
        logger.NotifyInformation(context, $"Culture: {CultureInfo.CurrentCulture.TwoLetterISOLanguageName}; {Encoding.Default.WebName}");
    }

    #endregion

}

