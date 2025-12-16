using System.ComponentModel;
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
        /// <returns>Object { int FileCount, List&lt;FileItem&gt; Files }</returns>
        public static async Task<Result> ListFiles([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
        {
            using var timeoutCts = connection.MaxExecutionTimeout > 0
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;

            if (timeoutCts != null)
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(connection.MaxExecutionTimeout));

            var effectiveToken = timeoutCts?.Token ?? cancellationToken;

            try
            {
                return await Task.Run(async () =>
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

                    await client.ConnectAsync(effectiveToken);

                    if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

                    var regex = "^" + Regex.Escape(input.FileMask).Replace("\\?", ".").Replace("\\*", ".*") + "$";
                    var regexStr = string.IsNullOrEmpty(input.FileMask) ? string.Empty : regex;
                    var files = GetFiles(client, regexStr, input.Directory, input, effectiveToken);
                    client.Disconnect();
                    client.Dispose();

                    return new Result(files);

                }, effectiveToken);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException(
                    $"SFTP operation exceeded maximum execution time of {connection.MaxExecutionTimeout} seconds.");
            }
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
                        if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase) || FileMatchesMask(file.Name, input.FileMask))
                            directoryList.Add(new FileItem(file));
                    }

                    if (file.IsDirectory && input.IncludeSubdirectories)
                        directoryList.AddRange(GetFiles(sftp, regexStr, file.FullName, input, cancellationToken));
                }
            }
            return directoryList;
        }

        private static bool FileMatchesMask(string filename, string mask)
        {
            const string regexEscape = "<regex>";
            string pattern;

            //check is pure regex wished to be used for matching
            if (mask != null && mask.StartsWith(regexEscape))
                //use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
                pattern = mask.Substring(regexEscape.Length);
            else
            {
                pattern = mask.Replace(".", "\\.");
                pattern = pattern.Replace("*", ".*");
                pattern = pattern.Replace("?", ".+");
                pattern = string.Concat("^", pattern, "$");
            }

            return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
        }
    }
}
