namespace Frends.SFTP.DownloadFiles.Definitions
{
    internal class TimeoutAction
    {
        private bool _success;

        private bool _timout;

        public TimeoutAction(int waitLimit, Action action)
        {
            ThreadSynchronizer = new AutoResetEvent(false);
            ActionThread = new Thread(new ThreadStart(delegate
            {
                action.Invoke();
                if (_timout) return;
                _timout = true;
                _success = true;
                ThreadSynchronizer.Set();
            }));

            TimeoutThread = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(waitLimit * 1000);
                if (_success) return;
                _timout = true;
                _success = false;
                ThreadSynchronizer.Set();
            }));
        }

        private Thread ActionThread { get; set; }

        private Thread TimeoutThread { get; set; }

        private AutoResetEvent ThreadSynchronizer { get; set; }

        /// <summary>
        /// If the action takes longer than the wait limit, this will throw a TimeoutException
        /// </summary>
        public void Start()
        {
            ActionThread.Start();
            TimeoutThread.Start();

            ThreadSynchronizer.WaitOne();

            if (!_success)
            {
                throw new TimeoutException();
            }

            ThreadSynchronizer.Close();
        }
    }
}