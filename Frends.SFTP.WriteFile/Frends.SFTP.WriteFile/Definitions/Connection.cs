#pragma warning disable 1591

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.WriteFile
{
    /// <summary>
    /// Parameters class usually contains parameters that are required.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// SFTP host address
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Address { get; set; }

        /// <summary>
        /// Port number
        /// </summary>
        [DefaultValue(22)]
        public int Port { get; set; } = 22;

        /// <summary>
        /// Selection for authentication type
        /// </summary>
        public AuthenticationType Authentication { get; set; } = AuthenticationType.UsernamePassword;

        /// <summary>
        /// Username
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [UIHint(nameof(AuthenticationType), "", AuthenticationType.UsernamePassword)]
        [PasswordPropertyText]
        public string Password { get; set; }

        /// <summary>
        /// Full path to private key file.
        /// </summary>
        [UIHint(nameof(AuthenticationType), "", AuthenticationType.PrivateKey, AuthenticationType.PrivateKeyPassphrase)]
        [DisplayFormat(DataFormatString = "Text")]
        public string PrivateKeyFileName { get; set; }

        /// <summary>
        /// Passphrase for the private key file.
        /// </summary>
        [UIHint(nameof(AuthenticationType), "", AuthenticationType.PrivateKeyPassphrase)]
        [PasswordPropertyText]
        public string Passphrase { get; set; }

        public enum AuthenticationType
        {
            UsernamePassword,
            PrivateKey,
            PrivateKeyPassphrase
        }
    }
}
