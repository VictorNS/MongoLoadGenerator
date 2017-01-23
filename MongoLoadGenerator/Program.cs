using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using PowerArgs;

namespace MongoLoadGenerator
{
	class Program
	{
		static int Main(string[] args)
		{
			ILogger logger;
			try
			{
				logger = LogManager.GetCurrentClassLogger();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex);
				Console.ReadKey();
				return 1;
			}

			MyArgs pars;
			try
			{
				pars = Args.Parse<MyArgs>(args);
			}
			catch (Exception ex)
			{
				logger.Error(ArgUsage.GenerateUsageFromTemplate<MyArgs>());
				logger.Fatal(ex);
				return 1;
			}
			logger.Info(pars.ToString());

			return runTasks(logger, pars);
		}

		private static int runTasks(ILogger logger, MyArgs pars)
		{
			ITaskExecutor tasker = pars.ReadMode ? (ITaskExecutor) new ReadExecutor() : (ITaskExecutor) new InsertExecutor();

			ITaskWatcher taskWatcher = new TaskWatcher();
			var taskList = new List<Task<bool>>();
			for (var i = 0; i < pars.AmountOfTasks; i++)
			{
				try
				{
					var task = tasker.ExecuteAsync(i, taskWatcher, logger, pars.RequestUri);
					taskList.Add(task);
				}
				catch (Exception ex)
				{
					logger.Error(ex);
				}
			}
			if (taskList.Count == 0)
				return 1;

			Task.WaitAll(taskList.Cast<Task>().ToArray());

			return taskList.Count == taskList.Count(t => t.Result) ? 0 : 1;
		}
	}
}
