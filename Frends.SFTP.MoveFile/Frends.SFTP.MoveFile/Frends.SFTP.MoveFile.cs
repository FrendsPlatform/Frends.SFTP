using System.ComponentModel;
using Renci.SshNet;
using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile;

/// <summary>
/// Main class of the task.
/// </summary>
public class SFTP
{
    /// <summary>
    /// Moves files in SFTP server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.MoveFile)
    /// </summary>
    /// <param name="connection">Transfer connection parameters</param>
    /// <param name="input">Read options with full path and encoding</param>
    /// <param name="cancellationToken">CancellationToken given by Frends.</param>
    /// <returns>Object { List&lt;FileItem&gt; Files [ { string SourcePath, string DestinationPath } ], string Message }</returns>
    public static async Task<Result> MoveFile([PropertyTab] Input input, [PropertyTab] Connection connection,
        CancellationToken cancellationToken)
    {
        ConnectionInfo connectionInfo;

        // Establish connectionInfo with connection parameters
        try
        {
            var builder = new ConnectionInfoBuilder(connection, input);
            connectionInfo = builder.BuildConnectionInfo();
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Error when initializing connection info: {e}");
        }

        using var client = new SftpClient(connectionInfo);
        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
        client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
        client.KeepAliveInterval = TimeSpan.FromMilliseconds(connection.KeepAliveInterval);
        client.BufferSize = connection.BufferSize * 1024;

        if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
            Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

        // Check the fingerprint of the server if given.
        if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
        {
            Util.AddServerFingerprintCheck(client, connection.ServerFingerPrint);
        }

        await client.ConnectAsync(cancellationToken);

        if (!client.IsConnected)
            throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

        var files = ListSourceFiles(client, input, cancellationToken);

        if (files.Count == 0)
            return new Result(new List<FileItem>(), "No files were found matching the given pattern.");

        var transferredFiles = new List<FileItem>();

        if (input.IfTargetFileExists == FileExistsOperation.Throw)
        {
            var targetFiles = files.Select(x => x.DestinationPath).ToList();
            AssertNoTargetFileConflicts(client, targetFiles);
        }

        if (!client.Exists(input.TargetDirectory))
        {
            if (input.CreateTargetDirectories)
            {
                try
                {
                    Util.CreateDirectoriesRecursively(client, input.TargetDirectory);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"Error while creating destination directory '{input.TargetDirectory}': {ex.Message}", ex);
                }
            }
            else
                throw new DirectoryNotFoundException($"Target directory {input.TargetDirectory} does not exist.");
        }

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (input.IfTargetFileExists)
            {
                case FileExistsOperation.Throw:
                    if (client.Exists(file.DestinationPath))
                    {
                        var filesMoved = transferredFiles.Count > 0 ? transferredFiles.Count.ToString() : "No files";
                        var msg = $"File '{file}' already exists. {filesMoved} moved.";

                        throw new ArgumentException($"File '{file}' already exists.");
                    }

                    transferredFiles.Add(Move(client, file));

                    break;
                case FileExistsOperation.Rename:
                    file.DestinationPath =
                        GetNonConflictingDestinationFilePath(client, file.SourcePath, file.DestinationPath);
                    transferredFiles.Add(Move(client, file));

                    break;
                case FileExistsOperation.Overwrite:
                    if (client.Exists(file.DestinationPath))
                        client.Delete(file.DestinationPath);
                    transferredFiles.Add(Move(client, file));

                    break;
            }
        }

        client.Disconnect();
        client.Dispose();

        return new Result(transferredFiles,
            $"Successfully moved {transferredFiles.Count} files to {input.TargetDirectory}.");
    }

    private static FileItem Move(SftpClient client, FileItem file)
    {
        var sf = client.Get(file.SourcePath);
        sf.MoveTo(file.DestinationPath);

        return file;
    }

    private static List<FileItem> ListSourceFiles(SftpClient client, Input input, CancellationToken cancellationToken)
    {
        var fileItems = new List<FileItem>();

        var files = client.ListDirectory(input.Directory).Where(f => (f.Name != ".") && (f.Name != ".."));

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Name.Equals(input.Pattern) || Util.FileMatchesMask(Path.GetFileName(file.FullName), input.Pattern))
            {
                var item = new FileItem(file.FullName,
                    Path.Combine(input.TargetDirectory, Path.GetFileName(file.FullName)).Replace("\\", "/"));
                fileItems.Add(item);
            }
        }

        return fileItems;
    }

    private static void AssertNoTargetFileConflicts(SftpClient client, List<string> files)
    {
        var duplicateTargetPaths = files.GroupBy(v => v).Where(x => x.Count() > 1).Select(k => k.Key).ToList();

        if (duplicateTargetPaths.Any())
            throw new IOException(
                $"Multiple files written to {string.Join(", ", duplicateTargetPaths)}. The files would get overwritten. No files moved.");

        foreach (var targetFile in files)
        {
            if (client.Exists(targetFile))
                throw new IOException($"File '{targetFile}' already exists. No files moved.");
        }
    }

    private static string GetNonConflictingDestinationFilePath(SftpClient client, string sourceFilePath,
        string destFilePath)
    {
        var count = 1;

        while (client.Exists(destFilePath))
        {
            var tempFileName = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}({count++})";
            destFilePath = Path.Combine(Path.GetDirectoryName(destFilePath),
                path2: tempFileName + Path.GetExtension(sourceFilePath)).Replace("\\", "/");
        }

        return destFilePath;
    }
}
