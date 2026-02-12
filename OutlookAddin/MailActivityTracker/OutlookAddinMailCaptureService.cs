using System;
using System.Linq;
using System.ServiceModel;
using log4net;
using OutlookInteropService;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderClient.Capturing.Mail;
using MailActivityTracker.Model;

namespace MailActivityTracker
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class OutlookAddinMailCaptureService : IAddinMailCaptureService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ThisAddIn thisAddIn;
        private List<byte> menuStream;

        public OutlookAddinMailCaptureService(ThisAddIn thisAddIn)
        {
            this.thisAddIn = thisAddIn;
        }
        public MailCaptures GetMailCaptures()
        {
	        return thisAddIn.GetMailCaptures();
        }

        public void StopService()
        {
            throw new NotImplementedException();
        }

        public void FilterMails(string[] keywords)
        {
            thisAddIn.ApplyFilter(keywords);
        }
        public void TransferMenuData(byte[] buffer)
        {
            if (menuStream == null) menuStream = new List<byte>();
            if (buffer == null || buffer.Length == 0) return;
            menuStream.AddRange(buffer);
        }
        public void UpdateMenu(string placeHolder)
        {
            if (menuStream == null)
            {
                log.Error("Calling UpdateMenu without menu data");
                return;
            }
            MemoryStream inStream = null;
            try
            {
                inStream = new MemoryStream(menuStream.ToArray());
                DataContractSerializer serializer = new DataContractSerializer(typeof(ClientMenu));
                var menu = (ClientMenu)serializer.ReadObject(inStream);
                thisAddIn.UpdateMenu(menu, placeHolder);
            }
            catch (Exception e)
            {
                log.Error("Menu deserialization error:", e);
            }
            finally
            {
                inStream.Dispose();
            }
            menuStream = null;
        }
        public string GetVersion()
        {
            return thisAddIn.Version;
        }


		public void SetMailTrackingBehavior(bool isTrackingEnabled, bool isSubjectTrackingEnabled)
		{
			thisAddIn.TrackingType = isTrackingEnabled ? isSubjectTrackingEnabled ? MailTrackingType.BodyAndSubject : MailTrackingType.BodyOnly : MailTrackingType.Disable;
		}

	    public void SetMailTrackingSettings(MailTrackingSettings settings)
	    {
		    thisAddIn.TrackingSettings = settings;
	    }


		public void Heartbeat()
		{
			thisAddIn.LastHeartbeat = DateTime.UtcNow;
		}

		public void SetTaskIdSettings(int settings)
		{
			ThisAddIn.TaskIdSettings = (MeetingPluginTaskIdSettings)settings;
		}
	}
}
