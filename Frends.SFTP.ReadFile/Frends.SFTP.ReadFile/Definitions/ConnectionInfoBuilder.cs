using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Definitions;

internal class ConnectionInfoBuilder
{
    private static Input _input;
    private static Connection _connection;

    internal ConnectionInfoBuilder(Input input, Connection connect)
    {
        _input = input;
        _connection = connect;
    }

    internal ConnectionInfo BuildConnectionInfo()
    {
        ConnectionInfo connectionInfo;
        var methods = new List<AuthenticationMethod>();

        if (_connection.UseKeyboardInteractiveAuthentication)
        {
            try
            {
                // Construct keyboard-interactive authentication method
                var kauth = new KeyboardInteractiveAuthenticationMethod(_connection.Username);
                kauth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
                methods.Add(kauth);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failure in Keyboard-Interactive authentication: {ex.Message}");
            }
        }

        PrivateKeyFile privateKey = null;
        if (_connection.Authentication == AuthenticationType.UsernamePrivateKeyFile || _connection.Authentication == AuthenticationType.UsernamePasswordPrivateKeyFile)
        {
            if (string.IsNullOrEmpty(_connection.PrivateKeyFile))
                throw new ArgumentException("Private key file path was not given.");
            privateKey = (_connection.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(_connection.PrivateKeyFile, _connection.PrivateKeyPassphrase)
                : new PrivateKeyFile(_connection.PrivateKeyFile);
        }
        if (_connection.Authentication == AuthenticationType.UsernamePrivateKeyString || _connection.Authentication == AuthenticationType.UsernamePasswordPrivateKeyString)
        {
            if (string.IsNullOrEmpty(_connection.PrivateKeyString))
                throw new ArgumentException("Private key string was not given.");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(_connection.PrivateKeyString));
            privateKey = (_connection.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(stream, _connection.PrivateKeyPassphrase)
                : new PrivateKeyFile(stream);
        }
        switch (_connection.Authentication)
        {
            case AuthenticationType.UsernamePassword:
                methods.Add(new PasswordAuthenticationMethod(_connection.Username, _connection.Password));
                break;
            case AuthenticationType.UsernamePrivateKeyFile:
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyFile:
                methods.Add(new PasswordAuthenticationMethod(_connection.Username, _connection.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePrivateKeyString:
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyString:
                methods.Add(new PasswordAuthenticationMethod(_connection.Username, _connection.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            default:
                throw new ArgumentException($"Unknown Authentication type: '{_connection.Authentication}'.");
        }

        connectionInfo = new ConnectionInfo(_connection.Address, _connection.Port, _connection.Username, methods.ToArray())
        {
            Encoding = Util.GetEncoding(_input.FileEncoding, _input.EnableBom, _input.EncodingInString),
            ChannelCloseTimeout = TimeSpan.FromSeconds(_connection.ConnectionTimeout),
            Timeout = TimeSpan.FromSeconds(_connection.ConnectionTimeout)
        };

        return connectionInfo;
    }

    private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
    {
        foreach (var prompt in e.Prompts)
            if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                prompt.Response = _connection.Password;
    }
}
