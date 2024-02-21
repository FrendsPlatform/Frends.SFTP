namespace Frends.SFTP.DownloadFiles.Definitions;

using System.Collections.Concurrent;

/// <summary>
///     Circular buffer impl, original from https://codereview.stackexchange.com/a/134147
/// </summary>
/// <typeparam name="T">T</typeparam>
internal class CircularBuffer<T>
{
    private readonly ConcurrentQueue<T> _data;
    private readonly ReaderWriterLockSlim _lock = new();
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
                _data.TryDequeue(out _);
            }

            _data.Enqueue(t);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
