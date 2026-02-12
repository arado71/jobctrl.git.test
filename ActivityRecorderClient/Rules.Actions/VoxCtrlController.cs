using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Rules.Actions
{
	/// <summary>
	/// Class for sending commands to VoxCTRL client and tracking it's status.
	/// </summary>
	public class VoxCtrlController //todo : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int retryTimeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
		private const string RuleActionValue = "actionvalue";

		private readonly SynchronizationContext context;
		private VoiceRecorderControllerClientWrapper client;
		private VoxCtrlState currentState = GetStopState();
		private VoxCtrlState requestedState = GetStopState();
		private bool isRequestInProgress;

		public VoxCtrlController()
		{
			context = SynchronizationContext.Current;
			Debug.Assert(context is System.Windows.Forms.WindowsFormsSynchronizationContext);
		}

		private VoiceRecorderControllerClientWrapper GetClient()
		{
			if (client != null && client.Client.State == System.ServiceModel.CommunicationState.Faulted)
			{
				client.Dispose();
				client = null;
			}
			if (client == null)
			{
				client = new VoiceRecorderControllerClientWrapper();
				client.Client.RecordCompleted += Client_RecordCompleted;
				client.Client.StopCompleted += Client_StopCompleted;
				client.Client.ChangeNameCompleted += Client_ChangeNameCompleted;
			}
			return client;
		}

		private void Client_ChangeNameCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			isRequestInProgress = false;
			var retryLater = false;
			if (e.Error != null)
			{
				log.Warn("Cannot change name", e.Error.InnerException);
				retryLater = true;
			}
			else
			{
				currentState = (VoxCtrlState)e.UserState;
				log.InfoFormat("Changed name, state: {0} requested: {1}", currentState, requestedState);
			}
			ProcessNextNowOrLater(retryLater);
		}

		private void Client_StopCompleted(object sender, VoiceRecorderControllerServiceReference.StopCompletedEventArgs e)
		{
			isRequestInProgress = false;
			var retryLater = false;
			if (e.Error != null)
			{
				log.Warn("Cannot stop recording", e.Error.InnerException);
				retryLater = true;
			}
			else
			{
				currentState = (VoxCtrlState)e.UserState; //we don't care about result, if it not an exception then it should be in stopped state
				log.InfoFormat("Stop recording result: {0} state: {1} requested: {2}", e.Result, currentState, requestedState);
			}
			ProcessNextNowOrLater(retryLater);
		}

		private void Client_RecordCompleted(object sender, VoiceRecorderControllerServiceReference.RecordCompletedEventArgs e)
		{
			isRequestInProgress = false;
			var retryLater = false;
			if (e.Error != null)
			{
				log.Warn("Cannot start recording", e.Error.InnerException);
				retryLater = true;
			}
			else
			{
				if (!e.Result)
				{
					retryLater = true;
				}
				else
				{
					currentState = (VoxCtrlState)e.UserState;
				}
				log.InfoFormat("Start recording result: {0} state: {1} requested: {2}", e.Result, currentState, requestedState);
			}
			ProcessNextNowOrLater(retryLater);
		}

		private void ProcessNextNowOrLater(bool retryLater)
		{
			if (retryLater)
			{
				RetryRequestLater();
			}
			else
			{
				ProcessNextRequest();
			}
		}

		private void RetryRequestLater()
		{
			var timer = new Timer(self =>
			{
				try
				{
					context.Post(_ => ProcessNextRequest(), null);
				}
				finally
				{
					((Timer)self).Dispose();
				}
			});
			timer.Change(retryTimeout, Timeout.Infinite);
		}

		private void EnqueueRequest(VoxCtrlState state)
		{
			if (requestedState == state) return;
			requestedState = state;
			ProcessNextRequest();
		}

		private void ProcessNextRequest()
		{
			if (currentState == requestedState) return; //we've reached out goal
			log.DebugFormat("VoxCTRL current state: {0} requested: {1} inProgress: {2}", currentState, requestedState, isRequestInProgress);
			if (isRequestInProgress) return;
			try
			{
				if (currentState.IsRecording)
				{
					if (requestedState.IsRecording
						&& currentState.RecordingName != requestedState.RecordingName
						&& !requestedState.IsRestartForNewNameRequired)
					{
						GetClient().Client.ChangeNameAsync(requestedState.RecordingName, requestedState);
						isRequestInProgress = true;
					}
					else
					{
						//we either ask for stop or restart recording with a new name
						GetClient().Client.StopAsync(GetStopState()); //we might not reach the requested state with one command
						isRequestInProgress = true;
					}
				}
				else if (requestedState.IsRecording) //!currentState.IsRecording
				{
					GetClient().Client.RecordAsync(requestedState.RecordingName, requestedState);
					isRequestInProgress = true;
				}
				else
				{
					log.ErrorAndFail("Invalid states curr: " + currentState + " req: " + requestedState);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in XXXAsync", ex);
			}
		}

		private void StartRecording(string name)
		{
			EnqueueRequest(new VoxCtrlState() { IsRecording = true, RecordingName = name, IsRestartForNewNameRequired = true });
		}

		private void SetName(string name)
		{
			if (!requestedState.IsRecording) return; //we cannot change name when not recording
			EnqueueRequest(new VoxCtrlState() { IsRecording = true, RecordingName = name, IsRestartForNewNameRequired = false });
		}

		private static VoxCtrlState GetStopState()
		{
			return new VoxCtrlState() { IsRecording = false, RecordingName = null, IsRestartForNewNameRequired = true };
		}

		public void StopRecording()
		{
			EnqueueRequest(GetStopState());
		}

		public void StartRecording(AssignData assignData)
		{
			StartRecording(GetNameFromAssignData(assignData));
		}

		public void SetName(AssignData assignData)
		{
			SetName(GetNameFromAssignData(assignData));
		}

		private static string GetNameFromAssignData(AssignData assignData)
		{
			if (assignData == null) return null;
			if (assignData.Common != null)
			{
				if (assignData.Common.Data.TryGetValue(RuleActionValue, out string value)) return value;
			}
			if (assignData.Work != null) return assignData.Work.WorkKey;
			if (assignData.Project != null) return assignData.Project.ProjectKey;
			if (assignData.Composite != null)
			{
				if (assignData.Composite.ProjectKeys != null && assignData.Composite.ProjectKeys.Count > 0)
				{
					var sb = new StringBuilder();
					foreach (var key in assignData.Composite.ProjectKeys)
					{
						sb.Append(key).Append("-");
					}
					sb.Append(assignData.Composite.WorkKey);
					return sb.ToString();
				}
				return assignData.Composite.WorkKey;
			}
			if (assignData.Common == null)
				log.ErrorAndFail("Unsupported assignData");
			return null;
		}

		private sealed class VoxCtrlState : IEquatable<VoxCtrlState>
		{
			public bool IsRecording { get; set; }
			public string RecordingName { get; set; }
			public bool IsRestartForNewNameRequired { get; set; }

			public bool Equals(VoxCtrlState other)
			{
				if (other == null) return false;
				return IsRecording == other.IsRecording
					&& IsRestartForNewNameRequired == other.IsRestartForNewNameRequired
					&& RecordingName == other.RecordingName;
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as VoxCtrlState);
			}

			public override int GetHashCode()
			{
				var result = 17;
				result = 31 * result + (RecordingName == null ? 0 : RecordingName.GetHashCode());
				result = 31 * result + IsRecording.GetHashCode();
				result = 31 * result + IsRestartForNewNameRequired.GetHashCode();
				return result;
			}

			public static bool operator ==(VoxCtrlState first, VoxCtrlState second)
			{
				if (ReferenceEquals(first, second)) return true;
				if (ReferenceEquals(first, null))
				{
					return false;
				}
				return first.Equals(second);
			}

			public static bool operator !=(VoxCtrlState first, VoxCtrlState second)
			{
				return !(first == second);
			}

			public override string ToString()
			{
				return "Rec: " + (IsRecording ? "On" : "Off") + " Name: " + RecordingName + " Rest: " + IsRestartForNewNameRequired;
			}
		}
	}
}
