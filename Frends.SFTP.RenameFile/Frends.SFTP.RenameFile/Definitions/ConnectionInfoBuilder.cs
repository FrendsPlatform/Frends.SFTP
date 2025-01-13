using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Definitions;

internal class ConnectionInfoBuilder
{
    private Connection _connection;
    private CancellationToken _cancellationToken;

    internal ConnectionInfoBuilder(Connection connect, CancellationToken cancellationToken)
    {
        _connection = connect;
        _cancellationToken = cancellationToken;
    }

    internal ConnectionInfo BuildConnectionInfo()
    {
        ConnectionInfo connectionInfo;
        List<AuthenticationMethod> methods = new List<AuthenticationMethod>();

        if (_connection.UseKeyboardInteractiveAuthentication)
        {
            // Construct keyboard-interactive authentication method
            var kauth = new KeyboardInteractiveAuthenticationMethod(_connection.Username);
            kauth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
            methods.Add(kauth);
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
                : new PrivateKeyFile(stream, string.Empty);
        }

        switch (_connection.Authentication)
        {
            case AuthenticationType.UsernamePassword:
                methods.Add(new PasswordAuthenticationMethod(_connection.Username, _connection.Password));
                break;
            case AuthenticationType.UsernamePrivateKeyFile:
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyFile or AuthenticationType.UsernamePasswordPrivateKeyString:
                methods.Add(new PasswordAuthenticationMethod(_connection.Username, _connection.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePrivateKeyString:
                methods.Add(new PrivateKeyAuthenticationMethod(_connection.Username, privateKey));
                break;
            default:
                throw new ArgumentException($"Unknown Authentication type: '{_connection.Authentication}'.");
        }

        connectionInfo = new ConnectionInfo(_connection.Address, _connection.Port, _connection.Username, methods.ToArray())
        {
            ChannelCloseTimeout = TimeSpan.FromSeconds(_connection.ConnectionTimeout),
            Timeout = TimeSpan.FromSeconds(_connection.ConnectionTimeout)
        };

        return connectionInfo;
    }

    private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
    {
        if (e.Prompts.Any())
        {
            foreach (var serverPrompt in e.Prompts)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrEmpty(_connection.Password) && serverPrompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    serverPrompt.Response = _connection.Password;
                }
                else
                {
                    if (!_connection.PromptAndResponse.Any() || !_connection.PromptAndResponse.Select(p => p.Prompt.ToLower()).ToList().Contains(serverPrompt.Request.Replace(":", string.Empty).Trim().ToLower()))
                    {
                        var errorMsg = $"Failure in Keyboard-interactive authentication: No response given for server prompt request --> {serverPrompt.Request.Replace(":", string.Empty).Trim()}";
                        throw new ArgumentException(errorMsg);
                    }

                    foreach (var prompt in _connection.PromptAndResponse)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        if (serverPrompt.Request.IndexOf(prompt.Prompt, StringComparison.InvariantCultureIgnoreCase) != -1)
                            serverPrompt.Response = prompt.Response;
                    }
                }
            }
        }
    }
}
