using System.ComponentModel;
using Renci.SshNet;
using Frends.SFTP.RenameFile.Definitions;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile;

/// <summary>
/// Main class of the task.
/// </summary>
public class SFTP
{
    /// <summary>
    /// Renames a file in SFTP server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.RenameFile)
    /// </summary>
    /// <param name="connection">Transfer connection parameters.</param>
    /// <param name="input">Rename options with full path, new file name and renaming behaviour.</param>
    /// <param name="cancellationToken">Token given by Frends to terminate the Task.</param>
    /// <returns>Result object { string Path }</returns>
    public static async Task<Result> RenameFile([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
    {
        ConnectionInfo connectionInfo;
        // Establish connectionInfo with connection parameters
        try
        {
            var builder = new ConnectionInfoBuilder(connection, cancellationToken);
            connectionInfo = builder.BuildConnectionInfo();
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Error when initializing connection info: {e}");
        }

        using var client = new SftpClient(connectionInfo);

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
            catch (Exception ex) { throw new ArgumentException($"Error when checking the server fingerprint: {ex.Message}"); }
        }

        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);

        client.BufferSize = connection.BufferSize * 1024;

        await client.ConnectAsync(cancellationToken);

        if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

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

    private static string GetNonConflictingDestinationFilePath(SftpClient client, string sourceFilePath, string destFilePath)
    {
        var count = 1;
        while (client.Exists(destFilePath))
        {
            var tempFileName = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}({count++})";
            destFilePath = Path.Combine(Path.GetDirectoryName(destFilePath), path2: tempFileName + Path.GetExtension(sourceFilePath)).Replace("\\", "/");
        }

        return destFilePath;
    }
}

