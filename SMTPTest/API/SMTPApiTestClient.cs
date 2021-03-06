﻿namespace SMTPTest.API
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    public class SMTPApiClient : ISMTPTestService, IDisposable
    {

        public Uri Url { get; }
        private readonly ISMTPTestService service;

        public static HttpBindingBase DefaultBinding { get; } = new NetHttpBinding(BasicHttpSecurityMode.None)
        {
            MaxReceivedMessageSize = int.MaxValue,
            ReceiveTimeout = TimeSpan.FromSeconds(10) 
        };


        public SMTPApiClient(HttpBindingBase binding, Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (binding == null) throw new ArgumentNullException(nameof(binding));

            this.Url = url;

            var address = new EndpointAddress(this.Url);
            var cf = new ChannelFactory<ISMTPTestService>(binding, address);
            this.service = cf.CreateChannel();
            
        }

        #region IDisposable Support
        private bool isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                (this.service as IClientChannel)?.Dispose();
                this.isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion

        public IEnumerable<Mail> GetRecentMailFor(string address, int max)
        {
            return service.GetRecentMailFor(address, max);
        }

        public IEnumerable<Mail> GetMailBetween(DateTime start, DateTime end)
        {
            return service.GetMailBetween(start, end);
        }

        public long DeleteMailFor(string address)
        {
            return service.DeleteMailFor(address);
        }

        public long DeleteMailBetween(DateTime start, DateTime end)
        {
            return service.DeleteMailBetween(start, end);
        }
    }
}
