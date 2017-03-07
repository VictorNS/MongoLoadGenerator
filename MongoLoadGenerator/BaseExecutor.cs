using System;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;

namespace MongoLoadGenerator
{
	public abstract class BaseExecutor
	{
		public virtual IMongoCollection<BsonDocument> Initialize(int taskNumber, ILogger logger, string uri)
		{
			logger.Info($"Task {taskNumber} is started.");
			System.Threading.Thread.Sleep(100 * taskNumber); // postpone killing
			IMongoCollection<BsonDocument> collection = null;
			try
			{
				var mongoSettings = MongoClientSettings.FromUrl(MongoUrl.Create(uri));
				//mongoSettings.WriteConcern = WriteConcern.Unacknowledged;
				//mongoSettings.MinConnectionPoolSize = 1;
				//mongoSettings.MaxConnectionPoolSize = 1;
				mongoSettings.SocketTimeout = TimeSpan.FromSeconds(10);
				mongoSettings.ReadPreference = ReadPreference.SecondaryPreferred;
				var _client = new MongoClient(mongoSettings);
				var _database = _client.GetDatabase("MongoLoadGenerator");
				collection = _database.GetCollection<BsonDocument>("MongoLoadCollection");
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"Task {taskNumber} was broked on initialization.");
			}
			return collection;
		}
	}
}