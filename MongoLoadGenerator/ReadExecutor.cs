using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;

namespace MongoLoadGenerator
{
	public class ReadExecutor : BaseExecutor, ITaskExecutor
	{
		public async Task<bool> ExecuteAsync(int taskNumber, ITaskWatcher testsWatcher, ILogger logger, string uri)
		{
			var collection = Initialize(taskNumber, logger, uri);
			if (collection == null)
				return false;

			string guid;
			try
			{
				var filter = Builders<BsonDocument>.Filter.Eq("taskNumber", 0);
				//var sort = Builders<BsonDocument>.Sort.Ascending("date");
				var list = await collection.Find(filter).Skip(100).Limit(1).ToListAsync();
				if (list.Count == 0)
				{
					logger.Error($"Task {taskNumber} was broked on initialization. Collection contains less then 100 elements.");
					return false;
				}
				var document = list[0];
				guid = document["guid"].AsString;
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"Task {taskNumber} was broked on initialization.");
				return false;
			}

			return await Execute(taskNumber, testsWatcher, logger, collection, guid);
		}

		public async Task<bool> Execute(int taskNumber, ITaskWatcher testsWatcher, ILogger logger, IMongoCollection<BsonDocument> collection, string guid)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var executionMode = ExecutionMode.Normal;
			long counter = 0;

			while (!testsWatcher.IsNeedStopTask && executionMode != ExecutionMode.Stopping)
			{
				try
				{
					var filter = Builders<BsonDocument>.Filter.Eq("guid", guid);
					await collection.FindAsync(filter);

					counter++;
					if (stopwatch.Elapsed.TotalSeconds > 1)
					{
						executionMode = ExecutionMode.Normal;
						stopwatch.Restart();
						logger.Trace($"Task {taskNumber} executed {counter} requests.");
					}
				}
				catch (Exception ex)
				{
					logger.Error(ex, $"Task {taskNumber}.");
					if (executionMode == ExecutionMode.Normal)
					{
						executionMode = ExecutionMode.GoingCancel;
						stopwatch.Restart();
					}
					else
					{
						if (stopwatch.Elapsed.TotalSeconds > 10)
							executionMode = ExecutionMode.Stopping;
					}
				}
			}

			logger.Info($"Task {taskNumber} is stopped.");
			return true;
		}
	}
}
