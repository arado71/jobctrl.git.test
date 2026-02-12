using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading;
using Domino;
using log4net;
using LotusNotesMeetingCaptureService;
using OutlookInteropService;

namespace LotusNotesMeetingCaptureServiceNamespace
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public class LotusNotesMeetingCaptureService : IMeetingCaptureService
	{
		private static readonly log4net.ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private NotesSession session;
		private NotesDatabase db;

		private bool? isLotusNotesInstalled;
		private bool IsLotusNotesInstalled
		{
			get
			{
				if (!isLotusNotesInstalled.HasValue)
				{
					isLotusNotesInstalled = LotusNotesSettingsHelper.IsLotusNotesInstalled;
				}
				return isLotusNotesInstalled.Value;
			}
		}

		public void Initialize(string password, string mailServer, string mailFile)
		{
			NotesSession session = null;
			NotesDatabase db = null;

			log.Info("Initialize started...");
			var sw = Stopwatch.StartNew();

			try
			{
				if (!IsLotusNotesInstalled) throw new InvalidOperationException("Lotus Notes is not installed.");
				if (this.session != null || this.db != null) Uninitialize();

				log.InfoFormat("Initializing connecting to Lotus Notes. (MailServer: {0}, MailFile: {1})", mailServer, mailFile);

				session = GetSession(password);
				Debug.Assert(session != null);

				db = GetMailDatabase(session, mailServer, mailFile);
				if (db == null) throw new Exception("Initialization failed.");

				log.InfoFormat(
					"Initialization succeded. (NotesVersion: {0}, NotesBuildVersion: {1}, Platform: {2}, UserName: {3}, ServerName: {4}, MailFile: {5})",
					session.NotesVersion, session.NotesBuildVersion, session.Platform, session.UserName, db.Server,
					db.FilePath); //session.ServerName returns null

				this.session = session;
				this.db = db;
			}
			catch (Exception e)
			{
				log.Error("Error during connecting to LotusNotes.", e);
				if (this.session != null || this.db != null) Uninitialize();
				if (e is COMException && (uint)((COMException)e).ErrorCode == 0x80040fa0) throw new FaultException("Wrong Password");
				throw;
			}
			finally
			{
				log.InfoFormat("Initialize finished in {0} ms.\n", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}
		}

		public string GetVersionInfo()
		{
			log.Info("GetVersionInfo started...");
			var sw = Stopwatch.StartNew();

			try
			{
				if (!IsLotusNotesInstalled) throw new InvalidOperationException("Lotus Notes is not installed.");
				if (session == null || db == null) throw new InvalidOperationException("NotInitialized");

				return String.Format("NotesVersion: {0}, NotesBuildVersion: {1}, Platform: {2}, UserName: {3}, ServerName: {4}, MailFile: {5}", session.NotesVersion, session.NotesBuildVersion, session.Platform, session.UserName, db.Server, db.FilePath);	//session.ServerName returns null
			}
			catch (Exception e)
			{
				log.Error("Error during retrieving version info from Lotus Notes.", e);
				if (e is COMException && (uint)((COMException)e).ErrorCode == 0x80040fa0) throw new FaultException("Wrong Password");
				throw;
			}
			finally
			{
				log.InfoFormat("GetVersionInfo finished in {0} ms.\n", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}
		}

		public List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate)
		{
			log.InfoFormat("CaptureMeetings started... ({0}, {1}, {2})", String.Join(", ", calendarAccountEmails.ToArray()), startDate, endDate);
			var sw = Stopwatch.StartNew();

			try
			{
				if (!IsLotusNotesInstalled) throw new InvalidOperationException("Lotus Notes is not installed.");
				if (session == null || db == null) throw new InvalidOperationException("NotInitialized");

				var result = new List<FinishedMeetingEntry>();

				//1. Query meetings that ended in the given time interval
				var selectionFormula1 = "SELECT @IsAvailable(CalendarDateTime) & Form = \"Appointment\" & AppointmentType = \"3\" & $PublicAccess = \"1\"";
				selectionFormula1 += String.Format(" & EndDateTime >= {0} & EndDateTime <= {1}", ToFormulaString(startDate), ToFormulaString(endDate));
				result.AddRange(SearchForMeetingsInDb(db, selectionFormula1, (a, b) => (b >= startDate && b <= endDate)));

				//2. Query meetings that created in AND ended before the given time interval 
				var backwardCreationLimit = new TimeSpan(30, 0, 0, 0);
				var selectionFormula2 = "SELECT @IsAvailable(CalendarDateTime) & Form = \"Appointment\" & AppointmentType = \"3\" & $PublicAccess = \"1\"";
				selectionFormula2 += String.Format(" & @Created >= {0} & @Created <= {1} & EndDateTime <= {0} & EndDateTime >= {2}", ToFormulaString(startDate), ToFormulaString(endDate), ToFormulaString(startDate.Subtract(backwardCreationLimit)));
				result.AddRange(SearchForMeetingsInDb(db, selectionFormula2, (a, b) => (b >= startDate.Subtract(backwardCreationLimit) && b <= startDate)));

				log.InfoFormat("CaptureMeetings: Returning the following meetings ({0})", result.Count);
				string finishedMeetingEntriesStr = String.Join("\n", result.Select(m => m.ToString()).ToArray());
				if (!String.IsNullOrEmpty(finishedMeetingEntriesStr)) log.Info(finishedMeetingEntriesStr);

				return result;

				//For repeating meetings EndDateTime field is a list. Unequality checks return true if it is true for any element of the list. (http://publib.boulder.ibm.com/infocenter/domhelp/v8r0/index.jsp?topic=%2Fcom.ibm.designer.domino.main.doc%2FH_OPERATIONS_ON_LISTS.html)
				//But the two unequality checks evaluation is independent from each other. So there no warranty that there is a single elemnt in EndDateTime list that is evaluates to true!!!
				//This is resolved later when we capturing the required instances of the repeating meeting.
			}
			catch (Exception e)
			{
				log.Error(String.Format("Error occured during capturing meetings. (calendarAccountEmails: {0}, startDate: {1}, endDate: {2})", calendarAccountEmails, startDate, endDate), e);
				if (e is COMException && (uint)((COMException)e).ErrorCode == 0x80040fa0) throw new FaultException("Wrong Password");
				throw;
			}
			finally
			{
				log.InfoFormat("CaptureMeetings finished in {0} ms.\n", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}
		}

		private readonly LotusNotesMailCaptureService mailCaptureService = new LotusNotesMailCaptureService();
		public MailCaptures GetMailCaptures()
		{
			return mailCaptureService.GetMailCaptures();
		}

		public void StopService()
		{
			log.Info("StopService called");
			var sw = Stopwatch.StartNew();

			try
			{
				if (session != null || db != null) Uninitialize();

				ServiceHostBase host = OperationContext.Current.Host;
				var sc = SynchronizationContext.Current;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					Thread.Sleep(50);
					sc.Post(__ => host.Close(), null);
				});	//With .NET 4.0 simple post doesn't worked.
			}
			catch (Exception ex)
			{
				log.Error("StopService failed.", ex);
				throw;
			}
			finally
			{
				log.InfoFormat("StopService finished in {0:0.000}ms.", sw.Elapsed.TotalMilliseconds);
			}
		}

		public void Dispose()
		{
			Uninitialize();
		}

		private void Uninitialize()
		{
			ReleaseComObject(db);
			db = null;
			ReleaseComObject(session);
			session = null;
		}

		private static NotesSession GetSession(string password)
		{
			//Options for eliminating password popup:
			//1. Password popup can be turned off in Lotus Notes. //http://stackoverflow.com/questions/20081545/use-existing-lotus-notes-8-5-security-session-via-net-application
			//2. Storing password in JC. (It can be provided in settings by the user, or JC may prompt for it.)
			//3. Reusing an authenticated session. It only popups for password after JC restart.
			//Single login? http://publib.boulder.ibm.com/infocenter/domhelp/v8r0/topic/com.ibm.notes.help.doc/DOC/H_TO_USE_WINDOWS_NT_PASSWORD_SYNCHRONIZATION_WITH_NOTES_OVER.html
			//Notes Shared Logon vs Notes Single Logon (http://www-10.lotus.com/ldd/dominowiki.nsf/dx/Notes_Shared_Login_FAQ)

			var session = new NotesSession();
			session.Initialize(password);	//If password is empty a login dialog will be presented.
			return session;
		}

		private static NotesDatabase GetMailDatabaseOfCurrentUser(NotesSession session)
		{
			return GetMailDatabase(session, "", "");
		}

		private static NotesDatabase GetMailDatabase(NotesSession session, string mailServer, string mailFile)
		{
			NotesDatabase db;

			if (string.IsNullOrEmpty(mailServer) || string.IsNullOrEmpty(mailFile))
			{
				var dbDirectory = session.GetDbDirectory("");	//Empty string means local, but it does not matter. //http://publib.boulder.ibm.com/infocenter/domhelp/v8r0/topic/com.ibm.designer.domino.main.doc/H_OPENMAILDATABASE_METHOD_DBDIRECTORY_COM.html
				db = dbDirectory.OpenMailDatabase();
			}
			else
			{
				db = session.GetDatabase(mailServer, mailFile, false);
				if (db != null && !db.IsOpen) db.Open();
			}

			//Third option would be:
			//var mailServer = session.GetEnvironmentString("MailServer", true);
			//var mailFile = session.GetEnvironmentString("MailFile", true);
			//db = session.GetDatabase(mailServer, mailFile, false);

			return db;
		}

		private static IList<FinishedMeetingEntry> SearchForMeetingsInDb(NotesDatabase db, string selectionFormula, Func<DateTime, DateTime, bool> shouldCaptureRepeatInstance)
		{
			var result = new List<FinishedMeetingEntry>();

			var documentCollection = db.Search(selectionFormula, null, 0);
			var document = documentCollection.GetFirstDocument();

			while (document != null)
			{
				if (GetFirstItemValue<string>(document, "Repeats") == "1") result.AddRange(CaptureRepeatingMeeting(document, shouldCaptureRepeatInstance));
				else result.Add(CaptureMeeting(document));

				document = documentCollection.GetNextDocument(document);
			}

			return result;
		}

		private static FinishedMeetingEntry CaptureMeeting(NotesDocument document)
		{
			return CaptureMeeting(document, 0);
		}

		private static FinishedMeetingEntry CaptureMeeting(NotesDocument document, int repeatInstanceIndex)
		{
			var meetingEntry = new FinishedMeetingEntry();
			meetingEntry.Id = document.UniversalID; //TODO: changge to ApptUNID property //http://www-10.lotus.com/ldd/ddwiki.nsf/dx/cs_schema_glossary#ApptUNID
			meetingEntry.CreationTime = ((DateTime)document.Created).ToUniversalTime();
			meetingEntry.LastmodificationTime = ((DateTime)document.LastModified).ToUniversalTime();
			meetingEntry.Title = GetFirstItemValue<string>(document, "Subject");//Subject vs Topic
			meetingEntry.Description = GetFirstItemValue<string>(document, "Body");
			meetingEntry.Location = GetFirstItemValue<string>(document, "Location");//Room?
			meetingEntry.StartTime = GetItemValueArray<DateTime>(document, "StartDateTime")[repeatInstanceIndex].ToUniversalTime();
			meetingEntry.EndTime = GetItemValueArray<DateTime>(document, "EndDateTime")[repeatInstanceIndex].ToUniversalTime();

			var session = document.ParentDatabase.Parent;

			var chair = GetFirstItemValue<string>(document, "Chair");	//Chair //From, InetFrom //Principal, ($iNetPrincipal)
			if (session.CreateName(chair).IsHierarchical) chair = GetInternetAddress(chair, session);

			//Alternate method for getting internet address
			//chair = ((object[])session.Evaluate("@NameLookup([Exhaustive];Chair;'InternetAddress')", document))[0].ToString();		//http://publib.boulder.ibm.com/infocenter/domhelp/v8r0/topic/com.ibm.designer.domino.main.doc/H_NAMELOOKUP_7699.html

			var internetAddressesForRequiredAttendees = GetItemValueArray<string>(document, "INetRequiredNames");	//RequiredAttendees, INetRequiredNames
			if (internetAddressesForRequiredAttendees.Contains("."))
			{
				var primaryNamesForRequiredAttendees = GetItemValueArray<string>(document, "RequiredAttendees");
				internetAddressesForRequiredAttendees = internetAddressesForRequiredAttendees.Select((x, i) => x != "." ? x : primaryNamesForRequiredAttendees[i]).ToArray();
			}
			internetAddressesForRequiredAttendees = internetAddressesForRequiredAttendees.Select(x => session.CreateName(x).IsHierarchical ? GetInternetAddress(x, session) : x).ToArray();

			var internetAddressesForOptionalAttendees = GetItemValueArray<string>(document, "INetOptionalNames");	//OptionalAttendees, INetOptionalNames
			if (internetAddressesForOptionalAttendees.Contains("."))
			{
				var primaryNamesForOptionalAttendees = GetItemValueArray<string>(document, "OptionalAttendees");
				internetAddressesForOptionalAttendees = internetAddressesForOptionalAttendees.Select((x, i) => x != "." ? x : primaryNamesForOptionalAttendees[i]).ToArray();
			}
			internetAddressesForOptionalAttendees = internetAddressesForOptionalAttendees.Select(x => session.CreateName(x).IsHierarchical ? GetInternetAddress(x, session) : x).ToArray();

			meetingEntry.Attendees = new List<MeetingAttendee>();
			meetingEntry.Attendees.Add(new MeetingAttendee()
			{
				Email = chair,
				Type = MeetingAttendeeType.Organizer,
				ResponseStatus = MeetingAttendeeResponseStatus.ResponseOrganized,
			});
			meetingEntry.Attendees.AddRange(internetAddressesForRequiredAttendees.Where(x => !String.IsNullOrEmpty(x))
							 .Select(x => new MeetingAttendee()
							 {
								 Email = x,
								 Type = MeetingAttendeeType.Required,
								 ResponseStatus = MeetingAttendeeResponseStatus.ResponseNone
							 }));
			meetingEntry.Attendees.AddRange(internetAddressesForOptionalAttendees.Where(x => !String.IsNullOrEmpty(x))
							 .Select(x => new MeetingAttendee()
							 {
								 Email = x,
								 Type = MeetingAttendeeType.Optional,
								 ResponseStatus = MeetingAttendeeResponseStatus.ResponseNone
							 }));

			return meetingEntry;
		}

		private static IList<FinishedMeetingEntry> CaptureRepeatingMeeting(NotesDocument document)
		{
			return CaptureRepeatingMeeting(document, (a, b) => true);
		}

		private static IList<FinishedMeetingEntry> CaptureRepeatingMeeting(NotesDocument document, Func<DateTime, DateTime, bool> shouldCaptureRepeatInstance)
		{
			var result = new List<FinishedMeetingEntry>();

			FinishedMeetingEntry repeatInstance = null;
			var startDateTimeValues = GetItemValueArray<DateTime>(document, "StartDateTime");
			var endDateTimeValues = GetItemValueArray<DateTime>(document, "EndDateTime");
			Debug.Assert(startDateTimeValues.Length == endDateTimeValues.Length);

			for (int i = 0; i < startDateTimeValues.Length; i++)
			{
				if (shouldCaptureRepeatInstance(startDateTimeValues[i], endDateTimeValues[i]))
				{
					repeatInstance = repeatInstance == null ? CaptureMeeting(document, i) : repeatInstance.Clone();
					repeatInstance.StartTime = startDateTimeValues[i].ToUniversalTime();
					repeatInstance.EndTime = endDateTimeValues[i].ToUniversalTime();
					result.Add(repeatInstance);
				}
			}

			return result;
		}

		private static string GetInternetAddress(string userName, NotesSession session)
		{
			var addressBook = GetPublicAddressBook(session);

			if (addressBook == null) return null; //Or throw
			if (addressBook.IsOpen == false) addressBook.Open();

			var view = addressBook.GetView("$Users");
			var name = session.CreateName(userName);
			var document = view.GetDocumentByKey(name.Abbreviated);

			return document == null ? null : GetFirstItemValue<string>(document, "InternetAddress");
		}

		private static NotesDatabase GetPublicAddressBook(NotesSession session)
		{
			var notesDatabases = session.AddressBooks as object[];
			return notesDatabases == null
				? null
				: notesDatabases.OfType<NotesDatabase>().Where(n => n.IsPublicAddressBook && !string.IsNullOrEmpty(n.Server)).FirstOrDefault();
		}

		private static T GetFirstItemValue<T>(NotesDocument document, string parameterName)
		{
			return (T)GetItemValueArray<object>(document, parameterName)[0];
		}

		private static T[] GetItemValueArray<T>(NotesDocument document, string parameterName)
		{
			var obj = document.GetItemValue(parameterName);
			return typeof(T).IsSubclassOf(typeof(object))
				//? Array.ConvertAll((object[]) obj, x => (T) Convert.ChangeType(x, typeof (T)))
					   ? ((IEnumerable)obj).Cast<object>().Select(x => (T)Convert.ChangeType(x, typeof(T))).ToArray()
					   : (T[])obj;
		}

		private static string ToFormulaString(DateTime date)
		{
			return String.Format("@Date({0}; {1}; {2}; {3}; {4}; {5})", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
		}

		private static void ReleaseComObject(object o)
		{
			try
			{
				if (o != null)
				{
					Marshal.ReleaseComObject(o);
					//int rc = Marshal.ReleaseComObject(o);
					//Debug.Assert(rc == 0);	//For example static COM objects will map to the same RCW, so RCW's reference count can be more than one.
				}
			}
			catch (Exception e)
			{
				log.Error("Error occured while releasing COM object.", e);
			}
		}
	}
}
