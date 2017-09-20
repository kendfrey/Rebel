using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rebel
{
	public delegate void DebugStepEventHandler(object sender, DebugStepEventArgs e);

	public class DebugStepEventArgs : EventArgs
	{
		public string OldData
		{
			get;
			set;
		}

		public string NewData
		{
			get;
			set;
		}

		public int ReplacementStart
		{
			get;
			set;
		}

		public int OldReplacementLength
		{
			get;
			set;
		}

		public int NewReplacementLength
		{
			get;
			set;
		}

		public Tuple<Regex, string> ReplacementPair
		{
			get;
			set;
		}

		public AutoResetEvent Continue
		{
			get;
			set;
		}
	}
}
