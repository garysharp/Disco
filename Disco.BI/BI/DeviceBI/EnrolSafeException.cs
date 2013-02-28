using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
namespace Disco.BI
{
	public class EnrolSafeException : System.Exception
	{
		public EnrolSafeException(string Message) : base(Message)
		{
		}
	}
}
