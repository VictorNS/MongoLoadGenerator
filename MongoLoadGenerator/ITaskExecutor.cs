using System.Threading.Tasks;
using NLog;

namespace MongoLoadGenerator
{
	public interface ITaskExecutor
	{
		Task<bool> ExecuteAsync(int taskNumber, ITaskWatcher testsWatcher, ILogger logger, string uri);
	}
}