namespace SMTPTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Runtime.Serialization;

    [Serializable, DataContract]
    public class Mail
    {
        [DataMember]
        public object _id { get; set; }

        [DataMember]
        public DateTime RecievedAt { get; set; }

        [DataMember]
        public string Sender { get; set; }

        [DataMember]
        public string Recipient { get; set; }

        [DataMember]
        public string Data { get; set; }

        public Mail()
        {
            this._id = Guid.NewGuid();
        }

        public string ToDisplayString()
        {
            return $@"FROM: {this.Sender}
TO: {this.Recipient}
DATA:
    {this.Data.Trim()}";
        }


        public IDictionary<string, string> ParseHeaders()
        {
            var hdrLines = this.Data.Split('\n').Select(l => l.Trim());

            var hdrs = new Dictionary<string, string>();

            string key = null, value = null;

            foreach(var hdrLine in hdrLines)
            {
                var indx = hdrLine.IndexOf(':');
                if(indx > -1)
                {
                    key = hdrLine.Substring(0, indx + 1);
                    value = hdrLine.Substring(indx);
                }
                else
                {
                    key = hdrLine;
                }
                hdrs.Add(key.ToUpperInvariant(), value);
            }

            return hdrs;
        }

        public MailAddress ParseSender() => new MailAddress(this.Sender.Trim('<', '>'));
        public MailAddress ParseReciever() => new MailAddress(this.Recipient.Trim('<', '>'));
    }
}
