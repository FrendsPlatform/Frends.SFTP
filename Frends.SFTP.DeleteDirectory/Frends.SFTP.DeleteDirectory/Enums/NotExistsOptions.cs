namespace Frends.SFTP.DeleteDirectory.Enums;

/// <summary>
/// Options for ThrowNotExistError.
/// </summary>
public enum NotExistsOptions
{
    /// <summary>
    /// Task execution will end and the error message will be added into Result.ErrorMessage and Result.Success will be set to true.
    /// </summary>
    Skip,

    /// <summary>
    /// An exception will be thrown.
    /// </summary>
    Throw,
}