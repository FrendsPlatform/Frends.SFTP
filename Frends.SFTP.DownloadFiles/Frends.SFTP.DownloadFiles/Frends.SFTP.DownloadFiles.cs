using System.Text.RegularExpressions;
using System.ComponentModel;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.Net.Sockets;
using Frends.SFTP.DownloadFiles.Definitions;
using Serilog;

#pragma warning disable 1591

namespace Frends.SFTP.DownloadFiles
{
    public class SFTP
    {
        private static BatchContext _batchContext;

        /// <summary>
        /// Downloads file through SFTP connection.
        /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.SFTP.DownloadFiles)
        /// </summary>
        /// <param name="info">Transfer info parameters</param>
        /// <param name="connection">Transfer connection parameters</param>
        /// <param name="source">Source file location</param>
        /// <param name="destination">Destination directory location</param>
        /// <param name="options">Transfer options</param>
        /// <param name="cancellationToken">CancellationToken is given by Frends</param>
        /// <returns>Result object {bool ActionSkiped, bool Success, string UserResultMessage, int SuccessfulTransferCount, int Failedstring FileName, string SourcePath, string DestinationPath, bool Success} </returns>
        public static Result DownloadFiles(
            [PropertyTab] Source source, 
            [PropertyTab] Destination destination, 
            [PropertyTab] Connection connection, 
            [PropertyTab] Options options,
            [PropertyTab] Info info,
            CancellationToken cancellationToken)
        {
            var maxLogEntries = options.OperationLog ? (int?)null : 100;
            var transferSink = new TransferLogSink(maxLogEntries);
            var operationsLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(transferSink)
                .CreateLogger();
            Log.Logger = Log.Logger ?? new LoggerConfiguration()
                .MinimumLevel
                .Debug()
                .CreateLogger();
            var fileTransferLog = Log.Logger;

            using (var logger = InitializeSFTPLogger(operationsLogger))
            {
                if (string.IsNullOrEmpty(info.ProcessUri))
                {
                    fileTransferLog.Warning("ProcessUri is empty. This means the transfer view cannot link to the correct page");
                }

                Guid executionId;
                if (!Guid.TryParse(info.TaskExecutionID, out executionId))
                {
                    fileTransferLog.Warning("'{0}' is not a valid task execution ID, will default to random Guid", info.TaskExecutionID);
                    executionId = Guid.NewGuid();
                }

                _batchContext = new BatchContext
                {
                    Info = info,
                    Options = options,
                    InstanceId = executionId,
                    ServiceId = info.TransferName,
                    SourceFiles = new List<SftpFile>(),
                    DestinationFiles = new List<FileItem>(),
                    RoutineUri = info.ProcessUri,
                    BatchTransferStartTime = DateTime.Now,
                    Source = source,
                    Destination = destination,
                    Connection = connection
                };

                var fileTransporter = new FileTransporter(logger, _batchContext, executionId);
                var result = fileTransporter.Run(cancellationToken);

                if (options.ThrowErrorOnFail && !result.Success)
                    throw new Exception($"SFTP transfer failed: {result.UserResultMessage}. " +
                                        $"Latest operations: \n{GetLogLines(transferSink.GetBufferedLogMessages())}");

                if (options.OperationLog)
                    result.OperationsLog = GetLogDictionary(transferSink.GetBufferedLogMessages());

                return new Result(result);
            }
        }

        /// <summary>
        /// Fetches log lines in case of exception.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static string GetLogLines(IEnumerable<Tuple<DateTimeOffset, string>> buffer)
        {
            try
            {
                return string.Join("\n", buffer.Select(x => x.Item1 == DateTimeOffset.MinValue ? "..." : $"{x.Item1:HH:mm:ssZ}: {x.Item2}"));

            }
            catch (Exception e)
            {
                return $"Error while creating log: \n{e.ToString()}";
            }
        }        

        /// <summary>
        /// Initializes SFTPLogger. 
        /// </summary>
        /// <param name="notificationLogger"></param>
        /// <returns></returns>
        private static SFTPLogger InitializeSFTPLogger(ILogger notificationLogger)
        {
            var logger = new SFTPLogger(notificationLogger);
            return logger;
        }

        /// <summary>
        /// Fetches logs for the Operations log.
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
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
                    { DateTimeOffset.Now.ToString(dateFormat), $"Error while creating operation log: \n{e.ToString()}" }
                };
            }
        }
    }
}
