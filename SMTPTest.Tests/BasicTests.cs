using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SMAIL = System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static System.Console;
using System.Collections.Concurrent;

namespace SMTPTest.Tests
{
    [TestFixture]
    public class BasicTests
    {

        private class MockStore : IMailReciever
        {
            public ConcurrentQueue<Mail> Mail { get; } = new ConcurrentQueue<SMTPTest.Mail>();
            public void Accept(Mail mail) => this.Mail.Enqueue(mail);
        }


        private volatile bool isWaiting = false;

        MailHandler listen(IMailReciever pipe)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 25);
            var listener = new TcpListener(endPoint);
            listener.Start();

            try
            {
                this.isWaiting = true; //potential race condition here
                TcpClient client = listener.AcceptTcpClient();
                return new MailHandler(client, pipe, Encoding.ASCII, 10000);
            }
            finally
            {
                this.isWaiting = false;
                listener.Stop();
            }
        }

        [Test]
        public void MailHandlerHandlesBasicRequest()
        {
            var pipe = new MockStore();

            var listenerTask = Task.Run(() => listen(pipe));

            while (!this.isWaiting) Thread.Sleep(100);

            using (var client = new SMAIL.SmtpClient("localhost"))
            {
                client.Send("bob1@mail.fakedom", "bob2@mail.fakedom", "subject", "body");
            }

            var handler = listenerTask.Result;

            Assert.That(pipe.Mail.Count, Is.EqualTo(1));
            var mail = pipe.Mail.First();
            Assert.That(mail.Sender, Is.EqualTo("<bob1@mail.fakedom>"));
            Assert.That(mail.Recipient, Is.EqualTo("<bob2@mail.fakedom>"));
            WriteLine(mail.ToDisplayString());
        }

        [Test]
        public void MailServerHandlesMultipleRequests()
        {
            log4net.Config.XmlConfigurator.Configure();

            var start = DateTime.UtcNow;
            var store = new MockStore();
            var server = new MailServer(new IPEndPoint(IPAddress.Any, 25), store, Encoding.ASCII);

            const int senderTasks = 10;

            try
            {
                var tasks = new Task<int>[senderTasks];
                var random = new Random();

                for (var i = 0; i < senderTasks; i++)
                {
                    tasks[i] = sendDelayedMailAsync(i, random);
                }

                Task.WaitAll(tasks);

                Assert.That(store.Mail.Count, Is.EqualTo(senderTasks));
            }
            finally
            {
                server.Stop();
            }
        }

        private async Task<int> sendDelayedMailAsync(int id, Random r)
        {
            await Task.Delay(r.Next(30));

            using (var client = new SMAIL.SmtpClient("localhost"))
            {
                client.Send("test1@mail.fakedom", "test2@mail.fakedom", "subject", "body");
            }

            Console.WriteLine($"Sent mail {id}");

            return id;
        }
    }
}
