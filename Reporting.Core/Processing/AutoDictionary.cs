using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Processing
{
	[Serializable]
	internal class AutoDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		private readonly Func<TValue> valueInitializer;

		protected AutoDictionary(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			valueInitializer = (Func<TValue>)info.GetValue("valueInitializer", typeof(Func<TValue>));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("valueInitializer", valueInitializer, typeof(Func<TValue>));
		}

		public AutoDictionary(Func<TValue> valueInitializer = null)
		{
			this.valueInitializer = valueInitializer ?? (() => default(TValue));
		}

		public AutoDictionary(IDictionary<TKey, TValue> other, Func<TValue> valueInitializer)
			: base(other)
		{
			this.valueInitializer = valueInitializer;
		}

		public new TValue this[TKey key]
		{
			get
			{
				TValue val;
				if (!base.TryGetValue(key, out val))
				{
					return (base[key] = valueInitializer());
				}
				else
				{
					return val;
				}
			}

			set
			{
				base[key] = value;
			}
		}
	}
}
