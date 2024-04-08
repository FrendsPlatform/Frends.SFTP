namespace Frends.SFTP.DeleteDirectory;

using Frends.SFTP.DeleteDirectory.Definitions;
using Frends.SFTP.DeleteDirectory.Enums;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Main class of the Task.
/// </summary>
public static class SFTP
{
    /// <summary>
    /// Frends Task for deleting directory from SFTP server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.DeleteDirectory).
    /// </summary>
    /// <param name="input">Input parameters</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Options parameters.</param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>Object { bool Success, List&lt;string&gt; Data, dynamic ErrorMessage }.</returns>
    public static async Task<Result> DeleteDirectory([PropertyTab] Input input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        var deleted = new List<string>();
        ConnectionInfo connectionInfo;

        try
        {
            var builder = new ConnectionInfoBuilder(input, connection, cancellationToken);
            connectionInfo = builder.BuildConnectionInfo();

            using var client = new SftpClient(connectionInfo);

            if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

            if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
                Util.CheckServerFingerprint(client, connection.ServerFingerPrint);

            client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.KeepAliveInterval = TimeSpan.FromSeconds(connection.ConnectionTimeout);

            await client.ConnectAsync(cancellationToken);

            if (!client.Exists(input.Directory) && options.ThrowNotExistError is NotExistsOptions.Throw)
                throw new ArgumentException($"Directory {input.Directory} does not exists.");
            if (!client.Exists(input.Directory) && options.ThrowNotExistError is NotExistsOptions.Skip)
                return new Result(true, deleted, $"Directory {input.Directory} does not exists.");

            client.ChangeDirectory(input.Directory);
            var files = client.ListDirectory(input.Directory);
            var validFiles = files.Where(file => file.Name != "." && file.Name != "..");

            foreach (var file in validFiles)
            {
                if (file.IsDirectory)
                {
                    client.ChangeDirectory(file.FullName);
                    var directoryFiles = client.ListDirectory(".").Where(f => f.Name != "." && f.Name != "..");

                    foreach (var f in directoryFiles)
                        client.DeleteFile(f.Name);

                    client.ChangeDirectory(input.Directory);
                    client.DeleteDirectory(file.FullName);
                }
                else
                {
                    client.DeleteFile(file.FullName);
                }

                deleted.Add(file.FullName);
            }
        }
        catch (Exception ex)
        {
            if (options.ThrowExceptionOnError)
                throw;
            return new Result(false, deleted, ex);
        }

        return new Result(true, deleted, null);
    }
}
