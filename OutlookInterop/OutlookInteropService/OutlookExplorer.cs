using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookInteropService
{
	public class OutlookExplorer : OutlookWindowWrapper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Outlook.Explorer Window { get; private set; }
		public bool IsOutboxSelected { get; private set; }

		public delegate void OnSelectionChangeHandler(OutlookExplorer explorer, Outlook.MailItem mailItem, bool isOutboxSelected);
		public event OnSelectionChangeHandler SelectionChange;

		private string lastEntryId;
		private string outboxEntryId;

		public OutlookExplorer(Outlook.Explorer explorer, IntPtr handle, Func<OutlookItem, string, string> getJcIdFromMailItem, Action<Action> contextPost, bool useRedemption) : base(getJcIdFromMailItem, contextPost, useRedemption)
		{
			Window = explorer;
			Handle = handle;
			Outlook.NameSpace ns = null;
			Outlook.MAPIFolder outboxFolder = null;
			try
			{
				ns = explorer.Application.GetNamespace("MAPI");
				outboxFolder = ns.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderOutbox);
				outboxEntryId = outboxFolder.EntryID;
			}
			finally
			{
				if (outboxFolder != null) Marshal.ReleaseComObject(outboxFolder);
				if (ns != null) Marshal.ReleaseComObject(ns);
			}
			IsOutboxSelected = Window?.CurrentFolder?.EntryID == outboxEntryId;
			((Outlook.ExplorerEvents_Event)explorer).Close += ExplorerEventsOnClose;
			explorer.SelectionChange += ExplorerOnSelectionChange;
			explorer.InlineResponse += ExplorerOnInlineResponse;
			explorer.InlineResponseClose += ExplorerOnInlineResponseClose;
			explorer.FolderSwitch += ExplorerOnFolderSwitch;
			SetSelection();
		}

		private void ExplorerOnFolderSwitch()
		{
			IsOutboxSelected = Window?.CurrentFolder?.EntryID == outboxEntryId;
		}

		private void ExplorerOnSelectionChange()
		{
			contextPost(() =>
			{
				try
				{
					if (Window == null) return;
					SetSelection();
				}
				catch (Exception ex)
				{
					log.Error("SelectChange handler failed", ex);
				}
			});
		}

		private void ExplorerOnInlineResponseClose()
		{
			contextPost(() =>
			{
				try
				{
					if (Window == null) return;
					DeInitInline();
					lastEntryId = null;
					SetSelection(); // recover originally selected item

				}
				catch (Exception ex)
				{
					log.Error("InlineResponseClose handler failed", ex);
				}
			});
		}

		private void ExplorerOnInlineResponse(object item)
		{
			try
			{
				if (!(item is Outlook.MailItem mailItem) || Window == null) return;
				Debug.Assert(InlineItem == null);
				InlineItem = new OutlookMailItemWrapper(mailItem, IsOutboxSelected);
				InitInline();

			}
			catch (Exception ex)
			{
				log.Error("InlineResponse handler failed", ex);
			}
		}

		private void SetSelection()
		{
			try
			{
				dynamic selectedObj;
				try
				{
					if (Window.Selection.Count == 0) return;
					selectedObj = Window.Selection[1];
				}
				catch (COMException ex)
				{
					log.Warn("Couldn't set selection. Maybe the explorer object has no selected item.");
					return;
				}
				catch (IndexOutOfRangeException ex)
				{
					log.Warn("Couldn't set selection. Maybe the explorer object has no selected item.", ex);
					return;
				}
				catch (InvalidCastException ex)
				{
					log.Warn("Couldn't set selection. Maybe the explorer object has no selected item.", ex);
					return;
				}

				DisableDelayTimer();
				var prevItem = Item;
				var selected = selectedObj as Outlook.MailItem;
				var changed = lastEntryId == null || selected != null && lastEntryId != selected.EntryID;
				Item = new OutlookMailItemWrapper(selectedObj, IsOutboxSelected);
				lastEntryId = Item?.EntryID;
				if (!changed) return;
				DeInit(prevItem);
				if (Mail == null) return;
				Init();
				SelectionChange?.Invoke(this, Mail, IsOutboxSelected);
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error occurred", ex);
			}
		}

		private void ExplorerEventsOnClose()
		{
			contextPost(() =>
			{
				try
				{
					if (Window == null) return;
					BeforeClose();
					Dispose();

				}
				catch (Exception ex)
				{
					log.Error("Close handler failed", ex);
				}
			});
		}

		public override void Dispose()
		{
			if (Window == null) return;
			Window.SelectionChange -= ExplorerOnSelectionChange;
			Window.InlineResponse -= ExplorerOnInlineResponse;
			Window.InlineResponseClose -= ExplorerOnInlineResponseClose;
			Window.FolderSwitch -= ExplorerOnFolderSwitch;
			try
			{
				((Outlook.ExplorerEvents_Event) Window).Close -= ExplorerEventsOnClose;
			}
			catch (Exception ex)
			{
				log.Warn("event unsubscribing failed", ex);
			}
			Marshal.ReleaseComObject(Window);
			Window = null;
			base.Dispose();
		}
	}
}
