using System.Collections.Concurrent;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    ///     Circular buffer impl, original from https://codereview.stackexchange.com/a/134147
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CircularBuffer<T>
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
