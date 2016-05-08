using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMTPTest
{
    public interface IMailSource
    {
        Task<IEnumerable<Mail>> MailBetween(DateTime start, DateTime end, int max);

        Task<IEnumerable<Mail>> RecentMailFor(string emailAddress, int max);
    }
}
