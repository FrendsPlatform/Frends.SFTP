using System.ComponentModel;
using Frends.SFTP.UploadFiles.Definitions;
using Frends.SFTP.UploadFiles.Helpers;
using Renci.SshNet.Sftp;
using Serilog;

namespace Frends.SFTP.UploadFiles;

/// <summary>
/// Main class for SFTP upload operations.
/// </summary>
public static class SFTP
{
    private static BatchContext batchContext;

    /// <summary>
    /// Uploads files through SFTP connection.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.SFTP.UploadFiles).
    /// </summary>
    /// <frendsdocs>
    /// # Transfer overview
    /// The file transfer progress has the following steps:
    /// 1. Initialize
    ///
    ///     Initializes the transfer and opens the source connection.
    /// 1. ListFiles
    ///
    ///     Get a list of files from the source endpoint according to the filename/mask. If there are no files to transfer, the source connections are closed, and the transfer finishes. The result of the task will then depend on what the option `NoSourceAction` was set to. If it was set to Error, either the #result.Success property will be set to false, or if the `ThrowError
    ///
    /// 1. Transfer files
    ///     If there are files to transfer, they are then transferred individually. For every file in the list returned from the source endpoint, the following process is repeated:
    ///
    ///     1. GetFile
    ///
    ///         Get a file from the source endpoint to the local work directory. If the parameter `RenameSourceFileBeforeTransfer` is set to `true`, the file is first renamed with a temporary filename before transfer.
    ///
    ///     1. Rename or move the source file.
    ///
    ///         This is done before transferring the file to the destination, this means that possible errors in the renaming or moving that would cause the transfer to fail will happen as early as possible - before we actually try to transfer files onward.
    ///
    ///     1. Transfer the file.
    ///
    ///         If destination file already exists, depending on the parameter `DestinationFileExistsAction` either an exception is thrown, the destination file is overwritten or the source file is appended to the destination file.
    ///         If the parameter `RenameDestinationFileDuringTransfer` is `true`, the file is first transferred with a temporary file name and afterwards renamed to intended filename, otherwise the file is transferred with the intended filename. The intended filename has its possible file masks expanded.
    ///
    ///     1. Do the source file operation.
    ///
    ///         Perform the operation defined by the `SourceOperation`.
    ///
    /// 1. Finish
    ///
    ///     Close the source and destination endpoint connections.
    ///     If the transfer is cancelled (e.g. by calling Terminate on the process instance), the files that are currently being transferred will be processed until finished, but no new files will be transferred. The cancelled transfer end result will be Failed.
    ///
    /// # Macro reference
    ///
    /// Macros can be used to dynamically configure source directory, destination directory or destination file name for a file transfer.
    ///
    /// Generally the following rules apply for macros:
    /// - Macros are case insensitive.
    /// - You can use any number of macros in all of the cases.
    /// - Dates and times are formatted with leading zeros.
    ///
    /// The following macros can be used with all of dynamically configurable locations for file transfer:
    ///
    /// - %Ticks% = will be replace with the current time as Ticks.
    /// - %DateTime% = will be replaced with date and time in format: "yyyy-MM-dd-HH-mm-ss".
    /// - %DateTimeMs% = will be replace with date and time in format: "yyyy-MM-dd-HH-mm-ss-fff".
    /// - %Date% = will be replaced with date in format: "yyyy-MM-dd".
    /// - %Time% = will be replaced with time in format: "HH-mm-ss".
    /// - %Year% = will be replaced with current year.
    /// - %Month% = will be replaced with current month.
    /// - %Day% = will be replaced with current day.
    /// - %Hour% = will be replaced with current hour.
    /// - %Minute% = will be replaced with current minute.
    /// - %Second% = will be replaced with current second.
    /// - %Millisecond% = will be replaced with current millisecond.
    /// - %WeekDay% = will be replaced with a number of weekday, ranging from 1 (monday) to 7 (sunday).
    /// - %Guid% = will be replaced with a new unique identifier.
    /// - %TransferId% = will be replaced with the transfer id.
    /// - %TransferName% = will be replaced with TransferName parameter specified in Connection point schema.
    /// - %TransferGroupName% = will be replaced with TransferGroupName parameter specified in routine's task arguments.
    ///
    /// Destination directory has one additional macro:
    ///
    /// - %SourceRelativeDirectory% = will be replaced with the relative path from source directory to file's directory during recursive uploads when IncludeSubdirectories is enabled.
    ///   Empty for files in root source directory. Example: for source 'C:\upload\' and file 'C:\upload\sub1\sub2\file.txt',
    ///   macro expands to 'sub1/sub2/'. Use in Destination.Directory like '/upload/%SourceRelativeDirectory%'
    ///   to preserve source folder structure on the SFTP server.
    ///
    /// Destination file name has two additional macros that can be used for dynamically creating destination file name.
    ///
    /// - %SourceFileName% = will be replaced with source file name without extension.
    /// - %SourceFileExtension% = will be replaced with source file's extension, with the dot '.' included, i.e. if the source file is named 'foo.txt', the %SourceFileExtension% will be expanded as '.txt'. If the source file name does not have an extension, the macro result will be empty, i.e. for original file name "foo", "bar%SourceFileExtension%" will result in "bar".
    /// </frendsdocs>
    /// <param name="input">Source, destination, and transfer metadata parameters.</param>
    /// <param name="connection">Transfer connection parameters.</param>
    /// <param name="options">Transfer options.</param>
    /// <param name="cancellationToken">CancellationToken is given by Frends.</param>
    /// <returns>object { bool ActionSkipped, bool Success, string UserResultMessage, int SuccessfulTransferCount, int FailedTransferCount, IEnumerable&lt;string&gt; TransferredFileNames, Dictionary&lt;string, IList&lt;string&gt;&gt; TransferErrors, IEnumerable&lt;string&gt; TransferredFilePaths, string[] TransferredDestinationFilePaths, IDictionary&lt;string, string&gt; OperationsLog, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> UploadFiles(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource();
        CancellationTokenSource linkedCts = null;

        if (options.Timeout > 0)
        {
            // Create a new cancellationTokenWithTimeOutSource with a timeout
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.Timeout));

            // Create a linked token source that combines the external and timeout tokens
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Get the linked token
            cancellationToken = linkedCts.Token;
        }

        FileTransferResult transferResult = null;

        try
        {
            var maxLogEntries = options.OperationLog ? (int?)null : 100;
            var transferSink = new TransferLogSink(maxLogEntries);
            var operationsLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(transferSink)
                .CreateLogger();
            Log.Logger ??= new LoggerConfiguration()
                .MinimumLevel
                .Debug()
                .CreateLogger();
            var fileTransferLog = Log.Logger;

            using (var logger = InitializeSFTPLogger(operationsLogger))
            {
                if (string.IsNullOrEmpty(input.Info.ProcessUri))
                    fileTransferLog.Warning("ProcessUri is empty. This means the transfer view cannot link to the correct page");

                if (!Guid.TryParse(input.Info.TaskExecutionID, out Guid executionId))
                {
                    fileTransferLog.Warning("'{0}' is not a valid task execution ID, will default to random Guid", input.Info.TaskExecutionID);
                    executionId = Guid.NewGuid();
                }

                batchContext = new BatchContext
                {
                    Info = input.Info,
                    TempWorkDir = InitializeTemporaryWorkPath(input.Info.WorkDir),
                    Options = options,
                    InstanceId = executionId,
                    ServiceId = input.Info.TransferName,
                    SourceFiles = new List<FileItem>(),
                    DestinationFiles = new List<SftpFile>(),
                    RoutineUri = input.Info.ProcessUri,
                    BatchTransferStartTime = DateTime.Now,
                    Source = input.Source,
                    Destination = input.Destination,
                    Connection = connection,
                };

                var fileTransporter = new FileTransporter(logger, batchContext, executionId);
                transferResult = await fileTransporter.Run(cancellationToken);

                if (!transferResult.Success)
                {
                    throw new Exception($"SFTP transfer failed: {transferResult.UserResultMessage}" +
                                        $"Latest operations: \n{GetLogLines(transferSink.GetBufferedLogMessages())}");
                }

                if (options.OperationLog)
                    transferResult.OperationsLog = GetLogDictionary(transferSink.GetBufferedLogMessages());

                return new Result(transferResult);
            }
        }
        catch (Exception ex)
        {
            var handledResult = ex.Handle(options);
            return transferResult is { Success: false }
                ? new Result(transferResult, handledResult.Error)
                : handledResult;
        }
        finally
        {
            if (options.Timeout > 0)
                linkedCts.Dispose();
        }
    }

    private static string InitializeTemporaryWorkPath(string workDir)
    {
        var tempWorkDir = GetTemporaryWorkPath(workDir);
        Directory.CreateDirectory(tempWorkDir);
        return tempWorkDir;
    }

    private static string GetTemporaryWorkPath(string workDir)
    {
        var tempWorkDirBase = workDir;

        if (string.IsNullOrEmpty(workDir)) tempWorkDirBase = Path.GetTempPath();

        return Path.Combine(tempWorkDirBase, Path.GetRandomFileName());
    }

    /// <summary>
    /// Fetches log lines in case of exception.
    /// </summary>
    /// <param name="buffer">Buffered log entries.</param>
    /// <returns>Formatted log lines.</returns>
    private static string GetLogLines(IEnumerable<Tuple<DateTimeOffset, string>> buffer)
    {
        try
        {
            return string.Join("\n", buffer.Select(x => x.Item1 == DateTimeOffset.MinValue ? "..." : $"{x.Item1:HH:mm:ssZ}: {x.Item2}"));
        }
        catch (Exception e)
        {
            return $"Error while creating log: \n{e}";
        }
    }

    private static SFTPLogger InitializeSFTPLogger(ILogger notificationLogger)
    {
        var logger = new SFTPLogger(notificationLogger);
        return logger;
    }

    private static IDictionary<string, string> GetLogDictionary(IList<Tuple<DateTimeOffset, string>> entries)
    {
        const string dateFormat = "yyyy-MM-dd HH:mm:ss.f0Z";

        try
        {
            return entries
                .Where(e => e?.Item2 != null) // Filter out nulls
                .ToLookup(
                    x => x.Item1.ToString(dateFormat))
                .ToDictionary(
                    x => x.Key,
                    x => string.Join("\n", x.Select(k => k.Item2)));
        }
        catch (Exception e)
        {
            return new Dictionary<string, string>
            {
                { DateTimeOffset.Now.ToString(dateFormat), $"Error while creating operation log: \n{e}" },
            };
        }
    }
}
