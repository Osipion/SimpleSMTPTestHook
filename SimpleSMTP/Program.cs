using MongoDB.Driver;
using SMTPTest;
using SMTPTest.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSMTP
{
    class Program
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("MAIN");

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            logger.Info($"Starting simple SMTP service and api. Remember this application may need to be run as administrator.");


            var start           = DateTime.UtcNow;

            //configure store
            var dbClient        = new MongoClient();
            var store           = new MailStore(dbClient, 
                                        Properties.Settings.Default.DBName, 
                                        Properties.Settings.Default.MailCollection);

            //smtp settings
            var smtpEncoding    = Encoding.GetEncoding(Properties.Settings.Default.SMTPEncoding);
            var smtpPort        = Properties.Settings.Default.SMTPPort;
            var smtpTimeout     = Properties.Settings.Default.SMTPRecieveTimeout;

            //api settings
            var apiPort         = Properties.Settings.Default.APIPort;
            var apiMax          = Properties.Settings.Default.ApiMaxMailPerRequest;
            var apiService      = new SMTPTestService(store, apiMax);
            var apiUrl          = new Uri($"http://{getIp()}:{apiPort}/api");

            //start smtp service
            var smtpServer      = new MailServer(new IPEndPoint(IPAddress.Any, smtpPort), 
                                        store, smtpEncoding, smtpTimeout);

            logger.Info($@"Simple SMTP Service listening on port {smtpPort}.");
                
            //start api service
            using(var host = configureApiService(apiUrl, apiService))
            {
                host.Open();
                logger.Info($@"API Service listening at {apiUrl}.");
                logger.Info($@"Options: 
    show: list all mail recieved since the service started
    quit: exit");

                while (true)
                {
                    var line = Console.ReadLine().ToUpperInvariant();
                    if (line == "SHOW")
                    {
                        var mail = store.MailBetween(start, DateTime.UtcNow, apiMax).Result;
                        foreach (var m in mail)
                        {
                            Console.WriteLine(m.ToDisplayString());
                        }
                    }
                    else if (line == "QUIT")
                    {
                        break;
                    }
                }
                logger.Info("Stopping smtp service...");
                smtpServer.Stop();
                logger.Info("Closing api host...");
                host.Close();

            }  
        }

        static ServiceHost configureApiService(Uri url, SMTPTestService serviceInstance)
        {
            var host = new ServiceHost(serviceInstance, url);

            var endpt = host.AddServiceEndpoint(typeof(ISMTPTestService), new NetHttpBinding(BasicHttpSecurityMode.None), "/mail");

            var smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            host.Description.Behaviors.Add(smb);

            var sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();

            if(sdb == null)
            {
                sdb = new ServiceDebugBehavior();
                host.Description.Behaviors.Add(sdb);
            }

            sdb.IncludeExceptionDetailInFaults = true;

            return host;
        }

        static string getIp()
        {
            IPHostEntry host;
            string localIP = "localhost";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
    }
}
