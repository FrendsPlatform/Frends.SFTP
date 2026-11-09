namespace Frends.SFTP.DownloadFiles.Definitions;

using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

/// <summary>
/// Sink that is used to store messages and events from seriolog Logger
/// </summary>
internal class TransferLogSink : ILogEventSink
{
    /// <summary>
    ///     Always store some initial log messages first
    /// </summary>
    private const int DefaultInitialLogMessages = 20;

    private readonly IList<Tuple<DateTimeOffset, string>> allMsgsBuffer;
    private readonly CircularBuffer<Tuple<DateTimeOffset, string>> circularBuffer;

    private readonly IList<Tuple<DateTimeOffset, string>> initialLogMessages;

    internal TransferLogSink(int? maxLogEntries)
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
    /// Stores the log event in the buffer.
    /// </summary>
    /// <param name="logEvent">Log event to store.</param>
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

    internal IList<Tuple<DateTimeOffset, string>> GetBufferedLogMessages()
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

    /// <summary>
    ///     Circular buffer impl, original from https://codereview.stackexchange.com/a/134147
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    internal class CircularBuffer<T>
    {
        private readonly ConcurrentQueue<T> data;
        private readonly ReaderWriterLockSlim bufferLock = new();
        private readonly int size;

        internal CircularBuffer(int size)
        {
            if (size < 1) throw new ArgumentException($"{nameof(size)} cannot be negative or zero");
            data = new ConcurrentQueue<T>();
            this.size = size;
        }

        internal IReadOnlyList<T> Latest()
        {
            return data.ToArray();
        }

        internal void Add(T t)
        {
            bufferLock.EnterWriteLock();
            try
            {
                if (data.Count == size)
                {
                    data.TryDequeue(out _);
                }

                data.Enqueue(t);
            }
            finally
            {
                bufferLock.ExitWriteLock();
            }
        }
    }
}
