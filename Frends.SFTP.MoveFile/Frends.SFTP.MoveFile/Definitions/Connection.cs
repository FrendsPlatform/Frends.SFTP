using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Parameters class usually contains parameters that are required.
/// </summary>
public class Connection
{
    /// <summary>
    /// The lenght of time, in seconds, until the connection times out.
    /// You can use value -1 to indicate that the connection does not time out.
    /// Default value is 60 seconds.
    /// </summary>
    /// <example><c>60</c>.</example>
    [DefaultValue(60)]
    public int ConnectionTimeout { get; set; }

    /// <summary>
    /// The keep-alive interval in milliseconds. Interval the client send keep-alive packages to the host.
    /// You can use value -1 to disable the keep-alive.
    /// </summary>
    /// <example><c>-1</c>.</example>
    [DefaultValue(-1)]
    public int KeepAliveInterval { get; set; }

    /// <summary>
    /// SFTP host address
    /// </summary>
    /// <example><c>localhost</c>.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Address { get; set; }

    /// <summary>
    /// Port number to use in the connection to the server.
    /// </summary>
    /// <example><c>22</c>.</example>
    [DefaultValue(22)]
    public int Port { get; set; } = 22;

    /// <summary>
    /// Selection for authentication type
    /// </summary>
    /// <example><c>AuthenticationType.UsernamePassword</c>.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public AuthenticationType Authentication { get; set; } = AuthenticationType.UsernamePassword;

    /// <summary>
    /// Username to use for authentication to the server. Note that the file endpoint only supports
    /// username for remote shares and the username must be in the format DOMAIN\Username.
    /// </summary>
    /// <example><c>foo</c>.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Username { get; set; }

    /// <summary>
    /// Password to use in the authentication to the server.
    /// </summary>
    /// <example><c>pass</c>.</example>
    [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePassword, AuthenticationType.UsernamePasswordPrivateKeyFile, AuthenticationType.UsernamePasswordPrivateKeyString)]
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// Full path to private key file. Supported private key formats: OpenSSH and ssh.com.
    /// PuTTY keys can be converted with puttygen.exe application.
    /// 1. Load your key file into puttygen.exe
    /// 2. Conversion > Export OpenSSH key (not the "force new file format" option).
    /// </summary>
    /// <example><c>C:\path\to\private\key</c>.</example>
    [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKeyFile, AuthenticationType.UsernamePasswordPrivateKeyFile)]
    [DisplayFormat(DataFormatString = "Text")]
    public string PrivateKeyFile { get; set; }

    /// <summary>
    /// Private key as a string, supported private key formats: OpenSSH and ssh.com.
    /// PuTTY keys can be converted with puttygen.exe application.
    /// 1. Load your key file into puttygen.exe
    /// 2. Conversion > Export OpenSSH key (not the "force new file format" option).
    /// </summary>
    /// <example>
    /// -----BEGIN RSA PRIVATE KEY-----
    /// Fqxq2jbSKyb0a+oW96Tjoif3Kcb5zZ0FiQyiHgQozLXrecjdUwjWuedkDoZMxwG5
    /// bxpOnxZ/88tDzYCtCPcYCPRF8BNueUsZO8/tztTra+4NgVd/omXHG5bqb7iMB4dc
    /// ...
    /// OX7Q/wO4lqOlFhLtRnSL0cfuhRmt59pM75Zd+euX5tv9jmCj+AQT/kiBoMhNrDGk
    /// N2gTujnH7HCr/afSBeL3xnYcEmeCQTxTPZofBjPC+TPd9g7MntSGBeU/Fstv0jbg
    /// <c>-----END RSA PRIVATE KEY-----</c>.
    /// </example>
    [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKeyString, AuthenticationType.UsernamePasswordPrivateKeyString)]
    [PasswordPropertyText]
    public string PrivateKeyString { get; set; }

    /// <summary>
    /// Passphrase for the private key file.
    /// </summary>
    /// <example><c>passphrase</c>.</example>
    [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKeyFile, AuthenticationType.UsernamePasswordPrivateKeyFile, AuthenticationType.UsernamePrivateKeyString, AuthenticationType.UsernamePasswordPrivateKeyString)]
    [PasswordPropertyText]
    public string PrivateKeyPassphrase { get; set; }

    /// <summary>
    /// Fingerprint of the SFTP server. When using "Username-Password" 
    /// authentication it is recommended to use server fingerprint in 
    /// order to be sure of the server you are connecting.
    /// </summary>
    /// <example>
    /// <c>MD5: 41:76:EA:65:62:6E:D3:68:DC:41:9A:F2:F2:20:69:9D</c>
    /// <c>SHA256: FBQn5eyoxpAl33Ly0gyScCGAqZeMVsfY7qss3KOM/hY=</c>.
    /// </example>
    [DefaultValue("")]
    public string ServerFingerPrint { get; set; }

    /// <summary>
    /// Host key algorithm to use when connecting to server. 
    /// Default value is Any which doesn't force the task to use 
    /// specific algorithm.
    /// </summary>
    /// <example><c>HostKeyAlgorithms.RSA</c>.</example>
    [DefaultValue(HostKeyAlgorithms.Any)]
    public HostKeyAlgorithms HostKeyAlgorithm { get; set; }

    /// <summary>
    /// Enable if the server uses keyboard-interactive authentication method.
    /// </summary>
    /// <example><c>false</c>.</example>
    [DefaultValue(false)]
    public bool UseKeyboardInteractiveAuthentication { get; set; }

    /// <summary>
    /// Responses for the server prompts when using Keyboard Interactive authentication method.
    /// </summary>
    /// <example><c>[object { Prompt: Verification code, Response: 123456789 }]</c>.</example>
    [UIHint(nameof(UseKeyboardInteractiveAuthentication), "", true)]
    public PromptResponse[] PromptAndResponse { get; set; } = Array.Empty<PromptResponse>();

    /// <summary>
    /// Integer value of used buffer size as KB.
    /// Default value is 32 KB.
    /// </summary>
    /// <example><c>32</c>.</example>
    [DefaultValue(32)]
    public uint BufferSize { get; set; }
}
