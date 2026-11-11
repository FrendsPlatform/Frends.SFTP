using System.Text;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Frends.SFTP.ReadFile.Helpers;

internal class ConnectionInfoBuilder
{
    private readonly Input input;
    private readonly Connection connection;

    internal ConnectionInfoBuilder(Input input, Connection connect)
    {
        this.input = input;
        connection = connect;
    }

    internal ConnectionInfo BuildConnectionInfo()
    {
        ConnectionInfo connectionInfo;
        var methods = new List<AuthenticationMethod>();

        if (connection.UseKeyboardInteractiveAuthentication)
        {
            try
            {
                // Construct keyboard-interactive authentication method
                var kauth = new KeyboardInteractiveAuthenticationMethod(connection.Username);
                kauth.AuthenticationPrompt += HandleKeyEvent;
                methods.Add(kauth);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failure in Keyboard-Interactive authentication: {ex.Message}");
            }
        }

        PrivateKeyFile privateKey = null;
        if (connection.Authentication == AuthenticationType.UsernamePrivateKeyFile || connection.Authentication == AuthenticationType.UsernamePasswordPrivateKeyFile)
        {
            if (string.IsNullOrEmpty(connection.PrivateKeyFile))
                throw new ArgumentException("Private key file path was not given.");
            privateKey = (connection.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(connection.PrivateKeyFile, connection.PrivateKeyPassphrase)
                : new PrivateKeyFile(connection.PrivateKeyFile);
        }

        if (connection.Authentication == AuthenticationType.UsernamePrivateKeyString || connection.Authentication == AuthenticationType.UsernamePasswordPrivateKeyString)
        {
            if (string.IsNullOrEmpty(connection.PrivateKeyString))
                throw new ArgumentException("Private key string was not given.");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(connection.PrivateKeyString));
            privateKey = (connection.PrivateKeyPassphrase != null)
                ? new PrivateKeyFile(stream, connection.PrivateKeyPassphrase)
                : new PrivateKeyFile(stream);
        }

        switch (connection.Authentication)
        {
            case AuthenticationType.UsernamePassword:
                methods.Add(new PasswordAuthenticationMethod(connection.Username, connection.Password));
                break;
            case AuthenticationType.UsernamePrivateKeyFile:
                methods.Add(new PrivateKeyAuthenticationMethod(connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyFile:
                methods.Add(new PasswordAuthenticationMethod(connection.Username, connection.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePrivateKeyString:
                methods.Add(new PrivateKeyAuthenticationMethod(connection.Username, privateKey));
                break;
            case AuthenticationType.UsernamePasswordPrivateKeyString:
                methods.Add(new PasswordAuthenticationMethod(connection.Username, connection.Password));
                methods.Add(new PrivateKeyAuthenticationMethod(connection.Username, privateKey));
                break;
            default:
                throw new ArgumentException($"Unknown Authentication type: '{connection.Authentication}'.");
        }

        connectionInfo = new ConnectionInfo(connection.Address, connection.Port, connection.Username, methods.ToArray())
        {
            Encoding = Util.GetEncoding(input.FileEncoding, input.EnableBom, input.EncodingInString),
            ChannelCloseTimeout = TimeSpan.FromSeconds(connection.ConnectionTimeout),
            Timeout = TimeSpan.FromSeconds(connection.ConnectionTimeout),
        };

        return connectionInfo;
    }

    private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
    {
        if (e.Prompts.Any())
        {
            foreach (var serverPrompt in e.Prompts)
            {
                if (!string.IsNullOrEmpty(connection.Password) && serverPrompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    serverPrompt.Response = connection.Password;
                }
                else
                {
                    if (!connection.PromptAndResponse.Any() || !connection.PromptAndResponse.Select(p => p.Prompt.ToLower()).ToList().Contains(serverPrompt.Request.Replace(":", string.Empty).Trim().ToLower()))
                    {
                        var errorMsg = $"Failure in Keyboard-interactive authentication: No response given for server prompt request --> {serverPrompt.Request.Replace(":", string.Empty).Trim()}";
                        throw new ArgumentException(errorMsg);
                    }

                    foreach (var prompt in connection.PromptAndResponse)
                    {
                        if (serverPrompt.Request.IndexOf(prompt.Prompt, StringComparison.InvariantCultureIgnoreCase) != -1)
                            serverPrompt.Response = prompt.Response;
                    }
                }
            }
        }
    }
}
