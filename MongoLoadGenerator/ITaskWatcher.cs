
namespace MongoLoadGenerator
{
	public class TaskWatcher : ITaskWatcher
	{
		public bool IsNeedStopTask { get; set; }

		public TaskWatcher()
		{
			IsNeedStopTask = false;
		}
	}

	public interface ITaskWatcher
	{
		bool IsNeedStopTask { get; set; }
	}
}
