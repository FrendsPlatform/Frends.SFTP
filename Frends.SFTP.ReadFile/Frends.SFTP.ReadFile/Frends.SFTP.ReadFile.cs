using System.ComponentModel;
using Renci.SshNet;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile;

/// <summary>
/// Main class of the task.
/// </summary>
public class SFTP
{
    /// <summary>
    /// Reads a file through SFTP connection.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.ReadFile)
    /// </summary>
    /// <param name="connection">Transfer connection parameters</param>
    /// <param name="input">Read options with full path and encoding</param>
    /// <param name="cancellationToken">Token given by Frends to enable Task termination.</param>
    /// <returns>Result object { string Content, string Path, double SizeInMegaBytes, DateTime LastWriteTime }</returns>
    public static async Task<Result> ReadFile([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
    {
        ConnectionInfo connectionInfo;

        // Establish connectionInfo with connection parameters
        try
        {
            var builder = new ConnectionInfoBuilder(input, connection);
            connectionInfo = builder.BuildConnectionInfo();
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Error when initializing connection info: {e}");
        }

        using var client = new SftpClient(connectionInfo);
        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
        client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
        client.BufferSize = connection.BufferSize * 1024;

        if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
            Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

        // Check the fingerprint of the server if given.
        if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
        {
            var userResultMessage = "";

            try
            {
                userResultMessage = Util.CheckServerFingerprint(client, connection.ServerFingerPrint);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error when checking the server fingerprint: {ex.Message}");
            }
        }


        await client.ConnectAsync(cancellationToken);

        if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");
        var encoding = Util.GetEncoding(input.FileEncoding, input.EnableBom, input.EncodingInString);
        var content = client.ReadAllText(input.Path, encoding);

        var result = new Result(client.Get(input.Path), content);

        client.Disconnect();
        client.Dispose();

        return result;
    }
}