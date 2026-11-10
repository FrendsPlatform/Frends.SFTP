using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Main class for SFTP file transfers.
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
        renamingPolicy = new RenamingPolicy(batchContext.Info.TransferName, this.instanceId);

        Result = new List<SingleFileTransferResult>();
        filePaths = (context.Source.FilePaths != null || !string.IsNullOrEmpty((string)context.Source.FilePaths))
            ? ConvertObjectToStringArray(context.Source.FilePaths)
            : null;

        SourceDirectoryWithMacrosExtended = string.IsNullOrEmpty(context.Source.Directory)
            ? string.Empty
            : renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);
        DestinationDirectoryWithMacrosExtended = string.IsNullOrEmpty(context.Destination.Directory)
            ? "/"
            : renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
    }

    private List<SingleFileTransferResult> Result { get; set; }

    private string SourceDirectoryWithMacrosExtended { get; set; }

    private string DestinationDirectoryWithMacrosExtended { get; set; }

    /// <summary>
    /// Gets or sets transfer state for SFTP Logger.
    /// </summary>
    /// <example>TransferState.PutFile</example>
    private TransferState State { get; set; }

    /// <summary>
    /// Creates a remote directory and any missing parent directories.
    /// </summary>
    /// <param name="client">Connected SFTP client.</param>
    /// <param name="path">Remote directory path to create.</param>
    internal static void CreateDirectoriesRecursively(SftpClient client, string path)
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

    /// <summary>
    /// Executes file transfers.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the transfer.</param>
    /// <returns>Result of the file transfer.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the source directory cannot be found.</exception>
    /// <exception cref="FileNotFoundException">Thrown when a selected source file cannot be found.</exception>
    /// <exception cref="ArgumentException">Thrown when a transfer parameter is invalid.</exception>
    internal async Task<FileTransferResult> Run(CancellationToken cancellationToken)
    {
        var userResultMessage = string.Empty;

        try
        {
            // Fetch source file info and check if files were returned.
            var (files, success) = ListSourceFiles(batchContext.Source, cancellationToken);

            // If source directory doesn't exist, modify userResultMessage accordingly.
            if (!success)
            {
                userResultMessage = $"Directory '{batchContext.Source.Directory}' doesn't exists.";
                logger.NotifyInformation(batchContext, userResultMessage);

                return FormFailedFileTransferResult(userResultMessage);
            }

            if (files.Count == 0)
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
                batchContext.SourceFiles = files;

                ConnectionInfo connectionInfo;

                try
                {
                    connectionInfo = GetConnectionInfo(batchContext.Destination, batchContext.Connection);
                }
                catch (Exception e)
                {
                    userResultMessage = $"Error when initializing connection info: {e}";
                    logger.NotifyError(null, "Error when initializing connection info: ", e);

                    return FormFailedFileTransferResult(userResultMessage);
                }

                LogSourceSystemInfo(batchContext, logger);

                logger.NotifyInformation(batchContext, "Negotiation started.");

                using var client = new SftpClient(connectionInfo);

                client.KeepAliveInterval = TimeSpan.FromMilliseconds(batchContext.Connection.KeepAliveInterval);
                client.OperationTimeout = TimeSpan.FromSeconds(batchContext.Connection.ConnectionTimeout);
                client.BufferSize = batchContext.Connection.BufferSize * 1024;

                try
                {
                    if (batchContext.Connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                        ForceHostKeyAlgorithm(client, batchContext.Connection.HostKeyAlgorithm);

                    var expectedServerFingerprint = batchContext.Connection.ServerFingerPrint;

                    AddServerFingerprintCheck(client, expectedServerFingerprint);

                    SetCurrentState(
                        TransferState.Connection,
                        $"Connecting to {batchContext.Connection.Address}:{batchContext.Connection.Port} using SFTP.");

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

                    // Check does the destination directory exists.
                    if (!client.Exists(DestinationDirectoryWithMacrosExtended))
                    {
                        if (batchContext.Options.CreateDestinationDirectories)
                        {
                            try
                            {
                                SetCurrentState(
                                    TransferState.CreateDestinationDirectories,
                                    $"Creating destination directory {DestinationDirectoryWithMacrosExtended}.");
                                CreateDestinationDirectories(
                                    client,
                                    DestinationDirectoryWithMacrosExtended,
                                    cancellationToken);
                                logger.NotifyInformation(
                                    batchContext,
                                    $"DIRECTORY CREATE: Destination directory {DestinationDirectoryWithMacrosExtended} created.");
                            }
                            catch (Exception ex)
                            {
                                userResultMessage =
                                    $"Error while creating destination directory '{DestinationDirectoryWithMacrosExtended}': {ex.Message}";
                                logger.NotifyError(batchContext, userResultMessage, ex);

                                return FormFailedFileTransferResult(userResultMessage);
                            }
                        }
                        else
                        {
                            userResultMessage =
                                $"Destination directory '{DestinationDirectoryWithMacrosExtended}' was not found.";
                            logger.NotifyError(
                                batchContext,
                                userResultMessage,
                                new ArgumentException("No such directory."));

                            return FormFailedFileTransferResult(userResultMessage);
                        }
                    }

                    foreach (var file in files)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return FormResultFromSingleTransferResults(Result);

                        // Check that the connection is alive and if not try to connect again.
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
                finally
                {
                    client.Disconnect();
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            userResultMessage = $"Error when fetching source files: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SshConnectionException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SocketException ex)
        {
            userResultMessage = $"Unable to establish the socket: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SshAuthenticationException ex)
        {
            userResultMessage = $"Authentication of SSH session failed: {ex.Message}, {userResultMessage}";
            logger.NotifyError(batchContext, userResultMessage, ex);

            return FormFailedFileTransferResult(userResultMessage);
        }
        catch (SftpPathNotFoundException ex)
        {
            userResultMessage = $"Error when establishing connection to the Server: {ex.Message}, {userResultMessage}";
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
            CleanTempFiles(batchContext);
        }

        return FormResultFromSingleTransferResults(Result);
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
                : new PrivateKeyFile(stream, string.Empty);
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
            Encoding = Util.GetEncoding(
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

                if (!string.IsNullOrEmpty(batchContext.Connection.Password) &&
                    serverPrompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
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

                    foreach (var prompt in batchContext.Connection.PromptAndResponse)
                    {
                        if (serverPrompt.Request.IndexOf(prompt.Prompt, StringComparison.InvariantCultureIgnoreCase) !=
                            -1)
                            serverPrompt.Response = prompt.Response;
                    }
                }
            }

            logger.NotifyInformation(batchContext, "Keyboard-Interactive negotiation finished.");
        }
    }

    private void ForceHostKeyAlgorithm(SftpClient client, HostKeyAlgorithms algorithm)
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

    private void AddServerFingerprintCheck(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;
        var mD5serverFingerprint = string.Empty;
        var sHAServerFingerprint = string.Empty;

        client.HostKeyReceived += (_, e) =>
        {
            try
            {
                mD5serverFingerprint = e.FingerPrintMD5;
                sHAServerFingerprint = e.FingerPrintSHA256;

                if (!string.IsNullOrEmpty(expectedServerFingerprint))
                {
                    if (Util.IsMD5(expectedServerFingerprint.Replace(":", string.Empty).Replace("-", string.Empty)))
                    {
                        if (!expectedServerFingerprint.Contains(':'))
                        {
                            e.CanTrust = expectedServerFingerprint.Replace("-", string.Empty).ToLower() ==
                                         mD5serverFingerprint.Replace(":", string.Empty).Replace("-", string.Empty).ToLower();
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{mD5serverFingerprint}'.";
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
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{mD5serverFingerprint}'.";
                            }
                        }
                    }
                    else if (Util.IsSha256(expectedServerFingerprint))
                    {
                        if (Util.TryConvertHexStringToHex(expectedServerFingerprint))
                        {
                            using (SHA256 mySHA256 = SHA256.Create())
                            {
                                sHAServerFingerprint = Util.ToHex(mySHA256.ComputeHash(e.HostKey));
                            }

                            e.CanTrust = string.Equals(
                                sHAServerFingerprint,
                                expectedServerFingerprint,
                                StringComparison.InvariantCultureIgnoreCase);
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sHAServerFingerprint}'.";
                            }
                        }
                        else
                        {
                            e.CanTrust = string.Equals(
                                sHAServerFingerprint,
                                expectedServerFingerprint,
                                StringComparison.InvariantCultureIgnoreCase) ||
                                string.Equals(
                                    sHAServerFingerprint.Replace("=", string.Empty),
                                    expectedServerFingerprint,
                                    StringComparison.InvariantCultureIgnoreCase);
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sHAServerFingerprint}'.";
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
                logger.NotifyInformation(batchContext, $"Fingerprint (MD5): {mD5serverFingerprint.ToLower()}");
                logger.NotifyInformation(
                    batchContext,
                    $"Fingerprint (SHA-256): {sHAServerFingerprint.Replace("=", string.Empty)}");
                logger.NotifyInformation(
                    batchContext,
                    $"Cipher info: {client.ConnectionInfo.CurrentKeyExchangeAlgorithm}, {client.ConnectionInfo.CurrentHostKeyAlgorithm}, {client.ConnectionInfo.CurrentServerEncryption}");
            }
            catch (Exception ex)
            {
                logger.NotifyError(null, $"Error when checking the server fingerprint: {ex.Message}", ex);
                throw new ArgumentException($"Error when checking the server fingerprint: {ex.Message}", ex);
            }
        };
    }

    private Tuple<List<FileItem>, bool> ListSourceFiles(Source source, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        if (filePaths != null)
        {
            foreach (var path in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(path))
                {
                    var item = new FileItem(Path.GetFullPath(path));
                    logger.NotifyInformation(batchContext, $"FILE LIST {item.FullPath}.");
                    fileItems.Add(item);
                }
                else
                {
                    var msg = $"FILE LIST File '{path}' not found.";
                    logger.NotifyInformation(batchContext, msg);
                    Result.Add(new SingleFileTransferResult
                    {
                        ActionSkipped = true,
                        ErrorMessages = new List<string>(),
                        Success = true,
                        TransferredFilePath = string.Empty,
                        TransferredDestinationFilePath = string.Empty,
                        TransferredFile = string.Empty,
                    });
                }
            }

            return new Tuple<List<FileItem>, bool>(fileItems, true);
        }

        // Return empty list if source directory doesn't exists.
        if (!Directory.Exists(SourceDirectoryWithMacrosExtended))
            return new Tuple<List<FileItem>, bool>(fileItems, false);

        fileItems = ListFiles(source, SourceDirectoryWithMacrosExtended, cancellationToken);

        // return Tuple with empty list and success.true if files are not found.
        if (fileItems.Count == 0)
            return new Tuple<List<FileItem>, bool>(fileItems, true);

        return new Tuple<List<FileItem>, bool>(fileItems, true);
    }

    private List<FileItem> ListFiles(Source source, string directory, CancellationToken cancellationToken)
    {
        return ListFilesRecursive(source, directory, SourceDirectoryWithMacrosExtended, cancellationToken);
    }

    private List<FileItem> ListFilesRecursive(Source source, string currentDirectory, string rootDirectory, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        var relativePath = GetRelativePath(rootDirectory, currentDirectory);

        // Get files in current directory
        var files = Directory.GetFiles(currentDirectory);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Path.GetFileName(file).Equals(source.FileName) || Util.FileMatchesMask(Path.GetFileName(file), source.FileName) || source.FileName == string.Empty)
            {
                var item = new FileItem(Path.GetFullPath(file), relativePath);
                logger.NotifyInformation(batchContext, $"FILE LIST {item.FullPath} (Relative: '{item.RelativeDirectoryPath}')");
                fileItems.Add(item);
            }
        }

        // Recursively process subdirectories if enabled
        if (source.IncludeSubdirectories)
        {
            var directories = Directory.GetDirectories(currentDirectory);

            foreach (var dir in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attrs = File.GetAttributes(dir);
                if ((attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    logger.NotifyInformation(batchContext, $"Skipping reparse point directory '{dir}'.");
                    continue;
                }

                fileItems.AddRange(ListFilesRecursive(source, dir, rootDirectory, cancellationToken));
            }
        }

        return fileItems;
    }

    private string GetRelativePath(string rootDirectory, string currentDirectory)
    {
        var normalizedRoot = Path.GetFullPath(rootDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedCurrent = Path.GetFullPath(currentDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (normalizedCurrent.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        if (normalizedCurrent.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = normalizedCurrent.Substring(normalizedRoot.Length);
            return relativePath.TrimEnd(Path.DirectorySeparatorChar);
        }

        return string.Empty;
    }

    private void CreateDestinationDirectories(
        SftpClient client,
        string path,
        CancellationToken cancellationToken)
    {
        CreateDirectoriesRecursively(client, path);
    }

    // Check whether the directory exists by trying to change workingDirectory into it.
    private bool TryToChangeDir(SftpClient client, string dir)
    {
        try
        {
            client.ChangeDirectory(dir);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string[] ConvertObjectToStringArray(object objectArray)
    {
        if (!objectArray.GetType().IsArray)
        {
            throw new ArgumentException(
                $"Invalid type for parameter FilePaths. Expected array but was {objectArray.GetType()}");
        }

        var res = objectArray as object[];

        return res.OfType<string>().ToArray();
    }

    private FileTransferResult FormFailedFileTransferResult(string userResultMessage)
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
            OperationsLog = new Dictionary<string, string>(),
        };
    }

    private FileTransferResult FormResultFromSingleTransferResults(List<SingleFileTransferResult> singleResults)
    {
        var success = singleResults.All(x => x.Success);
        var actionSkipped = success && singleResults.Any(x => x.ActionSkipped);
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
            TransferredFilePaths =
                transferredFileResults.Select(r => r.TransferredFilePath ?? "--unknown--").ToList(),
            TransferredDestinationFilePaths =
                transferredFileResults.Select(r => r.TransferredDestinationFilePath ?? "--unknown--").ToArray(),
            OperationsLog = new Dictionary<string, string>(),
        };
    }

    private string GetUserResultMessage(IList<SingleFileTransferResult> results)
    {
        var userResultMessage = string.Empty;

        var errorMessages = results.SelectMany(x => x.ErrorMessages).ToList();
        if (errorMessages.Any())
        {
            userResultMessage = MessageJoin(
                userResultMessage,
                $"{errorMessages.Count} Errors: {string.Join(",\n", errorMessages)}.");
        }

        var transferredFiles =
            results.Where(x => x.Success && !x.ActionSkipped).Select(x => x.TransferredFile).ToList();
        if (transferredFiles.Any())
        {
            userResultMessage = MessageJoin(
                userResultMessage,
                $"{transferredFiles.Count} files transferred:\n{string.Join(",\n", transferredFiles)}\n");
        }
        else
        {
            userResultMessage = MessageJoin(userResultMessage, "No files transferred.\n");
        }

        return userResultMessage;
    }

    private string MessageJoin(params string[] args)
    {
        return string.Join(" ", args.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    /// <summary>
    /// Handles source operations in case no source files were found.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="source">Source endpoint configuration.</param>
    /// <returns>Failed transfer result describing the missing source files.</returns>
    /// <exception cref="Exception">Thrown when the source action is configured to fail.</exception>
    private SingleFileTransferResult NoSourceOperation(BatchContext context, Source source)
    {
        string msg;

        var transferName = context.Info.TransferName ?? string.Empty;

        if (context.Source.FilePaths == null)
        {
            msg =
                $"No source files found from directory '{SourceDirectoryWithMacrosExtended}' with file mask '{source.FileName}' for transfer '{transferName}'";
        }
        else
        {
            msg =
                $"No source files found from FilePaths '{string.Join(", ", context.Source.FilePaths)}' for transfer '{transferName}'";
        }

        switch (batchContext.Source.Action)
        {
            case SourceAction.Error:
                logger.NotifyError(context, msg, new FileNotFoundException());

                return new SingleFileTransferResult { Success = false, ErrorMessages = { msg } };
            case SourceAction.Info:
                logger.NotifyInformation(context, msg);

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
            logger.NotifyError(context, "Temp workdir cleanup failed.", ex);
        }
    }

    private void SetCurrentState(TransferState state, string msg)
    {
        State = state;
        logger.NotifyTrace($"{state}: {msg}");
    }

    private void LogSourceSystemInfo(BatchContext context, ISFTPLogger logger)
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
}
