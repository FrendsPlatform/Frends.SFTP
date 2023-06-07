using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Sink that is used to store messages and events from seriolog Logger
/// </summary>
internal class TransferLogSink : ILogEventSink
{
    /// <summary>
    ///     Always store some initial log messages first
    /// </summary>
    private const int DefaultInitialLogMessages = 20;

    private readonly IList<Tuple<DateTimeOffset, string>> _allMsgsBuffer;
    private readonly CircularBuffer<Tuple<DateTimeOffset, string>> _circularBuffer;

    private readonly IList<Tuple<DateTimeOffset, string>> _initialLogMessages;

    public TransferLogSink(int? maxLogEntries)
    {
        if (maxLogEntries != null)
        {
            _initialLogMessages = new List<Tuple<DateTimeOffset, string>>(DefaultInitialLogMessages);
            _circularBuffer = new CircularBuffer<Tuple<DateTimeOffset, string>>(maxLogEntries.Value);
        }
        else
            _allMsgsBuffer = new List<Tuple<DateTimeOffset, string>>();
    }

    /// <summary>
    /// Emits new messages to the sink
    /// </summary>
    /// <param name="logEvent"></param>
    public void Emit(LogEvent logEvent)
    {
        if (_allMsgsBuffer != null)
            _allMsgsBuffer.Add(new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
        else
        {
            if (_initialLogMessages.Count < DefaultInitialLogMessages)
                _initialLogMessages.Add(
                    new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
            else
                _circularBuffer.Add(
                    new Tuple<DateTimeOffset, string>(logEvent.Timestamp, logEvent.RenderMessage()));
        }
    }

    /// <summary>
    /// Gets the log messages from sink
    /// </summary>
    /// <returns></returns>
    public IList<Tuple<DateTimeOffset, string>> GetBufferedLogMessages()
    {
        if (_allMsgsBuffer != null) return _allMsgsBuffer;

        var bufferedMessages = _circularBuffer.Latest();
        if (bufferedMessages.Any())
            return _initialLogMessages
                .Concat(new[] { Tuple.Create(DateTimeOffset.MinValue, "...") })
                .Concat(bufferedMessages).ToList();

        return _initialLogMessages;
    }

    /// <summary>
    ///     Circular buffer impl, original from https://codereview.stackexchange.com/a/134147
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularBuffer<T>
    {
        private readonly ConcurrentQueue<T> _data;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly int _size;

        public CircularBuffer(int size)
        {
            if (size < 1) throw new ArgumentException($"{nameof(size)} cannot be negative or zero");
            _data = new ConcurrentQueue<T>();
            _size = size;
        }

        public IReadOnlyList<T> Latest()
        {
            return _data.ToArray();
        }

        public void Add(T t)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_data.Count == _size)
                {
                    T value;
                    _data.TryDequeue(out value);
                }

                _data.Enqueue(t);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}

