using System;
using System.Collections.Generic;
using Renci.SshNet;

#pragma warning disable 1591

namespace Frends.Community.Sftp
{
    public interface ISftpService : IDisposable
    {
        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="connectionInfo">Connection information.</param>
        /// <returns>Instance of SftpService implementation.</returns>
        ISftpService Connect(ConnectionInfo connectionInfo);

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Retrieves list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>List of files.</returns>
        IEnumerable<IFileResult> ListDirectory(string path, Action<int> listCallback = null);
    }
}
