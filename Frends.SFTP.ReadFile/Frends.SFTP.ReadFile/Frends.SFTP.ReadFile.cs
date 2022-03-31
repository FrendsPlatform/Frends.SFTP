using System.ComponentModel;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net.Sockets;
using Frends.SFTP.ReadFile.Definitions;

#pragma warning disable 1591

namespace Frends.SFTP.ReadFile
{
    public class SFTP
    {
        /// <summary>
        /// Reads a file with SFTP connection.
        /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.SFTP.ReadFile)
        /// </summary>
        /// <param name="connection">Transfer connection parameters</param>
        /// <param name="source">Source file location</param>
        /// <param name="destination">Destination directory location</param>
        /// <returns>Result object { string name, string sourcePath, string destinationPath, bool success }</returns>
        public static Result ReadFile([PropertyTab] Source source, [PropertyTab] Destination destination, [PropertyTab] Connection connection, CancellationToken cancellationToken)
        {
            // Establish connectionInfo with connection parameters
            var connectionInfo = GetConnectionInfo(connection);
            Result result = null;
            
            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    client.ChangeDirectory(source.Directory);
                    client.BufferSize = 1024;
                    result = TransferSingleFile(client, source, destination);
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

        // Helper method to create connection info
        private static ConnectionInfo GetConnectionInfo(Connection connect)
        {
            switch (connect.Authentication)
            {
                case AuthenticationType.PrivateKey:
                    return new PrivateKeyConnectionInfo(connect.Address, connect.Port, connect.UserName, new PrivateKeyFile(connect.PrivateKeyFileName));

                case AuthenticationType.PrivateKeyPassphrase:
                    return new PrivateKeyConnectionInfo(connect.Address, connect.Port, connect.UserName, new PrivateKeyFile(connect.PrivateKeyFileName, connect.Passphrase));

                default:
                    return new PasswordConnectionInfo(connect.Address, connect.Port, connect.UserName, connect.Password);
            }
        }

        private static Result TransferSingleFile(SftpClient client, Source source, Destination destination)
        {
            var sourceFilePath = "";
            var destinationFilePath = Path.Combine(destination.Directory, source.FileName);
            if (source.Directory.StartsWith("/"))
                sourceFilePath = source.Directory + "/" + source.FileName;
            else 
                sourceFilePath = Path.Combine(source.Directory, source.FileName);

            // check if file exists in local folder
            if (File.Exists(Path.Combine(destination.Directory, source.FileName)))
            {
                switch (destination.Operation)
                {
                    case DestinationOperation.Rename:
                        destinationFilePath = RenameFile(source, destination);
                        break;
                    case DestinationOperation.Overwrite:
                        break;
                    default:
                        throw new Exception("Error in downloading the file: The destination file already exists.");
                }
            }

            using (Stream fs = File.Create(destinationFilePath))
            {
                client.DownloadFile(source.FileName, fs);
                return new Result(source.FileName, sourceFilePath, destinationFilePath, true);
            }
        }

        private static string RenameFile(Source source, Destination destination)
        {
            var file = source.FileName;
            var extension = Path.GetExtension(file);
            int index = 1;
            while (File.Exists(Path.Combine(destination.Directory, file)))
            {
                var filename = string.Format("{0}({1})", Path.GetFileNameWithoutExtension(source.FileName), index++);
                file = filename + extension;
            }

            return Path.Combine(destination.Directory, file);
        }
    }
}
