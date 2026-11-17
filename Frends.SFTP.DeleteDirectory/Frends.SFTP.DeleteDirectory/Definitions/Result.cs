namespace Frends.SFTP.DeleteDirectory.Definitions;

using System.Collections.Generic;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">Gets a value indicating whether the Task was executed successfully and without errors.</param>
    /// <param name="data">Gets list of deleted items.</param>
    /// <param name="errorMessage">Error message.</param>
    internal Result(bool success, List<string> data, dynamic errorMessage)
    {
        this.Success = success;
        this.Data = data;
        this.ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets a value indicating whether the Task was executed successfully and without errors.
    /// </summary>
    /// <example>true.</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets list of deleted items.
    /// This list can contain items that were deleted before an exception occured.
    /// </summary>
    /// <example>
    /// {
    ///     "/dir/subdir/"
    ///     "/dir/file.txt"
    ///     "/dir/.."
    ///     "/dir/."
    /// </example>
    public List<string> Data { get; private set; }

    /// <summary>
    /// Gets error message.
    /// This value is generated when an exception occurs and Options.ThrowExceptionOnError is false.
    /// </summary>
    /// <example>Error occured...</example>
    public dynamic ErrorMessage { get; private set; }
}