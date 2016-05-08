namespace SMTPTest
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class MailServer : IDisposable
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MailServer));
        private const int bufferCapacity = 10240;

        private readonly Task listenerTask;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly Encoding encoding;
        private readonly TcpListener listener;

        public IMailReciever MailPipe { get; }

        public MailServer(IPEndPoint endPoint, IMailReciever store, Encoding encoding, int recieveTimeout = 1000)
        {
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            this.MailPipe = store;
            this.encoding = encoding;
            this.listener = new TcpListener(endPoint);
            this.listener.Server.ReceiveTimeout = recieveTimeout;
            this.listener.Start();
            logger.Info("Started tcp listener...");
            this.listenerTask = Task.Run(() => acceptMailClients(listener, cancellation.Token), cancellation.Token);
        }

        public void Stop()
        {
            logger.Info("Recieves request to stop listening, so sets the cancellation token.");
            this.cancellation.Cancel();
            logger.Info("Starts waiting for listener task to stop...");
            this.listenerTask?.Wait();
            this.listener.Stop();
            logger.Info("Stopped.");
        }

        void acceptMailClients(TcpListener server, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (server.Pending())
                { 
                    server.AcceptTcpClientAsync()
                        .ContinueWith(t =>
                        {
                            var r = t.Result;
                            logger.Info($"Accepted tcp client @ {r.Client.RemoteEndPoint.ToString()}");
                            new MailHandler(r, this.MailPipe, this.encoding, bufferCapacity);
                        }, token);
                }
            }

            logger.Info($"Stopped accepting connections.");
        }

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                this.Stop();
                isDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
