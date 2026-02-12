using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using Accessibility;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public abstract class PluginOfficeBase : ICaptureExtension
	{
		protected const string KeyDocumentFullName = "DocumentFullName";
		protected const string KeyDocumentPath = "DocumentPath";
		protected const string KeyDocumentFileName = "DocumentFileName";
		protected const string ProtectedViewClassName = "OPH Previewer Window"; //same for word, excel and powerpoint

		protected HashSet<string> ProcessNamesToCheck { get; set; }
		protected Func<ChildWindowInfo, bool> ChildWindowPredicate { get; set; }

		protected PluginOfficeBase()
		{
			ProcessNamesToCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		public string Id
		{
			get;
			protected set;
		}

		public virtual IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public virtual void SetParameter(string name, string value)
		{
		}

		public virtual IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyDocumentFullName;
			yield return KeyDocumentPath;
			yield return KeyDocumentFileName;
		}

		public abstract IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName);

		//return the first window whose child is not ProtectedViewClassName
		protected static ChildWindowInfo GetFirstNonProtectedChild(List<ChildWindowInfo> childs)
		{
			for (int i = 0; i < childs.Count; i++)
			{
				if (childs[i].ClassName == ProtectedViewClassName) continue;
				if (i + 1 < childs.Count && childs[i + 1].ClassName == ProtectedViewClassName) //child of the current is a ProtectedViewClassName then we have to choose an other window
				{
					i++; //we know that we are not looking for the ProtectedViewClassName
					continue;
				}
				return childs[i];
			}
			return null;
		}

		protected static IDispatch GetNativeObjectFromWindow(IntPtr hWnd)
		{
			var iid = IID_IDispatch;
			return (IDispatch)WinApi.AccessibleObjectFromWindow(hWnd, OBJID_NATIVEOM, ref iid);
		}

		protected static IAccessible GetAccessibleObjectFromWindow(IntPtr hWnd)
		{
			var iid = IID_IAccessible;
			return (IAccessible)WinApi.AccessibleObjectFromWindow(hWnd, OBJID_CLIENT, ref iid);
		}

		private const uint OBJID_NATIVEOM = 0xFFFFFFF0;
		private static readonly Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");

		private const uint OBJID_CLIENT = 0xFFFFFFFC;
		private static readonly Guid IID_IAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");

		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-C000-000000000046")]
		public interface IDispatch
		{
		}

	}
}
