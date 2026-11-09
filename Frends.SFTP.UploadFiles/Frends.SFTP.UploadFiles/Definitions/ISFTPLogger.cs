namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Internal logger interface for SFTP transfers.
/// </summary>
internal interface ISFTPLogger : IDisposable
{
    /// <summary>
    /// Records an error and its associated exception.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="msg">Error message to log.</param>
    /// <param name="e">Exception associated with the error.</param>
    void NotifyError(BatchContext context, string msg, Exception e);

    /// <summary>
    /// Records an informational message.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="msg">Message to log.</param>
    void NotifyInformation(BatchContext context, string msg);

    /// <summary>
    /// Records a diagnostic trace message.
    /// </summary>
    /// <param name="message">Trace message to log.</param>
    void NotifyTrace(string message);

    /// <summary>
    /// Records a successful file transfer.
    /// </summary>
    /// <param name="transfer">Details of the completed file transfer.</param>
    /// <param name="context">Context for the current batch.</param>
    void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context);

    /// <summary>
    /// Records a failed file transfer.
    /// </summary>
    /// <param name="transfer">Details of the failed file transfer.</param>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="errorMessage">Description of the transfer failure.</param>
    /// <param name="exception">Exception associated with the failure.</param>
    void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception);

    /// <summary>
    /// Records the outcome of the complete batch.
    /// </summary>
    /// <param name="context">Context for the current batch.</param>
    /// <param name="userResultMessage">Summary shown to the task user.</param>
    /// <param name="success">Whether all requested transfers succeeded.</param>
    /// <param name="actionSkipped">Whether the configured action was skipped.</param>
    void LogBatchFinished(BatchContext context, string userResultMessage, bool success, bool actionSkipped);
}
