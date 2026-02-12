using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Screenshots
{
	public class DecodeId : IEquatable<DecodeId>
	{
		private readonly int userId, computerId, screenNumber;

		public DecodeId(int userId, int computerId, int screenNumber)
		{
			this.userId = userId;
			this.computerId = computerId;
			this.screenNumber = screenNumber;
		}

		public bool Equals(DecodeId other)
		{
			return this.userId == other.userId
				&& this.computerId == other.computerId
				&& this.screenNumber == other.screenNumber;
		}

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, null))
				return false;
			if (Object.ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != this.GetType())
				return false;

			return this.Equals(obj as DecodeId);
		}

		public override int GetHashCode()
		{
			return ((userId * 31) + computerId) * 31 + screenNumber;
		}

		public static bool operator ==(DecodeId left, DecodeId right)
		{
			if (Object.ReferenceEquals(left, null))
				return Object.ReferenceEquals(right, null);
			return left.Equals(right);
		}

		public static bool operator !=(DecodeId left, DecodeId right)
		{
			return !(left == right);
		}
	}
}
