using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;
using Tct.ActivityRecorderClient.Serialization;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class MailCapturesMappingTests
	{
		[Fact]
		public void MappingTest()
		{
			var outlookMailCaptures = new MailCaptures()
			{
				MailCaptureByHWnd = new Dictionary<int, MailCapture>()
				{
					{
						1, new MailCapture()
						{
							Subject = "Teszt",
							From = new MailAddress() { Name = "A", Email = "a@tct.hu"},
							To = new List<MailAddress>()
							{
								new MailAddress() { Name = "B", Email = "b@tct.hu"},
								new MailAddress() { Name = "C", Email = "c@tct.hu"},
							},
							Cc = new List<MailAddress>()
							{
								new MailAddress() { Name = "D", Email = "d@tct.hu"},
								new MailAddress() { Name = "E", Email = "e@tct.hu"},
							}
						}
					}
				}
			};

			var serializedData = JsonHelper.SerializeData(outlookMailCaptures);
			Tct.ActivityRecorderClient.LotusNotesMeetingCaptureServiceReference.MailCaptures lotusNotesMailCaptures;
			JsonHelper.DeserializeData(serializedData, out lotusNotesMailCaptures);

			Assert.Equal(1, lotusNotesMailCaptures.MailCaptureByHWnd.Count);
			Assert.Equal(2, lotusNotesMailCaptures.MailCaptureByHWnd[1].To.Count);
			Assert.Equal(2, lotusNotesMailCaptures.MailCaptureByHWnd[1].Cc.Count);
			Assert.Equal("Teszt", lotusNotesMailCaptures.MailCaptureByHWnd[1].Subject);
			Assert.Equal("A", lotusNotesMailCaptures.MailCaptureByHWnd[1].From.Name);
			Assert.Equal("a@tct.hu", lotusNotesMailCaptures.MailCaptureByHWnd[1].From.Email);
			Assert.Equal("B", lotusNotesMailCaptures.MailCaptureByHWnd[1].To[0].Name);
			Assert.Equal("b@tct.hu", lotusNotesMailCaptures.MailCaptureByHWnd[1].To[0].Email);
			Assert.Equal("C", lotusNotesMailCaptures.MailCaptureByHWnd[1].To[1].Name);
			Assert.Equal("c@tct.hu", lotusNotesMailCaptures.MailCaptureByHWnd[1].To[1].Email);
			Assert.Equal("D", lotusNotesMailCaptures.MailCaptureByHWnd[1].Cc[0].Name);
			Assert.Equal("d@tct.hu", lotusNotesMailCaptures.MailCaptureByHWnd[1].Cc[0].Email);
			Assert.Equal("E", lotusNotesMailCaptures.MailCaptureByHWnd[1].Cc[1].Name);
			Assert.Equal("e@tct.hu", lotusNotesMailCaptures.MailCaptureByHWnd[1].Cc[1].Email);
		}
	}
}
