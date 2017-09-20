using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rebel
{
	public class RebelProgram
	{
		public string Data
		{
			get;
			private set;
		}

		public List<Tuple<Regex, string>> Pairs
		{
			get;
			private set;
		}

		public IIO IO
		{
			get;
			private set;
		}

		public bool DebugMode
		{
			get;
			set;
		}

		public bool Stop
		{
			get;
			set;
		}

		public event DebugStepEventHandler DebugStep;

		public RebelProgram(string program, IIO io)
		{
			Regex escape = new Regex(@"\\(.)" , RegexOptions.Compiled);
			try
			{
				string[] tokens = Regex.Split(program, @"(?<=(?<!\\)(?:\\\\)*)/");
				if (tokens.Length % 2 == 0)
				{
					throw new Exception("Missing replacement string.");
				}
				Data = escape.Replace(tokens[0], "$1");
				Pairs = new List<Tuple<Regex, string>>();
				for (int i = 1; i < tokens.Length; i += 2)
				{
					Pairs.Add(new Tuple<Regex, string>(new Regex(tokens[i], RegexOptions.Compiled), escape.Replace(tokens[i + 1], "$1")));
				}
				IO = io;
			}
			catch (Exception ex)
			{
				throw new InvalidRebelException("Invalid program", ex);
			}
		}

		public void Run()
		{
			Regex input = new Regex(@"\$<", RegexOptions.Compiled);
			bool matchFound;
			do
			{
				matchFound = false;
				foreach (Tuple<Regex, string> pair in Pairs)
				{
					Match match = pair.Item1.Match(Data);
					if (match.Success)
					{
						DebugStepEventArgs e = new DebugStepEventArgs();
						e.OldData = Data;
						string replacement = pair.Item2;
						string output = null;
						int outputLocation = replacement.IndexOf("$>");
						if (outputLocation >= 0)
						{
							output = replacement.Substring(outputLocation + 2);
							replacement = replacement.Substring(0, outputLocation);
						}
						replacement = input.Replace(replacement, m => IO.ReadLine().Replace("$", "$$"));
						string result = match.Result(replacement);
						Data = Data.Substring(0, match.Index) + result + Data.Substring(match.Index + match.Length);
						if (output != null)
						{
							output = input.Replace(output, m => IO.ReadLine().Replace("$", "$$"));
							IO.WriteLine(match.Result(output));
						}
						e.NewData = Data;
						e.ReplacementStart = match.Index;
						e.OldReplacementLength = match.Length;
						e.NewReplacementLength = result.Length;
						e.ReplacementPair = pair;
						OnDebugStep(e);
						matchFound = true;
						break;
					}
				}
			}
			while (matchFound && !Stop);
		}

		private void OnDebugStep(DebugStepEventArgs e)
		{
			if (DebugMode && DebugStep != null)
			{
				DebugStep(this, e);
				if (e.Continue != null)
				{
					e.Continue.WaitOne();
				}
			}
		}
	}
}
