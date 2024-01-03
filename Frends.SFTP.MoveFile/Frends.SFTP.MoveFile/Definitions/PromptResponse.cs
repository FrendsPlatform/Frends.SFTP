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
        /// <example>Verification code</example>
        public string Prompt { get; set; }

        /// <summary>
        /// Response for the Prompt from the server.
        /// </summary>
        /// <example>123456789</example>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        public string Response { get; set; }
    }
}
