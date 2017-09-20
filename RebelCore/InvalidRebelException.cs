using System;
using System.Runtime.Serialization;

namespace Rebel
{
	[Serializable]
	public class InvalidRebelException : Exception
	{
		public InvalidRebelException() { }
		public InvalidRebelException(string message) : base(message) { }
		public InvalidRebelException(string message, Exception inner) : base(message, inner) { }
		protected InvalidRebelException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}
}
