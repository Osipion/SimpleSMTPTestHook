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

namespace SMTPTest.Tests
{
    [TestFixture]
    public class TestParse
    {

        private class MockStore : IMailReciever
        {
            public List<Mail> Mail { get; } = new List<SMTPTest.Mail>();
            public void Accept(Mail mail) => this.Mail.Add(mail);
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
            }
        }

        [Test]
        public void TestMethod()
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
        public void Experiment()
        {
            log4net.Config.XmlConfigurator.Configure();

            var start = DateTime.UtcNow;
            var store = new MockStore();
            var server = new MailServer(new IPEndPoint(IPAddress.Any, 25), store, Encoding.ASCII);

            try
            {
                for (var i = 0; i < 10; i++)
                {
                    using (var client = new SMAIL.SmtpClient("localhost"))
                    {
                        client.Send("test1@mail.fakedom", "test2@mail.fakedom", "subject", "body");
                    }
                }

                Thread.Sleep(2000);

                Assert.That(store.Mail.Count, Is.EqualTo(10));
            }
            finally
            {
                server.Stop();
            }


        }
    }
}
