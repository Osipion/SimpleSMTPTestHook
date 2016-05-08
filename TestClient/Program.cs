using SMTPTest;
using SMTPTest.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            const string HELP = @"
        ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        |Basic Test client for SMTP server.| 
        ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
Press Q to exit, M to send a test email, H to show this help, 
or one of the following api options:
    1 <recipient address part> - list the 10 most recent mail items 
                                 matching the argument
                                 <recipient address part>. Can be a 
                                 regex, no quotes required (argument 
                                 is everything after first space).
    2                          - all mail recieved in last 5 minutes
    3                          - all mail recieved in last 30 seconds
";

            var apiUrl = Properties.Settings.Default.SMTPApiUrl;
            var smtpHost = Properties.Settings.Default.SMTPHostName;

            Console.WriteLine($"Read configuration: SMTP Host: {smtpHost}, api url {apiUrl}");

            Console.WriteLine(HELP);



            using (var apiClient = new SMTPApiClient(SMTPApiClient.DefaultBinding, apiUrl))
            {
                while (true)
                {
                    var line = Console.ReadLine();

                    var firstChar = line.ToUpperInvariant().First();

                    try
                    {

                        switch(firstChar)
                        {
                            case 'H':
                                Console.WriteLine(HELP);
                                break;
                            case 'M':
                                using (var client = new SmtpClient(smtpHost))
                                    client.Send("from@address.fakedom", "to@address.fakedom", "Subject", "Body");
                                break;
                            case 'Q':
                                Environment.Exit(0);
                                break;
                            case '1':
                                var searchTerm = line.Substring(line.IndexOf(" ")).TrimStart();
                                printMail(apiClient.GetRecentMailFor(searchTerm, 10));
                                break;
                            case '2':
                                printMail(apiClient.GetMailBetween(DateTime.UtcNow - TimeSpan.FromMinutes(5), DateTime.UtcNow));
                                break;
                            case '3':
                                printMail(apiClient.GetMailBetween(DateTime.UtcNow - TimeSpan.FromSeconds(30), DateTime.UtcNow));
                                break;
                            default:
                                Console.WriteLine("Unknown option.");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        var oldColor = Console.BackgroundColor;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.BackgroundColor = oldColor;
                    }
                }
            }

        }

        static void printMail(IEnumerable<Mail> mail)
        {
            foreach (var m in mail)
            {
                Console.WriteLine(m.ToDisplayString());
            }
        }
    }
}
