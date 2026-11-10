using Serilog.Core;
using Serilog.Events;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Sink that is used to store messages and events from seriolog Logger.
/// </summary>
internal class TransferLogSink : ILogEventSink
{
    /// <summary>
    ///     Always store some initial log messages first.
    /// </summary>
    private const int DefaultInitialLogMessages = 20;

    private readonly IList<Tuple<DateTimeOffset, string>> allMsgsBuffer;
    private readonly CircularBuffer<Tuple<DateTimeOffset, string>> circularBuffer;

    private readonly IList<Tuple<DateTimeOffset, string>> initialLogMessages;

    public TransferLogSink(int? maxLogEntries)
    {
        if (maxLogEntries != null)
        {
            initialLogMessages = new List<Tuple<DateTimeOffset, string>>(DefaultInitialLogMessages);
            circularBuffer = new CircularBuffer<Tuple<DateTimeOffset, string>>(maxLogEntries.Value);
        }
        else
        {
            allMsgsBuffer = new List<Tuple<DateTimeOffset, string>>();
        }
    }

    /// <summary>
    /// Stores a rendered log event in the configured message buffer.
    /// </summary>
    /// <param name="logEvent">Log event to buffer.</param>
    public void Emit(LogEvent logEvent)
    {
        if (allMsgsBuffer != null)
        {
            allMsgsBuffer.Add(new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
        }
        else
        {
            if (initialLogMessages.Count < DefaultInitialLogMessages)
            {
                initialLogMessages.Add(
                    new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
            }
            else
            {
                circularBuffer.Add(
                    new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
            }
        }
    }

    /// <summary>
    /// Returns the buffered log messages in the order they were recorded.
    /// </summary>
    /// <returns>Timestamp and message pairs for the buffered entries.</returns>
    public IList<Tuple<DateTimeOffset, string>> GetBufferedLogMessages()
    {
        if (allMsgsBuffer != null) return allMsgsBuffer;

        var bufferedMessages = circularBuffer.Latest();
        if (bufferedMessages.Any())
        {
            return initialLogMessages
                .Concat(new[] { Tuple.Create(DateTimeOffset.MinValue, "...") })
                .Concat(bufferedMessages).ToList();
        }

        return initialLogMessages;
    }
}
