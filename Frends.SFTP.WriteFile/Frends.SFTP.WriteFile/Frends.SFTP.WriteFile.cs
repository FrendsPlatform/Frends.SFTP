using System.ComponentModel;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;
using Frends.SFTP.WriteFile.Helpers;
using Renci.SshNet;

namespace Frends.SFTP.WriteFile;

/// <summary>
/// Main class for the task.
/// </summary>
public static class SFTP
{
    /// <summary>
    /// Writes string content to a file through SFTP connection.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.WriteFile)
    /// </summary>
    /// <param name="input">Write options with full path and string content</param>
    /// <param name="connection">Transfer connection parameters</param>
    /// <param name="options">Options for write file</param>
    /// <param name="cancellationToken">Token given by Frends to enable Task termination.</param>
    /// <returns>object { bool Success, string Path, double SizeInMegaBytes, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result WriteFile(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            ConnectionInfo connectionInfo;

            try
            {
                var builder = new ConnectionInfoBuilder(input, connection);
                connectionInfo = builder.BuildConnectionInfo();
            }
            catch (Exception ex)
            {
                return new ArgumentException($"Error when initializing connection info: {ex}", ex).Handle(options);
            }

            using var client = new SftpClient(connectionInfo);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.BufferSize = connection.BufferSize * 1024;

            // Disable support for these host key exchange algorithms:
            // https://github.com/FrendsPlatform/Frends.SFTP/security/dependabot/4
            client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256");
            client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256@libssh.org");

            if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

            // Check the fingerprint of the server if given.
            if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
            {
                Util.AddServerFingerprintCheck(client, connection.ServerFingerPrint);
            }

            client.Connect();
            cancellationToken.ThrowIfCancellationRequested();

            if (!client.IsConnected)
                throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

            try
            {
                var targetDirectory = Path.GetDirectoryName(input.Path);

                if (!string.IsNullOrEmpty(targetDirectory))
                {
                    targetDirectory = targetDirectory.Replace("\\", "/");

                    if (!client.Exists(targetDirectory))
                    {
                        if (options.CreateDestinationDirectories)
                        {
                            try
                            {
                                Util.CreateDirectoriesRecursively(client, targetDirectory);
                            }
                            catch (Exception ex)
                            {
                                return new ArgumentException(
                                    $"Error while creating destination directory '{targetDirectory}': {ex.Message}", ex)
                                    .Handle(options);
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Destination directory '{targetDirectory}' was not found.");
                        }
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                var encoding = Util.GetEncoding(input.FileEncoding, input.EnableBom, input.EncodingInString);

                switch (input.WriteBehaviour)
                {
                    case WriteOperation.Append:
                        var content = string.Empty;
                        if (input.AddNewLine)
                            content = "\n";
                        content += input.Content;
                        client.AppendAllText(input.Path, content, encoding);

                        break;
                    case WriteOperation.Overwrite:
                        if (client.Exists(input.Path))
                            client.DeleteFile(input.Path);
                        client.WriteAllText(input.Path, input.Content, encoding);

                        break;
                    case WriteOperation.Error:
                        if (client.Exists(input.Path))
                            throw new ArgumentException($"File already exists: {input.Path}");
                        client.WriteAllText(input.Path, input.Content, encoding);

                        break;
                    default:
                        throw new ArgumentException($"Unknown WriteBehaviour type: '{input.WriteBehaviour}'.");
                }

                return new Result(client.Get(input.Path));
            }
            finally
            {
                client.Disconnect();
            }
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
