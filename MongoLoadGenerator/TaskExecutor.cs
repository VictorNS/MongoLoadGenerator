using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;

namespace MongoLoadGenerator
{
	public static class TaskExecutor
	{
		public static async Task<bool> ExecuteAsync(int taskNumber, ITaskWatcher testsWatcher, ILogger logger, string uri)
		{
			IMongoCollection<BsonDocument> collection;
			try
			{
				logger.Info($"Task {taskNumber} is started.");
				System.Threading.Thread.Sleep(10 * taskNumber); // postpone killing
				var _client = new MongoClient(uri);
				var _database = _client.GetDatabase("MongoLoadGenerator");
				collection = _database.GetCollection<BsonDocument>("MongoLoadCollection");

				while (!testsWatcher.IsNeedStopTask)
				{
					var document = new BsonDocument
					{
						{"taskNumber", taskNumber},
						{"date", DateTime.UtcNow},
						{"guid", Guid.NewGuid().ToString()},
					};
					await collection.InsertOneAsync(document);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return false;
			}

			while (!testsWatcher.IsNeedStopTask)
			{
				try
				{
					var document = new BsonDocument
					{
						{"taskNumber", taskNumber},
						{"date", DateTime.UtcNow},
						{"guid", Guid.NewGuid().ToString()},
					};
					await collection.InsertOneAsync(document);
				}
				catch (Exception ex)
				{
					logger.Error(ex);
					return false;
				}
			}

			return true;
		}
	}
}
