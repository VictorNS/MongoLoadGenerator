using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;

namespace MongoLoadGenerator
{
	public class InsertExecutor : BaseExecutor, ITaskExecutor
	{
		public async Task<bool> ExecuteAsync(int taskNumber, ITaskWatcher testsWatcher, ILogger logger, string uri)
		{
			var collection = Initialize(taskNumber, logger, uri);
			if (collection == null)
				return false;

			return await Execute(taskNumber, testsWatcher, logger, collection);
		}

		public async Task<bool> Execute(int taskNumber, ITaskWatcher testsWatcher, ILogger logger, IMongoCollection<BsonDocument> collection)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var executionMode = ExecutionMode.Normal;
			long counter = 0;

			BsonDocument document = new BsonDocument();
			while (!testsWatcher.IsNeedStopTask && executionMode != ExecutionMode.Stopping)
			{
				try
				{
					document = new BsonDocument
					{
						{"taskNumber", taskNumber},
						{"date", DateTime.UtcNow},
						{"guid", Guid.NewGuid().ToString()},
					};
					await collection.InsertOneAsync(document);

					counter++;
					if (stopwatch.Elapsed.TotalSeconds > 1)
					{
						executionMode = ExecutionMode.Normal;
						stopwatch.Restart();
						logger.Trace($"Task {taskNumber} inserted {counter} records.");
					}
				}
				catch (Exception ex)
				{
					logger.Error(ex, $"Task {taskNumber} Document.guid {document["guid"].AsString}.");
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
