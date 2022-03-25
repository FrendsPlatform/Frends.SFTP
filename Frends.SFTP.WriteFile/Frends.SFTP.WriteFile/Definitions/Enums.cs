using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Frends.SFTP.WriteFile.Definitions
{
    /// <summary>
    /// Enumeration to specify authentication type.
    /// </summary>
    public enum AuthenticationType
    {
        UsernamePassword,
        PrivateKey,
        PrivateKeyPassphrase
    }

    /// <summary>
    /// Enumeration to specify operation if destination file exists.
    /// </summary>
    public enum DestinationOperation
    {
        Rename,
        Overwrite,
        Error
    }
}
