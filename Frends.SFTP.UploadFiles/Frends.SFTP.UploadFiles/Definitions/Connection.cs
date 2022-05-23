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
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePassword)]
        [PasswordPropertyText]
        public string Password { get; set; }

        /// <summary>
        /// Full path to private key file.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKey, AuthenticationType.UsernamePasswordPrivateKey)]
        [DisplayFormat(DataFormatString = "Text")]
        public string PrivateKeyFileName { get; set; }

        /// <summary>
        /// Private key as a string, supported private key formats: PKCS#8,
        /// OpenSSH/OpenSSL and PuTTY.ppk.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKey, AuthenticationType.UsernamePasswordPrivateKey)]
        [PasswordPropertyText]
        public string PrivateKeyString { get; set; }

        /// <summary>
        /// Password for the private key file.
        /// </summary>
        [UIHint(nameof(Authentication), "", AuthenticationType.UsernamePrivateKey, AuthenticationType.UsernamePasswordPrivateKey)]
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
        /// If set, will explicitly use larger buffers (4 MB for TCP receive,
        /// 256 kB for TCP send, 2 MB for SSH buffer) instead of the defaults.
        /// This may increase especially download speeds.
        /// </summary>
        [DefaultValue(false)]
        public bool UseLargeBuffers { get; set; }
    }
}
