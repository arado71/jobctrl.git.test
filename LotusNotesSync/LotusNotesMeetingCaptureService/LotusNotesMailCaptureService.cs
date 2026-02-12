using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using OutlookInteropService;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using System.ServiceModel;

namespace LotusNotesMeetingCaptureService
{
	class LotusNotesMailCaptureService : IMailCaptureService
	{
		private static readonly log4net.ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Func<Tuple<int, IntPtr>, string> processNameResolverFunc = CachedFunc.Create<Tuple<int, IntPtr>, string>(t => ResolveProcessNameFromId(t.Item1, t.Item2), TimeSpan.FromSeconds(60)); //pids won't be reused in 30 secs
		private readonly bool isElevated = ProcessElevationHelper.IsElevated();
		private int notesProcessId;
		private bool notesElevated;

		public LotusNotesMailCaptureService()
		{
			log.Info("LotusNotesMailCaptureService started " + (isElevated ? "elevated." : "unelevated."));
		}

		public MailCaptures GetMailCaptures()
		{
			var currentWindowHandle = GetForegroundWindow();
			var processId = GetWindowThreadProcessIdWrapper(currentWindowHandle);
			if (processNameResolverFunc(Tuple.Create(processId, currentWindowHandle)) != "notes2.exe") return null;
			if (notesProcessId != processId)
			{
				notesProcessId = processId;
				notesElevated = ProcessElevationHelper.IsElevated(notesProcessId);
			}
			if (isElevated != notesElevated)
			{
				log.Info("Signaling different elevation levels to JC process...");
				throw new FaultException(notesElevated ? "Elevate" : "Unelevate");
			}

			try
			{
				object session = Activator.CreateInstance(Type.GetTypeFromProgID("Notes.NotesSession"));
				object uiWorkspace = Activator.CreateInstance(Type.GetTypeFromProgID("Notes.NotesUIWorkspace"));
				object uiView = uiWorkspace.GetType().InvokeMember("CurrentView", BindingFlags.GetProperty, null, uiWorkspace, null);

				object document;
				if (uiView != null)
				{
					object caretNoteId = uiView.GetType().InvokeMember("CaretNoteID", BindingFlags.GetProperty, null, uiView, null);
					object uiDatabase = uiWorkspace.GetType().InvokeMember("CurrentDatabase", BindingFlags.GetProperty, null, uiWorkspace, null);
					object database = uiDatabase.GetType().InvokeMember("Database", BindingFlags.GetProperty, null, uiDatabase, null);
					document = database.GetType().InvokeMember("GetDocumentByID", BindingFlags.InvokeMethod, null, database, new[] { caretNoteId });
					//viewCapture = GetMailCapture(session, document);
				}
				else
				{
					object uiDocument = uiWorkspace.GetType().InvokeMember("CurrentDocument", BindingFlags.GetProperty, null, uiWorkspace, null);
					if (uiDocument == null) return null;
					document = uiDocument.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, uiDocument, null);
					//object windowHandle = uiDocument.GetType().InvokeMember("WindowHandle", BindingFlags.GetProperty, null, uiDocument, null);
					//object noteId = document.GetType().InvokeMember("NoteID", BindingFlags.GetProperty, null, document, null);
					//docCapture = GetMailCapture(session, document);
				}

				return new MailCaptures()
				{
					MailCaptureByHWnd = new Dictionary<int, MailCapture>()
					{
						{ currentWindowHandle.ToInt32(), GetMailCapture(session, document) },
					}
				};
			}
			catch (Exception /*e*/)
			{
				//log.Error("Error occured during mail capture.", e);
				return null;
			}
		}

		public void StopService()
		{
			throw new NotImplementedException();
		}

		private static MailCapture GetMailCapture(object session, object document)
		{
			var from = GetMailAddresses(session, document, "From");
			var to = GetMailAddresses(session, document, "SendTo");
			var cc = GetMailAddresses(session, document, "CopyTo");
			var subject = GetFirstItemValue<string>(document, "Subject");
			var id = GetFirstItemValue<string>(document, "NoteID");

			return new MailCapture()
			{
				Id = id,
				From = from != null && from.Count > 0 ? from[0] : null,
				To = to,
				Cc = cc,
				Subject = subject,
			};
		}

		private static List<MailAddress> GetMailAddresses(object session, object document, string propName)
		{
			Debug.Assert(propName.Equals("From", StringComparison.InvariantCultureIgnoreCase)
				|| propName.Equals("SendTo", StringComparison.InvariantCultureIgnoreCase)
				|| propName.Equals("CopyTo", StringComparison.InvariantCultureIgnoreCase));

			var relatedINetPropName = "INet" + propName;

			var propValues = GetItemValueArray<string>(document, propName);
			if (propValues == null || propValues.Length == 0) return null;
			var relatedINetPropValues = GetItemValueArray<string>(document, relatedINetPropName);
			//Debug.Assert(relatedINetPropValues == null || relatedINetPropValues.Length == propValues.Length);

			var result = new List<MailAddress>();

			for (int i = 0; i < propValues.Length; i++)
			{
				var propValue = propValues[i];
				if (String.IsNullOrEmpty(propValue)) continue;

				var relatedINetPropValue = relatedINetPropValues != null && relatedINetPropValues.Length > i ? relatedINetPropValues[i] : null;
				result.Add(GetMailAddress(session, propValue, relatedINetPropValue));
			}

			return result;
		}

		private static MailAddress GetMailAddress(object session, string namePropValue, string relatedINetPropValue)
		{
			if (String.IsNullOrEmpty(namePropValue)) return null;

			var result = new MailAddress();

			object notesName = session.GetType().InvokeMember("CreateName", BindingFlags.InvokeMethod, null, session, new object[] { namePropValue });
			var isHierarchicalName = (bool)notesName.GetType().InvokeMember("IsHierarchical", BindingFlags.GetProperty, null, notesName, null);
			if (isHierarchicalName)
			{
				result.Name = notesName.GetType().InvokeMember("Common", BindingFlags.GetProperty, null, notesName, null) as string;
				if (!String.IsNullOrEmpty(relatedINetPropValue) && relatedINetPropValue != ".") //TODO: validating address
				{
					result.Email = relatedINetPropValue;
				}
				else
				{
					//var nameLookupRes = session.GetType()
					//	.InvokeMember("Evaluate", BindingFlags.InvokeMethod, null, session,
					//		new object[] { "@NameLookup([Exhaustive];Chair;'InternetAddress')", document });	//http://publib.boulder.ibm.com/infocenter/domhelp/v8r0/topic/com.ibm.designer.domino.main.doc/H_NAMELOOKUP_7699.html
					//result.Email = ((object[])nameLookupRes)[0].ToString();

					result.Email = CachedGetInternetAddress(session, namePropValue);	//Without cahcing it is too slow, communicate with server, makes LN mouse cursor flicker
				}
			}
			else//Interpret propNameValue as RFC822 internet address
			{
				result.Name = notesName.GetType().InvokeMember("Addr822Phrase", BindingFlags.GetProperty, null, notesName, null) as string;
				result.Email = notesName.GetType().InvokeMember("Addr821", BindingFlags.GetProperty, null, notesName, null) as string;
			}

			return result;
		}

		private static T GetFirstItemValue<T>(object document, string parameterName)
		{
			return (T)GetItemValueArray<object>(document, parameterName)[0];
		}

		private static T[] GetItemValueArray<T>(object document, string parameterName)
		{
			var obj = document.GetType().InvokeMember("GetItemValue", BindingFlags.InvokeMethod, null, document, new object[] { parameterName });
			return typeof(T).IsSubclassOf(typeof(object))
				//? Array.ConvertAll((object[]) obj, x => (T) Convert.ChangeType(x, typeof (T)))
					   ? ((IEnumerable)obj).Cast<object>().Select(x => (T)Convert.ChangeType(x, typeof(T))).ToArray()
					   : (T[])obj;
		}

		private static readonly CachedDictionary<string, string> addressCache = new CachedDictionary<string, string>(TimeSpan.FromMinutes(10), true);

		private static string CachedGetInternetAddress(object session, string userName)
		{
			string address;

			if (!addressCache.TryGetValue(userName, out address))
			{
				address = GetInternetAddress(session, userName);
				addressCache.Add(userName, address);
			}

			return address;
		}

		private static string GetInternetAddress(object session, string userName)
		{
			var addressBook = GetPublicAddressBook(session);

			if (addressBook == null) return null; //Or throw
			var isOpen = (bool)addressBook.GetType().InvokeMember("IsOpen", BindingFlags.GetProperty, null, addressBook, null);
			if (isOpen == false) addressBook.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, addressBook, new object[] { "", "" });

			var view = addressBook.GetType().InvokeMember("GetView", BindingFlags.InvokeMethod, null, addressBook, new object[] { "$Users" });
			object name = session.GetType().InvokeMember("CreateName", BindingFlags.InvokeMethod, null, session, new object[] { userName });
			var abbreviated = name.GetType().InvokeMember("Abbreviated", BindingFlags.GetProperty, null, name, null) as string;
			var document = view.GetType().InvokeMember("GetDocumentByKey", BindingFlags.InvokeMethod, null, view, new object[] { abbreviated });

			return document == null ? null : GetFirstItemValue<string>(document, "InternetAddress");
		}

		private static object GetPublicAddressBook(object session)
		{
			var notesDatabases = session.GetType().InvokeMember("AddressBooks", BindingFlags.GetProperty, null, session, null) as object[];
			if (notesDatabases == null) return null;
			foreach (var db in notesDatabases)
			{
				var isPublicAddressBook = (bool)db.GetType().InvokeMember("IsPublicAddressBook", BindingFlags.GetProperty, null, db, null);
				var server = db.GetType().InvokeMember("Server", BindingFlags.GetProperty, null, db, null) as string;
				if (isPublicAddressBook && !String.IsNullOrEmpty(server)) return db;
			}
			return null;
		}

		private static int GetWindowThreadProcessIdWrapper(IntPtr hWnd)
		{
			int procId;
			GetWindowThreadProcessId(hWnd, out procId);
			return procId;
		}

		private static string ResolveProcessNameFromId(int processId, IntPtr hWnd)
		{
			//avoid common exceptions
			switch (processId)
			{
				case -1:
					return "Locked"; //special id indicating that the screen is locked
				case 0:
					return "Idle";
				case 4:
					return "System"; //On XP or later
				default:
					string fileName;
					return (ProcessNameHelper.TryGetProcessName(processId, hWnd, out fileName)) ? fileName : "N\\A";
			}
		}

		#region Interop

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

		#endregion

	}
}
