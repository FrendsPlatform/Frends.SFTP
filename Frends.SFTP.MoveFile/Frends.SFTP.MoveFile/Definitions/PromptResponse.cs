using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.MoveFile.Definitions
{
    /// <summary>
    /// Prompt response class for Keyboard-interactive authentication.
    /// </summary>
    public class PromptResponse
    {
        /// <summary>
        /// Gets or sets the server prompt to respond to.
        /// </summary>
        /// <example><c>Verification code</c>.</example>
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the response to the server prompt.
        /// </summary>
        /// <example><c>123456789</c>.</example>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        public string Response { get; set; }
    }
}
