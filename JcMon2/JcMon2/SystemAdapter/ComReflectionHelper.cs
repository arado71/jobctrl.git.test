using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace JcMon2.SystemAdapter
{
	public static class ComReflectionHelper
	{
		private const int S_OK = 0; //From WinError.h
		private const int LOCALE_SYSTEM_DEFAULT = 2 << 10; //From WinNT.h == 2048 == 0x800
		private static Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");
		private static Guid IID_IAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");


		public static IDispatch GetNativeObject(IntPtr hWnd)
		{
			try
			{
				return (IDispatch)AccessibleObjectFromWindow(hWnd, (uint)0xFFFFFFF0, ref IID_IDispatch);
			}
			catch { }
			return null;
		}

		public static string GetInfo<T>(T obj)
		{
			if (obj == null) return null;
			var sb = new StringBuilder();
			var ct = GetCOMType(obj);

			if (ct != null)
			{
				sb.AppendLine("// Inferred COM stuff");
				GetInfoImpl(ct, sb);
			}

			sb.AppendLine("// Type definition");
			GetInfoImpl(obj.GetType(), sb);
			return sb.ToString();
		}

		private static Type GetCOMType(object obj)
		{
			var dispatch = obj as IDispatchInfo;
			if (dispatch == null) return null;
			Type result = null;
			int typeInfoCount;
			int hr = dispatch.GetTypeInfoCount(out typeInfoCount);
			if (hr == S_OK && typeInfoCount > 0)
			{
				dispatch.GetTypeInfo(0, LOCALE_SYSTEM_DEFAULT, out result);
			}

			return result;
		}

		private static void GetInfoImpl(Type t, StringBuilder sb)
		{
			/*foreach (var a in t.CustomAttributes)
			{
				sb.AppendLine("[" + a.AttributeType.Name + "]");
			}*/
			if (t.IsPublic) sb.Append("public");
			if (t.IsAbstract) sb.Append(" abstract");
			if (t.IsSealed) sb.Append(" sealed");
			if (t.IsEnum) sb.Append(" enum");
			if (t.IsClass) sb.Append(" class");
			if (t.IsInterface) sb.Append(" interface");
			sb.Append(" " + t.Name);
			if (t.IsGenericType) sb.Append("<>");
			sb.Append(" : ");
			if (t.BaseType != null) sb.Append(t.BaseType.Name + ", ");
			sb.Append(string.Join(", ", t.GetInterfaces().Select(x => x.Name).ToArray()));
			sb.AppendLine();
			sb.AppendLine("{");
			sb.AppendLine("\t//Constructors");
			foreach (var c in t.GetConstructors())
			{
				sb.Append("\t");
				if (c.IsPublic) { sb.Append("public "); }
				if (c.IsAssembly) sb.Append("internal ");
				sb.Append(c.Name + "(");
				sb.Append(string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name).ToArray()));
				sb.AppendLine("){...}");
			}
			sb.AppendLine("\t//Methods");
			foreach (var c in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy))
			{
				sb.Append("\t");
				if (c.IsPublic) sb.Append("public ");
				if (c.IsPrivate) sb.Append("private ");
				if (c.IsAssembly) sb.Append("internal ");
				if (c.IsStatic) sb.Append("static ");
				if (c.IsVirtual) sb.Append("virtual ");
				sb.Append(c.ReturnType.Name + " ");
				sb.Append(c.Name + "(");
				sb.Append(string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name).ToArray()));
				sb.AppendLine("){...}");
			}

			sb.AppendLine("}");
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("00020400-0000-0000-C000-000000000046")]
		private interface IDispatchInfo
		{
			[PreserveSig]
			int GetTypeInfoCount(out int typeInfoCount);

			void GetTypeInfo(int typeInfoIndex, int lcid, [MarshalAs(UnmanagedType.CustomMarshaler,
				MarshalTypeRef = typeof(System.Runtime.InteropServices.CustomMarshalers.TypeToTypeInfoMarshaler))] out Type typeInfo);

			[PreserveSig]
			int GetDispId(ref Guid riid, ref string name, int nameCount, int lcid, out int dispId);
		}

		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-C000-000000000046")]
		public interface IDispatch
		{
		}

		[DllImport("oleacc.dll", PreserveSig = false, CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Interface)]
		private static extern object AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid);
	}
}
