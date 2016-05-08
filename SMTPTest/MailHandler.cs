using System;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace SMTPTest
{
    public class MailHandler
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MailHandler));

        private TcpClient tcpClient;

        //assumption here is that each mail handler communicates synchronsly with the client, so will never need more
        //than 1 recieve buffer per handler, so may as well just re-use this one.
        private readonly byte[] buffer;

        public Encoding MessageEncoding { get; }

        public IMailReciever OnwardProcessor { get; }

        public MailHandler(TcpClient client, IMailReciever onwardProcessor, Encoding messageEncoding, int bufferCapacity)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (messageEncoding == null) throw new ArgumentNullException(nameof(messageEncoding));
            if (onwardProcessor == null) throw new ArgumentNullException(nameof(onwardProcessor));

            if (bufferCapacity < 256)
                throw new ArgumentOutOfRangeException(nameof(bufferCapacity), $"Must be greater than 256");

            this.tcpClient = client;
            this.OnwardProcessor = onwardProcessor;
            this.MessageEncoding = messageEncoding;
            this.buffer = new byte[bufferCapacity];

            logger.Debug($"Initialized");

            try
            {
                this.handle();
            }
            catch(Exception e)
            {
                logger.Error(e);
            }

            if (this.tcpClient.Connected) this.tcpClient.Close();

            logger.Debug($"Finished");
        }


        private void handle()
        {
            this.write(SmtpStatusCode.ServiceReady, "localhost Test Mail Service Ready ESMTP");

            var greeting = this.read().Trim();

            if(!greeting.StartsWith("EHLO") && !greeting.StartsWith("HELO"))
            {
                this.write(SmtpStatusCode.BadCommandSequence, "Must start with 'EHLO' or 'HELO'");
                logger.Debug($"Recieved incorrect initial message from client (not 'EHLO' or 'HELO').");
                return;
            }

            if (greeting.Length > 4)
                this.write($"250-localhost greets {greeting.Substring(4).Trim()}");

            this.write(SmtpStatusCode.Ok, "OK");

            var command = this.read().Trim();

            if(command.StartsWith("MAIL FROM:"))
            {
                this.write("250 OK");
                this.handleMail(command.Substring(10));
            }
            else
            {
                this.write(SmtpStatusCode.CommandUnrecognized);
                logger.Info($"Recieved unrecognized command. Ending connection.");
            }
        }

        private void handleMail(string sender)
        {
            var mailItem = new Mail();

            mailItem.Sender = sender;

            var recpient = this.read().Trim();

            if(!recpient.StartsWith("RCPT TO:"))
            {
                this.write(SmtpStatusCode.BadCommandSequence, "Must follow request to send mail with recipient");
                logger.Info($"Client did not follow MAIL FROM with RCPT TO.");
                return;
            }

            mailItem.Recipient = recpient.Substring(8);

            this.write(SmtpStatusCode.Ok, "OK");

            var command = this.read().Trim();

            if(string.Compare(command, "DATA", true) != 0)
            {
                this.write(SmtpStatusCode.BadCommandSequence, "Expected 'DATA'");
                logger.Info($"Client did not send expected 'DATA' command");
                return;
            }

            var lastCommand = this.readMailData(mailItem);

            if(!lastCommand.Contains("QUIT"))
            {
                command = this.read().Trim();
                if (!command.EndsWith("QUIT"))
                {
                    this.write(SmtpStatusCode.BadCommandSequence, "expected 'QUIT'");
                }
            }

            if (mailItem.Data != null)
                this.OnwardProcessor.Accept(mailItem);
        }

        private string readMailData(Mail mailItem)
        {
            logger.Debug($"Begins to recieve data from client...");
            this.write(SmtpStatusCode.StartMailInput, "Start mail input; end with <CRLF>.<CRLF>");

            const string endData = "\r\n.\r\n";

            var builder = new StringBuilder();

            string dataPart;

            do
            {
                dataPart = this.read();
                builder.Append(dataPart);
            }
            while (!dataPart.Contains(endData));

            mailItem.Data = builder.ToString();
            mailItem.RecievedAt = DateTime.UtcNow;
            this.write(SmtpStatusCode.Ok, "OK");
            logger.Debug($"Recieved data from client.");
            return dataPart.Substring(dataPart.IndexOf(endData)); //return last part of data read in case quit command followed data immediately
        }


        private void write(SmtpStatusCode status, string message = null)
        {
            this.write($"{(int)status} {message}");
        }

        private void write(string message)
        {
            byte[] buffer = this.MessageEncoding.GetBytes(message + "\r\n");
            var clientStream = this.tcpClient.GetStream();
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
            logger.Debug($"Wrote {message} to client {this.tcpClient.Client.RemoteEndPoint}");
        }

        private String read()
        {
            var clientStream = this.tcpClient.GetStream();
            int bytesRead = clientStream.Read(this.buffer, 0, this.buffer.Length);
            var ret = this.MessageEncoding.GetString(this.buffer, 0, bytesRead);

            if(!string.IsNullOrWhiteSpace(ret)) logger.Debug($"Read message {ret} from client {this.tcpClient.Client.RemoteEndPoint}");
            return ret;
        }

    }
}
