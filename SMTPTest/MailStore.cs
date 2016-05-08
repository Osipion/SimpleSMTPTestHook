using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMTPTest
{
    public class MailStore : IMailReciever, IMailSource
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MailStore));

        private readonly MongoClient dbClient;
        private readonly string dbName;
        private readonly string collectionName;

        public MailStore(MongoClient dbClient, string dbName, string collectionName)
        {
            if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));
            if (string.IsNullOrWhiteSpace(dbName)) throw new ArgumentNullException(nameof(dbName));
            if (string.IsNullOrWhiteSpace(collectionName)) throw new ArgumentNullException(nameof(collectionName));

            this.dbClient = dbClient;
            this.collectionName = collectionName;
            this.dbName = dbName;

            var serverDetails = dbClient.Settings.Server.ToString();

            logger.Info($"MailStore initialized and connected to mongodb server on {serverDetails}.");
        }


        public void Accept(Mail mail)
        {
            if (mail != null)
            {
                if (this.dbClient != null)
                {
                    logger.Debug($"Storing mail item {mail._id}...");
                    dbClient.GetDatabase(this.dbName)
                        .GetCollection<Mail>(this.collectionName)
                        .InsertOne(mail);
                    logger.Debug($"Stored mail item {mail._id}.");
                }
            }
        }

        public async Task<IEnumerable<Mail>> MailBetween(DateTime start, DateTime end, int max)
        {
            logger.Debug($"Recieved request for all mail between {start} and {end}.");
            var col = this.getMails();
            var filter = Builders<Mail>.Filter;
            var f = filter.Gte(m => m.RecievedAt, start) & filter.Lt(m => m.RecievedAt, end);
            return await col.Find(f).Limit(max).ToListAsync();
        }

        public async Task<IEnumerable<Mail>> RecentMailFor(string emailAddress, int max)
        {
            var col = this.getMails();
            logger.Debug($"Recieved request for recent mail for /{emailAddress}/ for up to {max} records");
            var regex = new BsonRegularExpression(emailAddress);
            var filter = Builders<Mail>.Filter.Regex(m => m.Recipient, regex);
            var sort = Builders<Mail>.Sort.Descending(m => m.RecievedAt);

            return await col.Find(filter).Sort(sort).Limit(max).ToListAsync();
        }

        public async Task<long> DeleteMailFor(string emailAddress)
        {
            var col = this.getMails();
            logger.Debug($"Recieved request to delete mail for /{emailAddress}/");
            var regex = new BsonRegularExpression(emailAddress);
            var filter = Builders<Mail>.Filter.Regex(m => m.Recipient, regex);

            return await col.DeleteManyAsync(filter).ContinueWith(t => t.Result.DeletedCount);
        }

        private IMongoCollection<Mail> getMails()
        {
            return dbClient.GetDatabase(this.dbName).GetCollection<Mail>(this.collectionName);
        }

        public async Task<long> DeleteMailBetween(DateTime start, DateTime end)
        {
            var col = this.getMails();
            logger.Debug($"Recieved request to delete mail between {start} and {end}.");
            var filterBuilder = Builders<Mail>.Filter;

            var filter = filterBuilder.Gte(m => m.RecievedAt, start) & filterBuilder.Lt(m => m.RecievedAt, end);

            return await col.DeleteManyAsync(filter).ContinueWith(t => t.Result.DeletedCount);
        }
    }
}
