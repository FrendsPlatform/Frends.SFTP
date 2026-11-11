namespace Frends.SFTP.DeleteFiles;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.DeleteFiles.Definitions;
using Frends.SFTP.DeleteFiles.Enums;
using Frends.SFTP.DeleteFiles.Helpers;
using Renci.SshNet;
using Renci.SshNet.Common;

/// <summary>
/// Main class of the Task.
/// </summary>
public static class SFTP
{
    /// <summary>
    /// Frends Task for deleting files from SFTP server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.DeleteFiles).
    /// </summary>
    /// <param name="input">Input parameters</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>object { bool Success, List&lt;FileItem&gt; Files, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> DeleteFiles(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            var deletedFiles = new List<FileItem>();
            var builder = new ConnectionInfoBuilder(input, connection, cancellationToken);
            var connectionInfo = builder.BuildConnectionInfo();

            using var client = new SftpClient(connectionInfo);
            client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.KeepAliveInterval = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.BufferSize = connection.BufferSize * 1024;

            if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

            if (!string.IsNullOrEmpty(connection.ServerFingerPrint))
                Util.AddServerFingerprintCheck(client, connection.ServerFingerPrint);

            await client.ConnectAsync(cancellationToken);

            if (!client.IsConnected)
                throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

            var files = await GetFiles(client, input, cancellationToken);

            foreach (var file in files)
            {
                if (client.Exists(file.Path))
                {
                    client.DeleteFile(file.Path);
                    deletedFiles.Add(file);
                }
            }

            return new Result
            {
                Success = true,
                Files = deletedFiles,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static async Task<List<FileItem>> GetFiles(SftpClient sftp, Input input, CancellationToken cancellationToken)
    {
        var directoryList = new List<FileItem>();
        var filePaths = ConvertObjectToStringArray(input.FilePaths);

        if (filePaths != null)
        {
            foreach (var file in filePaths.ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();
                directoryList.Add(new FileItem(sftp.Get(file)));
            }

            return directoryList;
        }

        var regex = "^" + Regex.Escape(input.FileMask).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        var regexStr = string.IsNullOrEmpty(input.FileMask) ? string.Empty : regex;

        try
        {
            var files = sftp.ListDirectoryAsync(input.Directory, cancellationToken).ConfigureAwait(false);

            await foreach (var file in files.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (file.Name != "." && file.Name != ".." && !file.IsDirectory &&
                    Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase))
                    directoryList.Add(new FileItem(file));
            }

            return directoryList;
        }
        catch (SftpPathNotFoundException ex)
        {
            throw new SftpPathNotFoundException($"No such Directory '{input.Directory}'.", ex);
        }
    }

    private static string[] ConvertObjectToStringArray(object objectArray)
    {
        var res = objectArray as object[];

        return res?.OfType<string>().ToArray();
    }
}
