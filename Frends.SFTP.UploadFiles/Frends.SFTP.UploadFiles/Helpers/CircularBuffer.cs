using System.Collections.Concurrent;

namespace Frends.SFTP.UploadFiles.Helpers;

/// <summary>
///     Circular buffer impl, original from https://codereview.stackexchange.com/a/134147.
/// </summary>
/// <typeparam name="T">Type of the values stored in the buffer.</typeparam>
internal class CircularBuffer<T>
{
    private readonly ConcurrentQueue<T> data;
    private readonly ReaderWriterLockSlim @lock = new();
    private readonly int size;

    public CircularBuffer(int size)
    {
        if (size < 1) throw new ArgumentException($"{nameof(size)} cannot be negative or zero");
        data = new ConcurrentQueue<T>();
        this.size = size;
    }

    /// <summary>
    /// Returns the currently buffered items in insertion order.
    /// </summary>
    /// <returns>The buffered items in insertion order.</returns>
    public IReadOnlyList<T> Latest()
    {
        return data.ToArray();
    }

    /// <summary>
    /// Adds an item and removes the oldest item when the buffer is full.
    /// </summary>
    /// <param name="t">Item to add to the buffer.</param>
    public void Add(T t)
    {
        @lock.EnterWriteLock();
        try
        {
            if (data.Count == size)
            {
                data.TryDequeue(out T value);
            }

            data.Enqueue(t);
        }
        finally
        {
            @lock.ExitWriteLock();
        }
    }
}
