using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions
{
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
        [DefaultValue(60)]
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// SFTP host address
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Address { get; set; }

        /// <summary>
        /// Port number to use in the connection to the server.
        /// </summary>
        [DefaultValue(22)]
        public int Port { get; set; } = 22;

        /// <summary>
        /// Selection for authentication type
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public AuthenticationType Authentication { get; set; } = AuthenticationType.UsernamePassword;

        /// <summary>
        /// Username to use for authentication to the server. Note that the file endpoint only supports
        /// username for remote shares and the username must be in the format DOMAIN\Username.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string UserName { get; set; }

        /// <summary>
        /// Password to use in the authentication to the server.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePassword, AuthenticationType.UsernamePasswordPrivateKeyFile, AuthenticationType.UsernamePasswordPrivateKeyString)]
        [PasswordPropertyText]
        public string Password { get; set; }

        /// <summary>
        /// Full path to private key file.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKeyFile, AuthenticationType.UsernamePasswordPrivateKeyFile)]
        [DisplayFormat(DataFormatString = "Text")]
        public string PrivateKeyFile { get; set; }

        /// <summary>
        /// Private key as a string, supported private key formats: PKCS#8,
        /// OpenSSH/OpenSSL and PuTTY.ppk.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKeyString, AuthenticationType.UsernamePasswordPrivateKeyString)]
        [PasswordPropertyText]
        public string PrivateKeyString { get; set; }

        /// <summary>
        /// Passphrase for the private key file.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKeyFile, AuthenticationType.UsernamePasswordPrivateKeyFile)]
        [PasswordPropertyText]
        public string PrivateKeyFilePassphrase { get; set; }

        /// <summary>
        /// Fingerprint of the SFTP server. When using "Username-Password" 
        /// authentication it is recommended to use server fingerprint in 
        /// order to be sure of the server you are connecting.
        /// </summary>
        [DefaultValue("")]
        public string ServerFingerPrint { get; set; }

        /// <summary>
        /// Integer value of used buffer size as KB.
        /// Default value is 32 KB.
        /// </summary>
        [DefaultValue(32)]
        public uint BufferSize { get; set; }
    }
}
