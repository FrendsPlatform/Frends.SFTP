﻿using System.ComponentModel;
using Renci.SshNet;
using System.Text.RegularExpressions;
using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;

namespace Frends.SFTP.ListFiles
{
    /// <summary>
    /// Main class of the Task
    /// </summary>
    public class SFTP
    {
        /// <summary>
        /// List files from set destination directory with optional file mask through SFTP connection.
        /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.ListFiles)
        /// </summary>
        /// <param name="connection">Transfer connection parameters</param>
        /// <param name="input">Source file location</param>
        /// <param name="cancellationToken">CancellationToken is given by the Frends UI</param>
        /// <returns>Object { int Count, List Files }</returns>
        public static Result ListFiles([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
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

            //Disable support for these host key exchange algorithms relating: https://github.com/FrendsPlatform/Frends.SFTP/security/dependabot/4
            client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256");
            client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256@libssh.org");

            if (connection.HostKeyAlgorithm != HostKeyAlgorithms.Any)
                Util.ForceHostKeyAlgorithm(client, connection.HostKeyAlgorithm);

            var expectedServerFingerprint = connection.ServerFingerPrint;
            // Check the fingerprint of the server if given.
            if (!string.IsNullOrEmpty(expectedServerFingerprint))
            {
                var userResultMessage = "";
                try
                {
                    // If this check fails then SSH.NET will throw an SshConnectionException - with a message of "Key exchange negotiation failed".
                    userResultMessage = Util.CheckServerFingerprint(client, expectedServerFingerprint);
                }
                catch
                {
                    throw new ArgumentException($"Error when checking the server fingerprint: {userResultMessage}");
                }
            }

            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.OperationTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);
            client.KeepAliveInterval = TimeSpan.FromSeconds(connection.ConnectionTimeout);

            client.Connect();

            if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

            var regex = "^" + Regex.Escape(input.FileMask).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            var regexStr = string.IsNullOrEmpty(input.FileMask) ? string.Empty : regex;
            var files = GetFiles(client, regexStr, input.Directory, input, cancellationToken);

            client.Disconnect();
            client.Dispose();
            
            return new Result(files);
        }

        private static List<FileItem> GetFiles(SftpClient sftp, string regexStr, string directory, Input input, CancellationToken cancellationToken)
        {
            var directoryList = new List<FileItem>();

            var files = sftp.ListDirectory(directory);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (file.Name != "." && file.Name != "..")
                {
                    if (input.IncludeType == IncludeType.Both
                        || (file.IsDirectory && input.IncludeType == IncludeType.Directory)
                        || (file.IsRegularFile && input.IncludeType == IncludeType.File))
                    {
                        if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase))
                            directoryList.Add(new FileItem(file));
                    }

                    if (file.IsDirectory && input.IncludeSubdirectories)
                        directoryList.AddRange(GetFiles(sftp, regexStr, file.FullName, input, cancellationToken));
                }
            }
            return directoryList;
        }
    }
}
