namespace Frends.SFTP.DeleteDirectory.Definitions;

using System.ComponentModel;
using Frends.SFTP.DeleteDirectory.Enums;

/// <summary>
/// Input parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets a value indicating whether an error should stop the Task and throw an exception.
    /// If set to true, an exception will be thrown when an error occurs.
    /// If set to false, Task will try to continue and the error message will be added into Result.ErrorMessage and Result.Success will be set to false.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowExceptionOnError { get; set; }

    /// <summary>
    /// Skip: Task execution will end and the error message will be added into Result.ErrorMessage and Result.Success will be set to true.
    /// Throw: An exception will be thrown.
    /// </summary>
    /// <example>NotExistsOptions.Skip</example>
    [DefaultValue(NotExistsOptions.Skip)]
    public NotExistsOptions ThrowNotExistError { get; set; }
}