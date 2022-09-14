namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Enumeration for host key algorithms.
/// </summary>
public enum HostKeyAlgorithms
{
    /// <summary>
    /// The algorithm is negotiated with the server.
    /// </summary>
    Any,
    /// <summary>
    /// Force the ssh-rsa host key algorithm.
    /// </summary>
    RSA,
    /// <summary>
    /// Force the ssh-ed25519 host key algorithm.
    /// </summary>
    Ed25519,
    /// <summary>
    /// Force the ssh-dss host key algorithm.
    /// </summary>
    DSS,
    /// <summary>
    /// Force the ecdsa-sha2-nistp256 host key algorithm.
    /// </summary>
    nistp256,
    /// <summary>
    /// Force the ecdsa-sha2-nistp384 host key algorithm.
    /// </summary>
    nistp384,
    /// <summary>
    /// Force the ecdsa-sha2-nistp521 host key algorithm.
    /// </summary>
    nistp521
}

