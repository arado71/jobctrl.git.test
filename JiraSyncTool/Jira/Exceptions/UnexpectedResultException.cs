using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraSyncTool.Jira.Exceptions
{
	public class UnexpectedResultException : Exception
	{
		public UnexpectedResultException(string message) : base(message) { }
	}
}
