using PowerArgs;

namespace MongoLoadGenerator
{
	public class MyArgs
	{
		[ArgShortcut("t")]
		[ArgDefaultValue(1)]
		[ArgRange(1, 300)]
		public int AmountOfTasks { get; set; }

		[ArgShortcut("r")]
		[ArgDefaultValue(false)]
		public bool ReadMode { get; set; }

		[ArgShortcut("u")]
		[ArgRequired]
		public string RequestUri { get; set; }

		public override string ToString()
		{
			return $"AmountOfTasks={AmountOfTasks} RequestUri='{RequestUri}' ReadMode={ReadMode}";
		}
	}
}
