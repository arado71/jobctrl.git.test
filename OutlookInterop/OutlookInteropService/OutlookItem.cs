using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookInteropService
{
	public class OutlookItem : IDisposable
	{
		private object m_item;  // the wrapped Outlook item
		private Type m_type;  // type for the Outlook item 
		private object[] m_args;  // dummy argument array
		private Type m_typeOlObjectClass;
		private Outlook.OlObjectClass m_olObjectClass;
		public bool IsInOutbox { get; }

		#region OutlookItem Constants

		private const string OlActions = "Actions";
		private const string OlApplication = "Application";
		private const string OlAttachments = "Attachments";
		private const string OlBillingInformation = "BillingInformation";
		private const string OlBody = "Body";
		private const string OlCategories = "Categories";
		private const string OlClass = "Class";
		private const string OlClose = "Close";
		private const string OlCompanies = "Companies";
		private const string OlConversationIndex = "ConversationIndex";
		private const string OlConversationTopic = "ConversationTopic";
		private const string OlCopy = "Copy";
		private const string OlCreationTime = "CreationTime";
		private const string OlDisplay = "Display";
		private const string OlDownloadState = "DownloadState";
		private const string OlEntryId = "EntryID";
		private const string OlFormDescription = "FormDescription";
		private const string OlGetInspector = "GetInspector";
		private const string OlImportance = "Importance";
		private const string OlIsConflict = "IsConflict";
		private const string OlItemProperties = "ItemProperties";
		private const string OlLastModificationTime = "LastModificationTime";
		private const string OlLinks = "Links";
		private const string OlMarkForDownload = "MarkForDownload";
		private const string OlMessageClass = "MessageClass";
		private const string OlMileage = "Mileage";
		private const string OlMove = "Move";
		private const string OlNoAging = "NoAging";
		private const string OlOutlookInternalVersion = "OutlookInternalVersion";
		private const string OlOutlookVersion = "OutlookVersion";
		private const string OlParent = "Parent";
		private const string OlPrintOut = "PrintOut";
		private const string OlPropertyAccessor = "PropertyAccessor";
		private const string OlSave = "Save";
		private const string OlSaveAs = "SaveAs";
		private const string OlSaved = "Saved";
		private const string OlSenderEmailType = "SenderEmailType";
		private const string OlSensitivity = "Sensitivity";
		private const string OlSession = "Session";
		private const string OlShowCategoriesDialog = "ShowCategoriesDialog";
		private const string OlSize = "Size";
		private const string OlSubject = "Subject";
		private const string OlUnRead = "UnRead";
		private const string OlUserProperties = "UserProperties";
		#endregion

		#region Constructor
		public OutlookItem(object item, bool isInOutbox)
		{
			this.IsInOutbox = isInOutbox;
			m_item = item;
			m_type = m_item.GetType();
			m_args = new Object[] { };
		}
		#endregion

		#region Public Methods and Properties
		public Outlook.Actions Actions
		{
			get
			{
				return GetPropertyValue(OlActions) as Outlook.Actions;
			}
		}

		public Outlook.Application Application
		{
			get
			{
				return GetPropertyValue(OlApplication) as Outlook.Application;
			}
		}

		public Outlook.Attachments Attachments
		{
			get
			{
				return GetPropertyValue(OlAttachments) as Outlook.Attachments;
			}
		}

		public string BillingInformation
		{
			get
			{
				return GetPropertyValue(OlBillingInformation).ToString();
			}
			set
			{
				SetPropertyValue(OlBillingInformation, value);
			}
		}

		public string Body
		{
			get
			{
				return GetPropertyValue(OlBody).ToString();
			}
			set
			{
				SetPropertyValue(OlBody, value);
			}
		}

		public string Categories
		{
			get
			{
				return GetPropertyValue(OlCategories).ToString();
			}
			set
			{
				SetPropertyValue(OlCategories, value);
			}
		}


		public void Close(Outlook.OlInspectorClose SaveMode)
		{
			object[] MyArgs = { SaveMode };
			CallMethod(OlClose);
		}

		public string Companies
		{
			get
			{
				return GetPropertyValue(OlCompanies).ToString();
			}
			set
			{
				SetPropertyValue(OlCompanies, value);
			}
		}

		public Outlook.OlObjectClass Class
		{
			get
			{
				if (m_typeOlObjectClass == null)
				{
					// Note: instantiate dummy ObjectClass enumeration to get type.
					//       type = System.Type.GetType("Outlook.OlObjectClass") doesn't seem to work
					Outlook.OlObjectClass objClass = Outlook.OlObjectClass.olAction;
					m_typeOlObjectClass = objClass.GetType();
					m_olObjectClass = (Outlook.OlObjectClass) System.Enum.ToObject(m_typeOlObjectClass, GetPropertyValue(OlClass));
				}
				return m_olObjectClass;
			}
		}

		public string ConversationIndex
		{
			get
			{
				return GetPropertyValue(OlConversationIndex).ToString();
			}
		}

		public string ConversationTopic
		{
			get
			{
				return GetPropertyValue(OlConversationTopic).ToString();
			}
		}

		public object Copy()
		{
			return (CallMethod(OlCopy));
		}

		public System.DateTime CreationTime
		{
			get
			{
				return (System.DateTime)GetPropertyValue(OlCreationTime);
			}
		}

		public void Display()
		{
			CallMethod(OlDisplay);
		}

		public Outlook.OlDownloadState DownloadState
		{
			get
			{
				return (Outlook.OlDownloadState)GetPropertyValue(OlDownloadState);
			}
		}

		public string EntryID
		{
			get
			{
				return GetPropertyValue(OlEntryId).ToString();
			}
		}

		public Outlook.FormDescription FormDescription
		{
			get
			{
				return (Outlook.FormDescription)GetPropertyValue(OlFormDescription);
			}
		}


		public Object InnerObject
		{
			get
			{
				return m_item;
			}
		}

		public Outlook.Inspector GetInspector
		{
			get
			{
				return GetPropertyValue(OlGetInspector) as Outlook.Inspector;
			}
		}

		public Outlook.OlImportance Importance
		{
			get
			{
				return (Outlook.OlImportance)GetPropertyValue(OlImportance);
			}
			set
			{
				SetPropertyValue(OlImportance, value);
			}
		}

		public bool IsConflict
		{
			get
			{
				return (bool)GetPropertyValue(OlIsConflict);
			}
		}

		public Outlook.ItemProperties ItemProperties
		{
			get
			{
				return (Outlook.ItemProperties)GetPropertyValue(OlItemProperties);
			}
		}

		public System.DateTime LastModificationTime
		{
			get
			{
				return (System.DateTime)GetPropertyValue(OlLastModificationTime);
			}
		}

		public Outlook.Links Links
		{
			get
			{
				return GetPropertyValue(OlLinks) as Outlook.Links;
			}
		}

		public Outlook.OlRemoteStatus MarkForDownload
		{
			get
			{
				return (Outlook.OlRemoteStatus)GetPropertyValue(OlMarkForDownload);
			}
			set
			{
				SetPropertyValue(OlMarkForDownload, value);
			}
		}

		public string MessageClass
		{
			get
			{
				return GetPropertyValue(OlMessageClass).ToString();
			}
			set
			{
				SetPropertyValue(OlMessageClass, value);
			}
		}

		public string Mileage
		{
			get
			{
				return GetPropertyValue(OlMileage).ToString();
			}
			set
			{
				SetPropertyValue(OlMileage, value);
			}
		}

		public object Move(Outlook.Folder DestinationFolder)
		{
			object[] myArgs = { DestinationFolder };
			return CallMethod(OlMove, myArgs);
		}

		public bool NoAging
		{
			get
			{
				return (bool)GetPropertyValue(OlNoAging);
			}
			set
			{
				SetPropertyValue(OlNoAging, value);
			}
		}

		public long OutlookInternalVersion
		{
			get
			{
				return (long)GetPropertyValue(OlOutlookInternalVersion);
			}
		}

		public string OutlookVersion
		{
			get
			{
				return GetPropertyValue(OlOutlookVersion).ToString();
			}
		}

		public Outlook.Folder Parent
		{
			get
			{
				return GetPropertyValue(OlParent) as Outlook.Folder;
			}
		}

		public Outlook.PropertyAccessor PropertyAccessor
		{
			get
			{
				return GetPropertyValue(OlPropertyAccessor) as Outlook.PropertyAccessor;
			}
		}

		public void PrintOut()
		{
			CallMethod(OlPrintOut);
		}

		public void Save()
		{
			CallMethod(OlSave);
		}

		public void SaveAs(string path, Outlook.OlSaveAsType type)
		{
			object[] myArgs = { path, type };
			CallMethod(OlSaveAs, myArgs);
		}

		public bool Saved
		{
			get
			{
				return (bool)GetPropertyValue(OlSaved);
			}
		}

		public string SenderEmailType
		{
			get
			{
				return (string)GetPropertyValue(OlSenderEmailType);
			}
			set
			{
				SetPropertyValue(OlSenderEmailType, value);
			}
		}

		public Outlook.OlSensitivity Sensitivity
		{
			get
			{
				return (Outlook.OlSensitivity)GetPropertyValue(OlSensitivity);
			}
			set
			{
				SetPropertyValue(OlSensitivity, value);
			}
		}

		public Outlook.NameSpace Session
		{
			get
			{
				return GetPropertyValue(OlSession) as Outlook.NameSpace;
			}
		}

		public void ShowCategoriesDialog()
		{
			CallMethod(OlShowCategoriesDialog);
		}

		public long Size
		{
			get
			{
				return (long)GetPropertyValue(OlSize);
			}
		}

		public string Subject
		{
			get
			{
				return GetPropertyValue(OlSubject).ToString();
			}
			set
			{
				SetPropertyValue(OlSubject, value);
			}
		}

		public bool UnRead
		{
			get
			{
				return (bool)GetPropertyValue(OlUnRead);
			}
			set
			{
				SetPropertyValue(OlUnRead, value);
			}
		}

		public Outlook.UserProperties UserProperties
		{
			get
			{
				return GetPropertyValue(OlUserProperties) as Outlook.UserProperties;
			}
		}

		#endregion

		#region Private Helper Functions
		private object GetPropertyValue(string propertyName)
		{
			try
			{
				// An invalid property name exception is propagated to client
				return m_type.InvokeMember(
					propertyName,
					BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty,
					null,
					m_item,
					m_args);
			}
			catch (SystemException ex)
			{
				Debug.WriteLine(
					string.Format(
					"OutlookItem: GetPropertyValue for {0} Exception: {1} ",
					propertyName, ex.Message));
				throw;
			}
		}

		private void SetPropertyValue(string propertyName, object propertyValue)
		{
			try
			{
				m_type.InvokeMember(
					propertyName,
					BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty,
					null,
					m_item,
					new object[] { propertyValue });
			}
			catch (SystemException ex)
			{
				Debug.WriteLine(
				   string.Format(
				   "OutlookItem: SetPropertyValue for {0} Exception: {1} ",
				   propertyName, ex.Message));
				throw;
			}
		}

		private object CallMethod(string methodName)
		{
			try
			{
				// An invalid property name exception is propagated to client
				return m_type.InvokeMember(
					methodName,
					BindingFlags.Public | BindingFlags.InvokeMethod,
					null,
					m_item,
					m_args);
			}
			catch (SystemException ex)
			{
				Debug.WriteLine(
					string.Format(
					"OutlookItem: CallMethod for {0} Exception: {1} ",
					methodName, ex.Message));
				throw;
			}
		}

		private object CallMethod(string methodName, object[] args)
		{
			try
			{
				// An invalid property name exception is propagated to client
				return m_type.InvokeMember(
					methodName,
					BindingFlags.Public | BindingFlags.InvokeMethod,
					null,
					m_item,
					args);
			}
			catch (SystemException ex)
			{
				Debug.WriteLine(
					string.Format(
					"OutlookItem: CallMethod for {0} Exception: {1} ",
					methodName, ex.Message));
				throw;
			}
		}
		#endregion

		public virtual void Dispose()
		{
			if (InnerObject != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(InnerObject);
		}
	}
}
