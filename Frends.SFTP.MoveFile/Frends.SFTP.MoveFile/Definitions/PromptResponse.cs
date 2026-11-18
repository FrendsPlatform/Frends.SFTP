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
        /// Prompt from the server what is to be expected.
        /// </summary>
        /// <example><c>Verification code</c>.</example>
        public string Prompt { get; set; }

        /// <summary>
        /// Response for the Prompt from the server.
        /// </summary>
        /// <example><c>123456789</c>.</example>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        public string Response { get; set; }
    }
}
