namespace Frends.SFTP.DownloadFiles.Definitions;

using Renci.SshNet;
using Renci.SshNet.Common;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Security;
using System.Threading;

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

        Result = new List<SingleFileTransferResult>();
        _filePaths = ConvertObjectToStringArray(context.Source.FilePaths);

        if (_filePaths == null || !_filePaths.Any())
            SourceDirectoryWithMacrosExtended = string.IsNullOrEmpty(context.Source.Directory) ? "/" : _renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);

        DestinationDirectoryWithMacrosExtended = _renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
    }

    private List<SingleFileTransferResult> Result { get; set; }

    private string SourceDirectoryWithMacrosExtended { get; set; }

    private string DestinationDirectoryWithMacrosExtended { get; set; }

    internal async Task<FileTransferResult> Run(CancellationToken cancellationToken)
    {
        _logger.NotifyInformation(_batchContext, $"Connecting to {_batchContext.Connection.Address}:{_batchContext.Connection.Port} using SFTP.");

        var userResultMessage = string.Empty;
        try
        {
            ConnectionInfo connectionInfo;

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

            LogSourceSystemInfo(_batchContext, _logger);

            _logger.NotifyInformation(_batchContext, "Negotiation started.");

            using var client = new SftpClient(connectionInfo);
            if (_batchContext.Connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                ForceHostKeyAlgorithm(client, _batchContext.Connection.HostKeyAlgorithm);

            var expectedServerFingerprint = _batchContext.Connection.ServerFingerPrint;

            try
            {
                CheckServerFingerprint(client, expectedServerFingerprint);
            }
            catch (Exception e)
            {
                _logger.NotifyError(null, $"Error when checking the server fingerprint", e);
                return FormFailedFileTransferResult(userResultMessage);
            }

            client.KeepAliveInterval = TimeSpan.FromMilliseconds(_batchContext.Connection.KeepAliveInterval);
            client.OperationTimeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout);

            _batchContext.Connection.BufferSize = _batchContext.Connection.BufferSize * 1024;
            client.BufferSize = _batchContext.Connection.BufferSize;

            await client.ConnectAsync(cancellationToken);

            if (!client.IsConnected)
            {
                _logger.NotifyError(null, "Error while connecting to destination: ", new SshConnectionException(userResultMessage));
                return FormFailedFileTransferResult(userResultMessage);
            }

            _logger.NotifyInformation(_batchContext, "Negotiation finished.");

            // Fetch source file info and check if files were returned.
            var (files, success) = ListSourceFiles(client, _batchContext.Source, cancellationToken);

            // If source directory doesn't exist, modify userResultMessage accordingly.
            if (!success)
            {
                userResultMessage = $"Directory '{SourceDirectoryWithMacrosExtended}' doesn't exists.";
                _logger.NotifyInformation(_batchContext, userResultMessage);
                return FormFailedFileTransferResult(userResultMessage);
            }

            if (files == null || files.Count == 0)
            {
                if (files == null)
                {
                    _logger.NotifyInformation(
                        _batchContext,
                        "Source end point returned null list for file list. If there are no files to transfer, the result should be an empty list.");
                }

                var noSourceResult = NoSourceOperation(_batchContext, _batchContext.Source);
                Result.Add(noSourceResult);
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

                _batchContext.DestinationFiles = GetDestinationFiles(DestinationDirectoryWithMacrosExtended, cancellationToken);

                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return FormResultFromSingleTransferResults(Result);

                    // Check that the connection is alive and if not try to connect again
                    if (!client.IsConnected)
                        await client.ConnectAsync(cancellationToken);

                    var singleTransfer = new SingleFileTransfer(file, _batchContext, client, _renamingPolicy, _logger);
                    var result = await singleTransfer.TransferSingleFile(cancellationToken);
                    Result.Add(result);
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

        return FormResultFromSingleTransferResults(Result);
    }

    #region Helper methods

    private static void ForceHostKeyAlgorithm(SftpClient client, HostKeyAlgorithms algorithm)
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
            case HostKeyAlgorithms.Nistp256:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp256", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp256", new EcdsaKey(), data); });
                break;
            case HostKeyAlgorithms.Nistp384:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp384", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp384", new EcdsaKey(), data); });
                break;
            case HostKeyAlgorithms.Nistp521:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp521", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp521", new EcdsaKey(), data); });
                break;
        }

        return;
    }

    private static List<FileItem> GetDestinationFiles(string directory, CancellationToken cancellationToken)
    {
        var destFiles = Directory.GetFiles(directory);
        var result = new List<FileItem>();
        foreach (var file in destFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
            TransferredDestinationFilePaths = Array.Empty<string>(),
            OperationsLog = new Dictionary<string, string>(),
        };
    }

    private static string GetUserResultMessage(IList<SingleFileTransferResult> results)
    {
        var userResultMessage = string.Empty;

        var errorMessages = results.SelectMany(x => x.ErrorMessages).ToList();
        if (errorMessages.Any())
        {
            userResultMessage = MessageJoin(
                userResultMessage,
                $"{errorMessages.Count} Errors: {string.Join(", \n", errorMessages)}.");
        }

        var transferredFiles = results.Where(x => x.Success && !x.ActionSkipped).Select(x => x.TransferredFile).ToList();

        userResultMessage = transferredFiles.Any()
            ? MessageJoin(
            userResultMessage,
            $"{transferredFiles.Count} files transferred: {string.Join(", \n", transferredFiles)}.")
            : MessageJoin(userResultMessage, "No files transferred.");

        return userResultMessage;
    }

    private static string MessageJoin(params string[] args)
    {
        return string.Join(" ", args.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static void LogSourceSystemInfo(BatchContext context, ISFTPLogger logger)
    {
        logger.NotifyInformation(context, $"Assembly: {Assembly.GetAssembly(typeof(SftpClient)).GetName().Name} {Assembly.GetAssembly(typeof(SftpClient)).GetName().Version}");
        var bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        logger.NotifyInformation(context, $"Platform: {RuntimeInformation.OSDescription} {bit}; CLR: {RuntimeInformation.FrameworkDescription}");
        logger.NotifyInformation(context, $"Culture: {CultureInfo.CurrentCulture.TwoLetterISOLanguageName}; {Encoding.Default.WebName}");
    }

    private SingleFileTransferResult NoSourceOperation(BatchContext context, Source source)
    {
        string msg;

        var transferName = context.Info.TransferName ?? string.Empty;

        msg = context.Source.FilePaths == null
            ? $"No source files found from directory '{SourceDirectoryWithMacrosExtended}' with file mask '{source.FileName}' for transfer '{transferName}'."
            : $"No source files found from FilePaths '{string.Join(", ", context.Source.FilePaths)}' for transfer '{transferName}'.";

        switch (_batchContext.Source.Action)
        {
            case SourceAction.Error:
                _logger.NotifyError(context, msg, new ArgumentException(msg));
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
                : new PrivateKeyFile(stream);
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
            Timeout = TimeSpan.FromSeconds(_batchContext.Connection.ConnectionTimeout),
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
                _logger.NotifyInformation(_batchContext, $"Prompt: {serverPrompt.Request.Replace(":", string.Empty)}");

                if (!string.IsNullOrEmpty(_batchContext.Connection.Password)
                    && serverPrompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    serverPrompt.Response = _batchContext.Connection.Password;
                }
                else
                {
                    if (!_batchContext.Connection.PromptAndResponse.Any() || !_batchContext.Connection.PromptAndResponse.Select(p => p.Prompt.ToLower()).ToList().Contains(serverPrompt.Request.Replace(":", string.Empty).Trim().ToLower()))
                    {
                        var errorMsg = $"Failure in Keyboard-interactive authentication: No response given for server prompt request --> {serverPrompt.Request.Replace(":", string.Empty).Trim()}";
                        throw new ArgumentException(errorMsg);
                    }

                    foreach (var prompt in _batchContext.Connection.PromptAndResponse
                        .Where(e => serverPrompt.Request.IndexOf(e.Prompt, StringComparison.InvariantCultureIgnoreCase) != -1))
                        serverPrompt.Response = prompt.Response;
                }
            }

            _logger.NotifyInformation(_batchContext, $"Keyboard-Interactive negotiation finished.");
        }
    }

    private void CheckServerFingerprint(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;
        var md5serverFingerprint = string.Empty;
        var shaServerFingerprint = string.Empty;

        client.HostKeyReceived += (sender, e) =>
        {
            md5serverFingerprint = BitConverter.ToString(e.FingerPrint).Replace('-', ':');

            using (SHA256 mySHA256 = SHA256.Create())
            {
                shaServerFingerprint = Convert.ToBase64String(mySHA256.ComputeHash(e.HostKey));
            }

            if (!string.IsNullOrEmpty(expectedServerFingerprint))
            {
                if (Util.IsMD5(expectedServerFingerprint.Replace(":", string.Empty).Replace("-", string.Empty)))
                {
                    if (!expectedServerFingerprint.Contains(':'))
                    {
                        e.CanTrust = expectedServerFingerprint.ToLower() == md5serverFingerprint.Replace(":", string.Empty).ToLower();
                        if (!e.CanTrust)
                        {
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{md5serverFingerprint}'.";
                        }
                    }
                    else
                    {
                        e.CanTrust = e.FingerPrint.SequenceEqual(Util.ConvertFingerprintToByteArray(expectedServerFingerprint));
                        if (!e.CanTrust)
                        {
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{md5serverFingerprint}'.";
                        }
                    }
                }
                else if (Util.IsSha256(expectedServerFingerprint))
                {
                    if (Util.TryConvertHexStringToHex(expectedServerFingerprint))
                    {
                        using (SHA256 mySHA256 = SHA256.Create())
                        {
                            shaServerFingerprint = Util.ToHex(mySHA256.ComputeHash(e.HostKey));
                        }

                        e.CanTrust = shaServerFingerprint == expectedServerFingerprint;
                        if (!e.CanTrust)
                        {
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{shaServerFingerprint}'.";
                        }
                    }
                    else
                    {
                        e.CanTrust = shaServerFingerprint == expectedServerFingerprint || shaServerFingerprint.Replace("=", string.Empty) == expectedServerFingerprint;
                        if (!e.CanTrust)
                        {
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{shaServerFingerprint}'.";
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
            }

            _logger.NotifyInformation(_batchContext, $"Server: {client.ConnectionInfo.ServerVersion}");
            _logger.NotifyInformation(_batchContext, $"Fingerprint (MD5): {md5serverFingerprint.ToLower()}");
            _logger.NotifyInformation(_batchContext, $"Fingerprint (SHA-256): {shaServerFingerprint.Replace("=", string.Empty)}");
            _logger.NotifyInformation(_batchContext, $"Cipher info: {client.ConnectionInfo.CurrentKeyExchangeAlgorithm}, {client.ConnectionInfo.CurrentHostKeyAlgorithm}, {client.ConnectionInfo.CurrentServerEncryption}");
        };
    }

    private Tuple<List<FileItem>, bool> ListSourceFiles(SftpClient client, Source source, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        if (_filePaths != null)
        {
            foreach (var file in _filePaths.ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!client.Exists(file))
                    _logger.NotifyError(_batchContext, $"File does not exist: '{file}", new SftpPathNotFoundException());
                else
                    fileItems.Add(new FileItem(client.Get(file)));
            }

            if (fileItems.Any()) return new Tuple<List<FileItem>, bool>(fileItems, true);

            return new Tuple<List<FileItem>, bool>(fileItems, true);
        }

        // Return empty list and success.false value if source directory doesn't exists.
        if (!client.Exists(SourceDirectoryWithMacrosExtended)) return new Tuple<List<FileItem>, bool>(fileItems, false);

        fileItems = ListFiles(client, source, SourceDirectoryWithMacrosExtended, cancellationToken);

        // return Tuple with empty list and success.true if files are not found.
        if (fileItems.Count == 0) return new Tuple<List<FileItem>, bool>(fileItems, true);

        return new Tuple<List<FileItem>, bool>(fileItems, true);
    }

    private List<FileItem> ListFiles(SftpClient sftp, Source source, string directory, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        // fetch all file names in given directory
        var files = sftp.ListDirectory(directory).ToList();

        // create List of FileItems from found files.
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Name.Equals(".") || file.Name.Equals("..")) continue;

            if (file.IsRegularFile && (file.Name.Equals(source.FileName) || Util.FileMatchesMask(Path.GetFileName(file.FullName), source.FileName)))
            {
                var item = new FileItem(file);
                _logger.NotifyInformation(_batchContext, $"FILE LIST {item.FullPath}");
                fileItems.Add(item);
            }

            if (file.IsDirectory && source.IncludeSubdirectories)
                fileItems.AddRange(ListFiles(sftp, source, file.FullName, cancellationToken));
        }

        return fileItems;
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
            TransferredDestinationFilePaths = transferredFileResults.Select(s => s.DestinationFilePath ?? "--unknown--").ToArray(),
            OperationsLog = singleResults.Any(x => !x.EnableOperationsLog) ? null : new Dictionary<string, string>(),
        };
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

    #endregion
}