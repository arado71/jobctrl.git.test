using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using log4net;

namespace Tct.ActivityRecorderClient.Serialization
{
	public static class CloneHelper
	{
		/// <summary>
		/// Makes a deep copy of an object with rebuilding object structure.
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="obj">The object to be cloned</param>
		/// <returns></returns>
		public static T DeepClone<T>(this T obj)
		{
			if (obj == null) return default(T);
			var type = obj.GetType();
			if (type.IsPrimitive || type.IsValueType)
				return obj;
			var copyMethod = type.GetMethod("Copy", new[] { type });
			if (copyMethod != null)
				return (T)copyMethod.Invoke(null, new object[] { obj });
			if (type.IsGenericType)
			{
				var emr = obj as System.Collections.IEnumerable;
				if (emr == null) throw new ArgumentException("not supported type: " + type.Name);
				var emrcnstr = type.GetConstructor(new[] {type});
				if (emrcnstr == null) throw new ArgumentException("target type can't be constructed: " + type.Name);
				var typeArguments = type.GetGenericArguments();
				if (typeArguments.Length > 2) throw  new ArgumentException("too many type parameters: " + type.Name + " " + typeArguments);
				var genericType = (typeArguments.Length > 1 ? typeof(Dictionary<,>) : typeof (List<>)).MakeGenericType(typeArguments);
				var list = Activator.CreateInstance(genericType);
				var addMethod = genericType.GetMethod("Add", typeArguments);
				foreach (var item in emr)
				{
					if (typeArguments.Length > 1 /* item.GetType().IsInstanceOfType(typeof (KeyValuePair<,>)) */)
					{
						var key = item.GetType().GetProperty("Key").GetValue(item, null);
						var value = item.GetType().GetProperty("Value").GetValue(item, null);
						addMethod.Invoke(list, new[] { key.DeepClone(), value.DeepClone() });
					}
					else addMethod.Invoke(list, new[] {item.DeepClone()});
				}
				return (T)emrcnstr.Invoke(new [] {list});
			}
			object result;
			try
			{
				result = FormatterServices.GetUninitializedObject(type);
			}
			catch (MissingMethodException)
			{
				Debug.Fail("object cannot be initialized type=" + type);
				LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Error("object cannot be initialized type=" + type);
				throw;
			}
			foreach (var prop in type.GetProperties().Where(prop => prop.CanWrite))
				prop.SetValue(result, prop.GetValue(obj, null).DeepClone(), null);
			foreach (var fild in type.GetFields().Where(f => !f.IsInitOnly && !f.IsLiteral && !f.IsNotSerialized && !f.IsStatic))
				fild.SetValue(result, fild.GetValue(obj).DeepClone());
			return (T)result;
		}
	}
}
