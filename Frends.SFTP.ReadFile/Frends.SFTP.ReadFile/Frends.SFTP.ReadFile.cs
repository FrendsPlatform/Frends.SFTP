using System.ComponentModel;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;
using Frends.SFTP.ReadFile.Helpers;
using Renci.SshNet;

namespace Frends.SFTP.ReadFile;

/// <summary>
/// Main class of the task.
/// </summary>
public static class SFTP
{
    /// <summary>
    /// Reads a file through SFTP connection.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.ReadFile)
    /// </summary>
    /// <param name="input">Read options with a full path and encoding</param>
    /// <param name="connection">Transfer connection parameters</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">Token given by Frends to enable Task termination.</param>
    /// <returns>object { bool Success, string TextContent, byte[] BinaryContent, string Path, double SizeInMegaBytes, DateTime LastWriteTime, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> ReadFile(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ConnectionInfo connectionInfo;

            try
            {
                var builder = new ConnectionInfoBuilder(input, connection);
                connectionInfo = builder.BuildConnectionInfo();
            }
            catch (Exception e)
            {
                return new ArgumentException($"Error when initializing connection info: {e}").Handle(options);
            }

            using var client = new SftpClient(connectionInfo);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.BufferSize = connection.BufferSize * 1024;

            if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

            if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
                Util.AddServerFingerprintCheck(client, connection.ServerFingerPrint);

            await client.ConnectAsync(cancellationToken);

            if (!client.IsConnected)
                throw new ArgumentException($"Error while connecting to destination: {connection.Address}");
            var encoding = Util.GetEncoding(input.FileEncoding, input.EnableBom, input.EncodingInString);

            object content = options.ContentType switch
            {
                ContentType.Binary => client.ReadAllBytes(input.Path),
                ContentType.Text => client.ReadAllText(input.Path, encoding),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(options),
                    options.ContentType,
                    "Content type not supported."),
            };

            var file = await client.GetAsync(input.Path, cancellationToken);

            return new Result
            {
                Success = true,
                BinaryContent = content as byte[] ?? Array.Empty<byte>(),
                TextContent = content as string ?? string.Empty,
                Path = file.FullName,
                SizeInMegaBytes = Math.Round(file.Length / 1024d / 1024d, 3),
                LastWriteTime = file.LastWriteTime,
            };
        }
        catch (Exception e)
        {
            return e.Handle(options);
        }
    }
}
