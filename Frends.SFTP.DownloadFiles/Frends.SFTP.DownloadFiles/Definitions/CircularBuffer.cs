namespace Frends.SFTP.DownloadFiles.Definitions;

using System.Collections.Concurrent;

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
