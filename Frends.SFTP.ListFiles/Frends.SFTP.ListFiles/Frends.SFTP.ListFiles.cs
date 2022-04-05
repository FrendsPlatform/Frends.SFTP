using System.ComponentModel;
using Renci.SshNet;
using System.Net.Sockets;
using Renci.SshNet.Common;
using System.Text.RegularExpressions;
using Frends.SFTP.ListFiles.Definitions;

#pragma warning disable 1591
#pragma warning disable 1573

namespace Frends.SFTP.ListFiles
{
    public class SFTP
    {
        /// <summary>
        /// List files from set destination folder with file mask through SFTP connection.
        /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.SFTP.ListFiles)
        /// </summary>
        /// <param name="connection">Transfer connection parameters</param>
        /// <param name="options">Source file location</param>
        /// <returns>List [ Object { string FullPath, bool IsDirectory, bool IsFile, long Length, string Name, DateTime LastWriteTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTime, DateTime LastAccessTime } ]</returns>
        public static List<Result> ListFiles([PropertyTab] Options options, [PropertyTab] Connection connection, CancellationToken cancellationToken)
        {
            // Establish connectionInfo with connection parameters
            var connectionInfo = GetConnectionInfo(connection);
            var result = new List<Result>();
            var regex = "^" + Regex.Escape(options.FileMask).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            var regexStr = string.IsNullOrEmpty(options.FileMask) ? string.Empty : regex;

            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    result = GetFiles(client, regexStr, options.Directory, options, cancellationToken);
                    client.Disconnect();

                }
            }
            catch (SshConnectionException ex)
            {
                throw new Exception($"Error when establishing connection to the Server: {ex.Message}", ex);
            }
            catch (SocketException ex)
            {
                throw new Exception($"Unable to establish the socket: No such host is known.", ex);
            }
            catch (SshAuthenticationException ex)
            {
                throw new Exception($"Authentication of SSH session failed: {ex.Message}", ex);
            }

            return result;
        }

        private static List<Result> GetFiles(SftpClient sftp, string regexStr, string directory, Options options, CancellationToken cancellationToken)
        {
            var directoryList = new List<Result>();

            var files = sftp.ListDirectory(directory);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (file.Name != "." && file.Name != "..")
                {
                    if (options.IncludeType == IncludeType.Both
                        || (file.IsDirectory && options.IncludeType == IncludeType.Directory)
                        || (file.IsRegularFile && options.IncludeType == IncludeType.File))
                    {
                        if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase))
                            directoryList.Add(new Result(file));
                    }

                    if (file.IsDirectory && options.IncludeSubdirectories)
                    {
                        directoryList.AddRange(GetFiles(sftp, regexStr, file.FullName, options, cancellationToken));
                    }
                }
            }
            return directoryList;
        }

        // Helper method to create connection info
        private static ConnectionInfo GetConnectionInfo(Connection connect)
        {
            switch (connect.Authentication)
            {
                case AuthenticationType.PrivateKey:
                    return new PrivateKeyConnectionInfo(connect.Address, connect.Port, connect.Username, new PrivateKeyFile(connect.PrivateKeyFileName));

                case AuthenticationType.PrivateKeyPassphrase:
                    return new PrivateKeyConnectionInfo(connect.Address, connect.Port, connect.Username, new PrivateKeyFile(connect.PrivateKeyFileName, connect.Passphrase));

                default:
                    return new PasswordConnectionInfo(connect.Address, connect.Port, connect.Username, connect.Password);
            }
        }
    }
}
