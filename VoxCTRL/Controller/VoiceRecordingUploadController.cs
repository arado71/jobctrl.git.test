using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using VoxCTRL.ActivityRecorderServiceReference;
using VoxCTRL.Communication;

namespace VoxCTRL.Controller
{
	public class VoiceRecordingUploadController : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int retryTimeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;

		private readonly SynchronizationContext context;
		private readonly Queue<VoiceRecording> itemsToUpload = new Queue<VoiceRecording>();
		private readonly Dictionary<Guid, List<VoiceRecording>> skippedItems = new Dictionary<Guid, List<VoiceRecording>>();
		private ActivityRecorderClientWrapper client;
		private bool isDisposed;
		private bool isUploading;
		private int numSendItems;

		public event EventHandler<SingleValueEventArgs<VoiceRecording>> VoiceRecordingUploaded;

		public VoiceRecordingUploadController()
		{
			context = SynchronizationContext.Current;
			Debug.Assert(context is System.Windows.Forms.WindowsFormsSynchronizationContext);
		}

		public void UploadRecordingAsync(VoiceRecording voiceRecording)
		{
			if (voiceRecording == null) return;
			List<VoiceRecording> skipItems;
			if (skippedItems.TryGetValue(voiceRecording.ClientId, out skipItems)) //if we are currently skipping this guid
			{
				skipItems.Add(voiceRecording);
				numSendItems++;
			}
			else
			{
				itemsToUpload.Enqueue(voiceRecording);
				if (!isUploading) UploadNextAsync();
				EnsureOrder();
			}
		}

		private void UploadNextAsync()
		{
			if (isDisposed) return;
			isUploading = (itemsToUpload.Count != 0);
			if (!isUploading) return;
			var curr = itemsToUpload.Dequeue();
			numSendItems++;
			if (curr.IsMarkedForDelete)
			{
				GetClient().DeleteVoiceRecordingAsync(curr, curr);
			}
			else
			{
				GetClient().UpsertVoiceRecordingAsync(curr, curr);
			}
		}

		[Conditional("DEBUG")]
		private void EnsureOrder()
		{
			Debug.Assert(itemsToUpload.OrderBy(n => n.ClientId).SequenceEqual(itemsToUpload.OrderBy(n => n.ClientId).ThenBy(n => n.Offset)));
		}

		private VoiceRecorderClient GetClient()
		{
			Debug.Assert(!isDisposed);
			if (client != null && client.Client.State == System.ServiceModel.CommunicationState.Faulted)
			{
				client.Dispose();
				client = null;
			}
			if (client == null)
			{
				client = new ActivityRecorderClientWrapper();
				client.Client.UpsertVoiceRecordingCompleted += UploadVoiceRecordingCompleted;
				client.Client.DeleteVoiceRecordingCompleted += UploadVoiceRecordingCompleted;
			}
			return client.Client;
		}

		private void UploadVoiceRecordingCompleted(object sender, AsyncCompletedEventArgs e)
		{
			var rec = (VoiceRecording)e.UserState;
			if (e.Error != null)
			{
				log.Error("Cannot upload voice data " + rec, e.Error);
				List<VoiceRecording> skipItems;
				if (!skippedItems.TryGetValue(rec.ClientId, out skipItems))
				{
					skipItems = new List<VoiceRecording>();
					skippedItems.Add(rec.ClientId, skipItems);
				}
				else //this should never happen
				{
					log.ErrorAndFail("Voice data already in skippedItems " + rec);
				}
				skipItems.Add(rec);
				int count = itemsToUpload.Count;
				for (int i = 0; i < count; i++)
				{
					var curr = itemsToUpload.Dequeue();
					if (curr.ClientId == rec.ClientId)
					{
						log.Warn("Skipped uploading voice data " + curr);
						skipItems.Add(curr);
						numSendItems++;
					}
					else
					{
						itemsToUpload.Enqueue(curr);
					}
				}

				var timer = new Timer(self =>
				{
					try
					{
						context.Post(_ =>
						{
							skippedItems.Remove(rec.ClientId); //it is crucial to remove this first so items will go into the itemsToUpload queue
							foreach (var skipItem in skipItems)
							{
								Debug.Assert(skipItem.ClientId == rec.ClientId);
								UploadRecordingAsync(skipItem);
								numSendItems--;
							}
						}, null);
					}
					finally
					{
						((Timer)self).Dispose();
					}
				});
				timer.Change(retryTimeout, Timeout.Infinite);
				if (AuthenticationHelper.IsInvalidUserOrPasswordException(e.Error)) //todo rewrite hax hax hax
				{
					if (!invalidPass)
					{
						System.Windows.Forms.MessageBox.Show("Indítsa újra az alkalmazást", "Hibás jelszó");
						LoginData.DeleteFromDisk();
						invalidPass = true;
					}
				}
			}
			else
			{
				numSendItems--;
				OnVoiceRecordingUploaded(rec);
			}
			UploadNextAsync();
			log.Debug("Number of items to upload " + (numSendItems + itemsToUpload.Count));
		}

		private bool invalidPass;

		private void OnVoiceRecordingUploaded(VoiceRecording voiceRecording)
		{
			var del = VoiceRecordingUploaded;
			if (del != null) del(this, SingleValueEventArgs.Create(voiceRecording));
		}

		public void Dispose()
		{
			isDisposed = true;
			if (client != null)
			{
				client.Dispose();
				client = null;
			}
			log.Debug("Disposed");
		}
	}
}
