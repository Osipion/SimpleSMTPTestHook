namespace SMTPTest.API
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    /// <summary>
    /// Access the test mail inbox
    /// </summary>
    [ServiceContract]
    public interface ISMTPTestService
    {
        /// <summary>
        /// Gets the most recent mail for the specified address (part). Address is treated as a regular expression
        /// search against the <see cref="Mail.Recipient"/> field
        /// </summary>
        /// <param name="address"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [OperationContract]
        IEnumerable<Mail> GetRecentMailFor(string address, int max);

        /// <summary>
        /// Gets mail for all users between the specified times, up to the maximum allowed by the server
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [OperationContract]
        IEnumerable<Mail> GetMailBetween(DateTime start, DateTime end);

        /// <summary>
        /// Deletes all mail which matches the specified address (part). Address is treated as a regular expression
        /// search against <see cref="Mail.Recipient"/>
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [OperationContract]
        long DeleteMailFor(string address);

        /// <summary>
        /// Deletes all mail for all users between the specified dates
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [OperationContract]
        long DeleteMailBetween(DateTime start, DateTime end);
    }
}
