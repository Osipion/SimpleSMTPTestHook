using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SMTPTest.API
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, 
        InstanceContextMode = InstanceContextMode.Single, 
        IncludeExceptionDetailInFaults = true)]
    public class SMTPTestService : ISMTPTestService
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SMTPTestService));

        private readonly MailStore mails;
        private readonly int max;

        public SMTPTestService(MailStore mails, int max)
        {
            this.mails = mails;
            this.max = max;
        }

        public long DeleteMailBetween(DateTime start, DateTime end)
        {
            return this.mails.DeleteMailBetween(start, end).Result;
        }

        public long DeleteMailFor(string address)
        {
            return this.mails.DeleteMailFor(address).Result;
        }

        public IEnumerable<Mail> GetMailBetween(DateTime start, DateTime end)
        {
            return this.mails.MailBetween(start, end, this.max).Result;
        }

        public IEnumerable<Mail> GetRecentMailFor(string address, int max)
        {
            if (max > this.max) max = this.max;
            return this.mails.RecentMailFor(address, max).Result;
        }
    }
}
