using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Frends.SFTP.DownloadFiles.Definitions;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Frends.SFTP.DownloadFiles.Helpers;

/// <summary>
/// Main class for SFTP file transfers
/// </summary>
internal class FileTransporter
{
    private readonly Guid instanceId;
    private readonly ISFTPLogger logger;
    private readonly BatchContext batchContext;
    private readonly string[] filePaths;

    private readonly RenamingPolicy renamingPolicy;

    internal FileTransporter(ISFTPLogger logger, BatchContext context, Guid instanceId)
    {
        this.logger = logger;
        batchContext = context;
        this.instanceId = instanceId;
        renamingPolicy = new RenamingPolicy(batchContext.Info.TransferName, instanceId);

        Result = new List<SingleFileTransferResult>();
        filePaths = ConvertObjectToStringArray(context.Source.FilePaths);

        if (filePaths == null || !filePaths.Any())
        {
            SourceDirectoryWithMacrosExtended = string.IsNullOrEmpty(context.Source.Directory)
                ? "/"
                : renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);
        }

        DestinationDirectoryWithMacrosExtended =
            renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
    }

    private List<SingleFileTransferResult> Result { get; set; }

    private string SourceDirectoryWithMacrosExtended { get; set; }

    private string DestinationDirectoryWithMacrosExtended { get; set; }

    internal async Task<FileTransferResult> Run(CancellationToken cancellationToken)
    {
        logger.NotifyInformation(
            batchContext,
            $"Connecting to {batchContext.Connection.Address}:{batchContext.Connection.Port} using SFTP.");

        var userResultMessage = string.Empty;

        try
        {
            ConnectionInfo connectionInfo;

            try
            {
                connectionInfo = GetConnectionInfo(batchContext.Destination, batchContext.Connection);
            }
            catch (Exception e)
            {
                userResultMessage = $"Error when initializing connection info: {e}.";
                logger.NotifyError(null, "Error when initializing connection info: ", e);

                return FormFailedFileTransferResult(userResultMessage);
            }

            LogSourceSystemInfo(batchContext, logger);

            logger.NotifyInformation(batchContext, "Negotiation started.");

            using var client = new SftpClient(connectionInfo);
            client.KeepAliveInterval = TimeSpan.FromMilliseconds(batchContext.Connection.KeepAliveInterval);
            client.OperationTimeout = TimeSpan.FromSeconds(batchContext.Connection.ConnectionTimeout);
            client.BufferSize = batchContext.Connection.BufferSize * 1024;
            if (batchContext.Connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                ForceHostKeyAlgorithm(client, batchContext.Connection.HostKeyAlgorithm);

            var expectedServerFingerprint = batchContext.Connection.ServerFingerPrint;

            try
            {
                AddServerFingerprintCheck(client, expectedServerFingerprint);
            }
            catch (Exception e)
            {
                logger.NotifyError(null, $"Error when checking the server fingerprint", e);

                return FormFailedFileTransferResult(userResultMessage);
            }

            await client.ConnectAsync(cancellationToken);

            if (!client.IsConnected)
            {
                logger.NotifyError(
                    null,
                    "Error while connecting to destination: ",
                    new SshConnectionException(userResultMessage));

                return FormFailedFileTransferResult(userResultMessage);
            }

            logger.NotifyInformation(batchContext, "Negotiation finished.");

            // Fetch source file info and check if files were returned.
            var (files, success) = ListSourceFiles(client, batchContext.Source, cancellationToken);

            // If source directory doesn't exist, modify userResultMessage accordingly.
            if (!success)
            {
                userResultMessage = $"Directory '{SourceDirectoryWithMacrosExtended}' doesn't exists.";
                logger.NotifyInformation(batchContext, userResultMessage);

                return FormFailedFileTransferResult(userResultMessage);
            }

            if (files == null || files.Count == 0)
            {
                if (files == null)
                {
                    logger.NotifyInformation(
                        batchContext,
                        "Source end point returned null list for file list. If there are no files to transfer, the result should be an empty list.");
                }

                var noSourceResult = NoSourceOperation(batchContext, batchContext.Source);
                Result.Add(noSourceResult);
            }
            else
            {
                // Check does the destination directory exists.
                if (!Directory.Exists(DestinationDirectoryWithMacrosExtended))
                {
                    if (batchContext.Options.CreateDestinationDirectories)
                    {
                        try
                        {
                            Directory.CreateDirectory(DestinationDirectoryWithMacrosExtended);
                        }
                        catch (Exception ex)
                        {
                            userResultMessage =
                                $"Error while creating destination directory '{DestinationDirectoryWithMacrosExtended}': {ex.Message}";

                            return FormFailedFileTransferResult(userResultMessage);
                        }
                    }
                    else
                    {
                        userResultMessage =
                            $"Destination directory '{DestinationDirectoryWithMacrosExtended}' was not found.";

                        return FormFailedFileTransferResult(userResultMessage);
                    }
                }

                batchContext.DestinationFiles =
                    GetDestinationFiles(DestinationDirectoryWithMacrosExtended, cancellationToken);

                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return FormResultFromSingleTransferResults(Result);

                    // Check that the connection is alive and if not try to connect again
                    if (!client.IsConnected)
                        await client.ConnectAsync(cancellationToken);

                    var singleTransfer = new SingleFileTransfer(
                        file,
                        DestinationDirectoryWithMacrosExtended,
                        batchContext,
                        client,
                        renamingPolicy,
                        logger);
                    var result = await singleTransfer.TransferSingleFile(cancellationToken);
                    Result.Add(result);
                }
            }
        }
        catch (SshConnectionException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SocketException ex)
        {
            userResultMessage = $"Unable to establish the socket: No such host is known.";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SshAuthenticationException ex)
        {
            userResultMessage = $"Authentication of SSH session failed: {ex.Message}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SftpPathNotFoundException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (FileNotFoundException ex)
        {
            userResultMessage = $"Error when fetching source files: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (ArgumentException ex)
        {
            userResultMessage = $"{ex.Message} {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (Exception ex)
        {
            userResultMessage = $"Error when executing file transfer: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        finally
        {
            CleanTempWorkDir();
        }

        return FormResultFromSingleTransferResults(Result);
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
            case HostKeyAlgorithms.Nistp256:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp256", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ecdsa-sha2-nistp256", new EcdsaKey(sshKeyData));
                });

                break;
            case HostKeyAlgorithms.Nistp384:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp384", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ecdsa-sha2-nistp384", new EcdsaKey(sshKeyData));
                });

                break;
            case HostKeyAlgorithms.Nistp521:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp521", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ecdsa-sha2-nistp521", new EcdsaKey(sshKeyData));
                });

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

        var transferredFiles =
            results.Where(x => x.Success && !x.ActionSkipped).Select(x => x.TransferredFile).ToList();

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
        logger.NotifyInformation(
            context,
            $"Assembly: {Assembly.GetAssembly(typeof(SftpClient)).GetName().Name} {Assembly.GetAssembly(typeof(SftpClient)).GetName().Version}");
        var bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        logger.NotifyInformation(
            context,
            $"Platform: {RuntimeInformation.OSDescription} {bit}; CLR: {RuntimeInformation.FrameworkDescription}");
        logger.NotifyInformation(
            context,
            $"Culture: {CultureInfo.CurrentCulture.TwoLetterISOLanguageName}; {Encoding.Default.WebName}");
    }

    private SingleFileTransferResult NoSourceOperation(BatchContext context, Source source)
    {
        string msg;

        var transferName = context.Info.TransferName ?? string.Empty;

        msg = context.Source.FilePaths == null
            ? $"No source files found from directory '{SourceDirectoryWithMacrosExtended}' with file mask '{source.FileName}' for transfer '{transferName}'."
            : $"No source files found from FilePaths '{string.Join(", ", context.Source.FilePaths)}' for transfer '{transferName}'.";

        switch (batchContext.Source.Action)
        {
            case SourceAction.Error:
                logger.NotifyError(context, msg, new ArgumentException(msg));

                return new SingleFileTransferResult { Success = false, ErrorMessages = { msg } };
            case SourceAction.Info:
                logger.NotifyInformation(context, msg);

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
                logger.NotifyError(batchContext, "Failure in Keyboard-Interactive authentication: ", ex);
            }
        }

        PrivateKeyFile privateKey = null;

        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyFile ||
            connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyFile)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyFile))
                throw new ArgumentException("Private key file path was not given.");
            privateKey = (connect.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(connect.PrivateKeyFile, connect.PrivateKeyPassphrase)
                : new PrivateKeyFile(connect.PrivateKeyFile);
        }

        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyString ||
            connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyString)
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
            Encoding =
                Util.GetEncoding(
                    destination.FileNameEncoding,
                    destination.FileNameEncodingInString,
                    destination.EnableBomForFileName),
            ChannelCloseTimeout = TimeSpan.FromSeconds(batchContext.Connection.ConnectionTimeout),
            Timeout = TimeSpan.FromSeconds(batchContext.Connection.ConnectionTimeout),
        };

        return connectionInfo;
    }

    private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
    {
        if (e.Prompts.Any())
        {
            logger.NotifyInformation(
                batchContext,
                $"Keyboard-Interactive negotiation started with the server {batchContext.Connection.Address}.");

            foreach (var serverPrompt in e.Prompts)
            {
                logger.NotifyInformation(batchContext, $"Prompt: {serverPrompt.Request.Replace(":", string.Empty)}");

                if (!string.IsNullOrEmpty(batchContext.Connection.Password)
                    && serverPrompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    serverPrompt.Response = batchContext.Connection.Password;
                }
                else
                {
                    if (!batchContext.Connection.PromptAndResponse.Any() || !batchContext.Connection.PromptAndResponse
                            .Select(p => p.Prompt.ToLower()).ToList()
                            .Contains(serverPrompt.Request.Replace(":", string.Empty).Trim().ToLower()))
                    {
                        var errorMsg =
                            $"Failure in Keyboard-interactive authentication: No response given for server prompt request --> {serverPrompt.Request.Replace(":", string.Empty).Trim()}";

                        throw new ArgumentException(errorMsg);
                    }

                    foreach (var prompt in batchContext.Connection.PromptAndResponse
                                 .Where(e => serverPrompt.Request.IndexOf(
                                     e.Prompt,
                                     StringComparison.InvariantCultureIgnoreCase) != -1))
                        serverPrompt.Response = prompt.Response;
                }
            }

            logger.NotifyInformation(batchContext, $"Keyboard-Interactive negotiation finished.");
        }
    }

    private void AddServerFingerprintCheck(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;
        var md5serverFingerprint = string.Empty;
        var shaServerFingerprint = string.Empty;

        client.HostKeyReceived += (_, e) =>
        {
            try
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
                            var normalizedExpected = expectedServerFingerprint
                                .Replace(":", string.Empty)
                                .Replace("-", string.Empty)
                                .ToLowerInvariant();
                            var normalizedActual = md5serverFingerprint
                                .Replace(":", string.Empty)
                                .Replace("-", string.Empty)
                                .ToLowerInvariant();
                            e.CanTrust = normalizedActual == normalizedExpected;

                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{md5serverFingerprint}'.";
                            }
                        }
                        else
                        {
                            e.CanTrust =
                                e.FingerPrint.SequenceEqual(
                                    Util.ConvertFingerprintToByteArray(expectedServerFingerprint));

                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
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
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{shaServerFingerprint}'.";
                            }
                        }
                        else
                        {
                            e.CanTrust = shaServerFingerprint == expectedServerFingerprint ||
                                         shaServerFingerprint.Replace("=", string.Empty) == expectedServerFingerprint;

                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
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
                    {
                        logger.NotifyError(batchContext, userResultMessage, new SshConnectionException());

                        throw new ArgumentException(userResultMessage);
                    }
                }

                logger.NotifyInformation(batchContext, $"Server: {client.ConnectionInfo.ServerVersion}");
                logger.NotifyInformation(batchContext, $"Fingerprint (MD5): {md5serverFingerprint.ToLower()}");
                logger.NotifyInformation(
                    batchContext,
                    $"Fingerprint (SHA-256): {shaServerFingerprint.Replace("=", string.Empty)}");
                logger.NotifyInformation(
                    batchContext,
                    $"Cipher info: {client.ConnectionInfo.CurrentKeyExchangeAlgorithm}, {client.ConnectionInfo.CurrentHostKeyAlgorithm}, {client.ConnectionInfo.CurrentServerEncryption}");
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Error when checking the server fingerprint", exception);
            }
        };
    }

    private Tuple<List<FileItem>, bool> ListSourceFiles(
        SftpClient client,
        Source source,
        CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        if (filePaths != null)
        {
            foreach (var file in filePaths.ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!client.Exists(file))
                    logger.NotifyError(batchContext, $"File does not exist: '{file}'", new SftpPathNotFoundException());
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
        return ListFilesRecursive(sftp, source, directory, SourceDirectoryWithMacrosExtended, cancellationToken);
    }

    private List<FileItem> ListFilesRecursive(SftpClient sftp, Source source, string currentDirectory, string rootDirectory, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        var files = sftp.ListDirectory(currentDirectory).ToList();

        var relativePath = GetRelativePath(rootDirectory, currentDirectory);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Name.Equals(".") || file.Name.Equals("..")) continue;

            if (file.IsDirectory && source.IncludeSubdirectories)
                fileItems.AddRange(ListFilesRecursive(sftp, source, file.FullName, rootDirectory, cancellationToken));

            if (!file.IsDirectory && !file.IsSocket && !file.IsSymbolicLink && !file.IsBlockDevice &&
                !file.IsCharacterDevice && !file.IsNamedPipe && (file.Name.Equals(source.FileName) || Util.FileMatchesMask(Path.GetFileName(file.FullName), source.FileName)))
            {
                var item = new FileItem(file, relativePath);
                logger.NotifyInformation(batchContext, $"FILE LIST {item.FullPath} (Relative: '{item.RelativeDirectoryPath}')");
                fileItems.Add(item);
            }
        }

        return fileItems;
    }

    private string GetRelativePath(string rootDirectory, string currentDirectory)
    {
        var normalizedRoot = rootDirectory.TrimEnd('/') + "/";
        var normalizedCurrent = currentDirectory.TrimEnd('/') + "/";

        if (normalizedCurrent.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        if (normalizedCurrent.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = normalizedCurrent.Substring(normalizedRoot.Length);
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        return string.Empty;
    }

    private FileTransferResult FormResultFromSingleTransferResults(List<SingleFileTransferResult> singleResults)
    {
        var success = singleResults.All(x => x.Success);
        var actionSkipped = success && singleResults.All(x => x.ActionSkipped);
        var userResultMessage = GetUserResultMessage(singleResults.ToList());

        logger.LogBatchFinished(batchContext, userResultMessage, success, actionSkipped);

        var transferErrors = singleResults.Where(r => r.ErrorMessages.Any())
            .GroupBy(r => r.TransferredFile ?? "--unknown--")
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
            TransferredDestinationFilePaths =
                transferredFileResults.Select(s => s.DestinationFilePath ?? "--unknown--").ToArray(),
            OperationsLog = singleResults.Any(x => !x.EnableOperationsLog) ? null : new Dictionary<string, string>(),
        };
    }

    private void CleanTempWorkDir()
    {
        try
        {
            Directory.Delete(batchContext.TempWorkDir, true);
        }
        catch (Exception ex)
        {
            logger.NotifyError(batchContext, "Temp workdir cleanup failed.", ex);
        }
    }
}
