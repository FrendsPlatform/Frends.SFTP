using System.ComponentModel;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using System.Security.Cryptography;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile;

/// <summary>
/// Main class of the task.
/// </summary>
public class SFTP
{
    /// <summary>
    /// Reads a file through SFTP connection.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.ReadFile)
    /// </summary>
    /// <param name="connection">Transfer connection parameters</param>
    /// <param name="input">Read options with full path and encoding</param>
    /// <returns>Result object { string Content, string Path, double SizeInMegaBytes, DateTime LastWriteTime }</returns>
    public static Result ReadFile([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        var encoding = GetEncoding(input.FileEncoding, input.EnableBom, input.EncodingInString);

        ConnectionInfo connectionInfo;
        // Establish connectionInfo with connection parameters
        try
        {
            connectionInfo = GetConnectionInfo(input, connection);
            connectionInfo.Encoding = encoding;
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Error when initializing connection info: {e}");
        }

        using var client = new SftpClient(connectionInfo);

        //Disable support for these host key exchange algorithms relating: https://github.com/FrendsPlatform/Frends.SFTP/security/dependabot/4
        client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256");
        client.ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256@libssh.org");

        // Check the fingerprint of the server if given.
        if (!String.IsNullOrEmpty(connection.ServerFingerPrint))
        {
            var userResultMessage = "";
            try
            {
                // If this check fails then SSH.NET will throw an SshConnectionException - with a message of "Key exchange negotiation failed".
                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    // First try with SHA256 typed fingerprint
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        var sha256Fingerprint = Convert.ToBase64String(mySHA256.ComputeHash(e.HostKey));
                        // Set the userResultMessage in case checking server fingerprint failes.
                        userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{connection.ServerFingerPrint}', but was: '{sha256Fingerprint}'";
                        e.CanTrust = (sha256Fingerprint == connection.ServerFingerPrint);
                    }

                    if (!e.CanTrust)
                    {

                        userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{connection.ServerFingerPrint}', but was: '{BitConverter.ToString(e.FingerPrint).Replace("-", ":")}'";
                        // If previous failed try with MD5 typed fingerprint
                        var expectedFingerprint = ConvertFingerprintToByteArray(connection.ServerFingerPrint);
                        e.CanTrust = e.FingerPrint.SequenceEqual(expectedFingerprint);
                    }

                };
            }
            catch
            {
                throw new ArgumentException($"Error when checking the server fingerprint: {userResultMessage}");
            }
        }

        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout);

        client.BufferSize = connection.BufferSize * 1024;

        client.Connect();

        if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

        var content = client.ReadAllText(input.Path, encoding);

        var result = new Result(client.Get(input.Path), content);

        client.Disconnect();
        client.Dispose();

        return result;
    }

    #region Helper methods
    private static ConnectionInfo GetConnectionInfo(Input input, Connection connect)
    {
        ConnectionInfo connectionInfo;
        List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
        PrivateKeyFile privateKey = null;
        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyFile || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyFile)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyFile))
                throw new ArgumentException("Private key file path was not given.");
            privateKey = (connect.PrivateKeyFilePassphrase != null)
                ? new PrivateKeyFile(connect.PrivateKeyFile, connect.PrivateKeyFilePassphrase)
                : new PrivateKeyFile(connect.PrivateKeyFile);
        }
        if (connect.Authentication == AuthenticationType.UsernamePrivateKeyString || connect.Authentication == AuthenticationType.UsernamePasswordPrivateKeyString)
        {
            if (string.IsNullOrEmpty(connect.PrivateKeyString))
                throw new ArgumentException("Private key string was not given.");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(connect.PrivateKeyString));
            privateKey = (connect.PrivateKeyFilePassphrase != null)
                ? new PrivateKeyFile(stream, connect.PrivateKeyFilePassphrase)
                : new PrivateKeyFile(stream, "");
        }
        switch (connect.Authentication)
        {
            case AuthenticationType.UsernamePassword:
                methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                break;
            case AuthenticationType.UsernamePrivateKeyFile:
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyFile:
                methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            case AuthenticationType.UsernamePrivateKeyString:
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyString:
                methods.Add(new PasswordAuthenticationMethod(connect.UserName, connect.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(connect.UserName, privateKey));
                break;
            default:
                throw new ArgumentException($"Unknown Authentication type: '{connect.Authentication}'.");
        }

        connectionInfo = new ConnectionInfo(connect.Address, connect.Port, connect.UserName, methods.ToArray());

        return connectionInfo;
    }

    /// <summary>
    /// Get encoding for the file name to be transferred.
    /// </summary>
    private static Encoding GetEncoding(FileEncoding optionsFileEncoding, bool optionsEnableBom, string optionsEncodingInString)
    {
        switch (optionsFileEncoding)
        {
            case FileEncoding.UTF8:
                return optionsEnableBom ? new UTF8Encoding(true) : new UTF8Encoding(false);
            case FileEncoding.ASCII:
                return Encoding.ASCII;
            case FileEncoding.ANSI:
                return Encoding.Default;
            case FileEncoding.Unicode:
                return Encoding.Unicode;
            case FileEncoding.WINDOWS1252:
                return Encoding.Default;
            case FileEncoding.Other:
                return Encoding.GetEncoding(optionsEncodingInString);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static byte[] ConvertFingerprintToByteArray(string fingerprint)
    {
        return fingerprint.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
    }

    #endregion
}

