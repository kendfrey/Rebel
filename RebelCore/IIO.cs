using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rebel
{
	public interface IIO
	{
		string ReadLine();
		void WriteLine(string msg);
	}
}
