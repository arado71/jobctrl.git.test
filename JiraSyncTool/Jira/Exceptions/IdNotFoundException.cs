using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraSyncTool.Jira.Exceptions
{
	public class IdNotFoundException : Exception
	{
		public IdNotFoundException(string message) : base(message)
		{ }
	}
}
