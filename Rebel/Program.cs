using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Rebel
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0 || !File.Exists(args[0]))
			{
				Console.WriteLine("Usage: rebel program.re");
				return;
			}
			try
			{
				RebelProgram interpreter = new RebelProgram(File.ReadAllText(args[0]), new ConsoleIO());
				interpreter.Run();
			}
			catch (InvalidRebelException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
