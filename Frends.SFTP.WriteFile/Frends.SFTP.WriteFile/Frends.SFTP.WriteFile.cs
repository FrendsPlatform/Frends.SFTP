using System.ComponentModel;
using Renci.SshNet;

#pragma warning disable 1591

namespace Frends.SFTP.WriteFile
{
    public class SFTP
    {

        /// <summary>
        /// Writes a file to SFTP server.
        /// Documentation: https://github.com/FrendsPlatform/Frends.SFTP.WriteFile
        /// </summary>
        /// <param name="connection">Transfer connection parameters</param>
        /// <param name="source">Source file location</param>
        /// <param name="destination">Destination directory location</param>
        /// <param name="cancellationToken"></param>
        /// <returns>{string Replication} </returns>
        public static Result WriteFile([PropertyTab] Source source, [PropertyTab] Destination destination, [PropertyTab] Connection connection, CancellationToken cancellationToken)
        {
            return new SFTP().WriteFileInternal(connection, source, destination, cancellationToken);
        }

        internal Result WriteFileInternal(Connection connection, Source source, Destination destination, CancellationToken cancellationToken)
        {
            // Establish connectionInfo with connection parameters
            var connectionInfo = GetConnectionInfo(connection);
            var result = new Result();
            
            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    client.ChangeDirectory(destination.Directory);
                    client.BufferSize = 1024;
                    result = TransferSingleFile(client, source, destination);
                    client.Disconnect();

                }
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception($"Error when establishing connection to the Server: {ex.Message}", ex);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                throw new Exception($"Unable to establish the socket: No such host is known.", ex);
            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
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
                case Connection.AuthenticationType.PrivateKey:
                    return new PrivateKeyConnectionInfo(connect.Address, connect.Port, connect.UserName, new PrivateKeyFile(connect.PrivateKeyFileName));

                case Connection.AuthenticationType.PrivateKeyPassphrase:
                    return new PrivateKeyConnectionInfo(connect.Address, connect.Port, connect.UserName, new PrivateKeyFile(connect.PrivateKeyFileName, connect.Passphrase));

                default:
                    return new PasswordConnectionInfo(connect.Address, connect.Port, connect.UserName, connect.Password);
            }
        }

        private static Result TransferSingleFile(SftpClient client, Source source, Destination destination)
        {
            using (FileStream fs = new FileStream(Path.Combine(source.Directory, source.FileName), FileMode.Open))
            {
                if (CheckIfFileExists(client, source.FileName))
                {
                    switch (destination.Operation)
                    {
                        case Destination.DestinationOperation.Rename:
                            var newFile = RenameFile(client, source, destination);
                            client.UploadFile(fs, newFile, false);
                            if (destination.Directory.StartsWith("/"))
                                return new Result(newFile, Path.Combine(source.Directory, source.FileName), destination.Directory + "/" + newFile, true);
                            return new Result(newFile, Path.Combine(source.Directory, source.FileName), Path.Combine(destination.Directory, newFile), true);

                        case Destination.DestinationOperation.Overwrite:
                            client.UploadFile(fs, source.FileName, true);
                            if (destination.Directory.StartsWith("/"))
                                return new Result(source.FileName, Path.Combine(source.Directory, source.FileName), destination.Directory + "/" + source.FileName, true);
                            return new Result(source.FileName, Path.Combine(source.Directory, source.FileName), Path.Combine(destination.Directory, source.FileName), true);

                        default:
                            throw new Exception("Error in uploading the file: The destination file already exists.");
                    }
                }

                client.UploadFile(fs, source.FileName, false);
                return new Result(source.FileName, Path.Combine(source.Directory, source.FileName), Path.Combine(destination.Directory, source.FileName), true);
            }
        }

        // Finds all the files in directory and checks if source file exists in destination directory.
        private static bool CheckIfFileExists(SftpClient client, string fileName)
        {
            foreach (var file in client.ListDirectory("."))
            {
                if (file.Name.Equals(fileName))
                {
                    return true;
                }
            }

            return false;
        }

        private static string RenameFile(SftpClient client, Source source, Destination destination)
        {
            var files = ListDestinationFileNames(client);
            var file = source.FileName;
            var extension = Path.GetExtension(file);
            int index = 1;
            while (files.Contains(Path.GetFileName(file)))
            {
                var filename = string.Format("{0}({1})", Path.GetFileNameWithoutExtension(source.FileName), index++);
                file = filename + extension;
            }

            return file;
        }

        private static List<string> ListDestinationFileNames(SftpClient client)
        {
            var files = client.ListDirectory(".");
            var result = new List<string>();

            foreach (var file in files)
            {
                result.Add(file.Name);
            }

            return result;
        }
    }
}
