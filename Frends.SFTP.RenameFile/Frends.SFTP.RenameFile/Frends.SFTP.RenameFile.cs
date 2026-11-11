using System.ComponentModel;
using Frends.SFTP.RenameFile.Definitions;
using Frends.SFTP.RenameFile.Enums;
using Frends.SFTP.RenameFile.Helpers;
using Renci.SshNet;

namespace Frends.SFTP.RenameFile;

/// <summary>
/// Main class of the task.
/// </summary>
public static class SFTP
{
    /// <summary>
    /// Renames a file in SFTP server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.RenameFile)
    /// </summary>
    /// <param name="input">Rename options with full path, new file name and renaming behaviour.</param>
    /// <param name="connection">Transfer connection parameters.</param>
    /// <param name="options">Options for handling errors.</param>
    /// <param name="cancellationToken">Token given by Frends to terminate the Task.</param>
    /// <returns>object { bool Success, string Path, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> RenameFile(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        options ??= new Options();

        try
        {
            ConnectionInfo connectionInfo;

            try
            {
                var builder = new ConnectionInfoBuilder(connection, cancellationToken);
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

            if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

            if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
                Util.AddServerFingerprintCheck(client, connection.ServerFingerPrint);

            await client.ConnectAsync(cancellationToken);

            if (!client.IsConnected)
                throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

            var directory = Path.GetDirectoryName(input.Path).Replace("\\", "/");
            var newFileFullPath = Path.Combine(directory, input.NewFileName).Replace("\\", "/");

            switch (input.RenameBehaviour)
            {
                case RenameBehaviour.Rename:
                    newFileFullPath = GetNonConflictingDestinationFilePath(client, input.Path, newFileFullPath);

                    break;
                case RenameBehaviour.Overwrite:
                    if (client.Exists(newFileFullPath))
                        client.Delete(newFileFullPath);

                    break;
                case RenameBehaviour.Throw:
                    if (client.Exists(newFileFullPath))
                        throw new ArgumentException($"File already exists {newFileFullPath}. No file renamed.");

                    break;
            }

            var file = client.Get(input.Path);
            file.MoveTo(newFileFullPath);

            client.Disconnect();
            client.Dispose();

            return new Result(newFileFullPath);
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static string GetNonConflictingDestinationFilePath(
        SftpClient client,
        string sourceFilePath,
        string destFilePath)
    {
        var count = 1;

        while (client.Exists(destFilePath))
        {
            var tempFileName = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}({count++})";
            destFilePath = Path.Combine(
                Path.GetDirectoryName(destFilePath),
                path2: tempFileName + Path.GetExtension(sourceFilePath)).Replace("\\", "/");
        }

        return destFilePath;
    }
}
