using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Prompt response class for Keyboard-interactive authentication.
    /// </summary>
    public class PromptResponse
    {
        /// <summary>
        /// Prompt from the server indicating what is expected.
        /// </summary>
        /// <example>Verification code</example>
        public string Prompt { get; set; }

        /// <summary>
        /// Response to the prompt from the server.
        /// </summary>
        /// <example>123456789</example>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        public string Response { get; set; }
    }
}
