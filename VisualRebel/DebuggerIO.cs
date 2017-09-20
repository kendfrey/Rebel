using Rebel;
using System;
using System.Collections.Concurrent;
using System.Windows.Threading;

namespace VisualRebel
{
	class DebuggerIO : IIO
	{
		BlockingCollection<string> input;
		Action<string> callback;
		Dispatcher dispatcher;

		public DebuggerIO(BlockingCollection<string> inputQueue, Action<string> echoCallback, Dispatcher callbackDispatcher)
		{
			input = inputQueue;
			callback = echoCallback;
			dispatcher = callbackDispatcher;
		}

		public string ReadLine()
		{
			string line = input.Take();
			dispatcher.Invoke(callback, line);
			return line;
		}

		public void WriteLine(string msg)
		{
			dispatcher.Invoke(callback, msg);
		}
	}
}
