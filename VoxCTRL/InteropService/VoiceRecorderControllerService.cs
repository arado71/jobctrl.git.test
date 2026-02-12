using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using log4net;
using VoxCTRL.Controller;

namespace VoxCTRL.InteropService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public class VoiceRecorderControllerService : IVoiceRecorderControllerService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly RecorderFormController controller;

		public VoiceRecorderControllerService(RecorderFormController controller)
		{
			this.controller = controller;
		}

		public bool Record(string name)
		{
			log.Info("Record invoked from another process. Name: " + name);
			return controller.Record(name);
		}

		public bool Pause()
		{
			log.Info("Pause invoked from another process.");
			return controller.Pause();
		}

		public bool Resume()
		{
			log.Info("Resume invoked from another process.");
			return controller.Resume();
		}

		public bool Stop()
		{
			log.Info("Stop invoked from another process.");
			return controller.Stop(true);
		}

		public bool StopAndDelete()
		{
			log.Info("StopAndDelete invoked from another process.");
			return controller.StopAndDelete();
		}

		public void ChangeName(string name)
		{
			log.Info("ChangeName invoked from another process. Name: " + name);
			controller.RecordingName = name;
		}
	}
}
