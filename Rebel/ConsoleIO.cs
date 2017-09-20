using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rebel
{
	class ConsoleIO : IIO
	{
		public string ReadLine()
		{
			return Console.ReadLine();
		}

		public void WriteLine(string msg)
		{
			Console.WriteLine(msg);
		}
	}
}
