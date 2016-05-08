using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SMTPTest.API
{
    [ServiceContract]
    public interface ISMTPTestService
    {
        [OperationContract]
        IEnumerable<Mail> GetRecentMailFor(string address, int max);

        [OperationContract]
        IEnumerable<Mail> GetMailBetween(DateTime start, DateTime end);

        [OperationContract]
        long DeleteMailFor(string address);

        [OperationContract]
        long DeleteMailBetween(DateTime start, DateTime end);
    }
}
