using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Communication
{
	public enum Severity
	{
		Info = 0,
		Warn,
		Error,
	}

	public class ValidationResult : IEquatable<ValidationResult>
	{
		public Severity Severity { get; private set; }
		public string Message { get; private set; }

		public ValidationResult(Severity severity, string message)
		{
			Severity = severity;
			Message = message;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ValidationResult);
		}

		public override int GetHashCode()
		{
			return Severity.GetHashCode() * 17 + Message.GetHashCode();
		}

		public bool Equals(ValidationResult other)
		{
			if (ReferenceEquals(other, null)) return false;
			return other.Severity == Severity && other.Message == Message;
		}

		public override string ToString()
		{
			return "{S: " + Severity + " M: " + Message + "}";
		}
	}
}
