using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.Persistence;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class EmailPersistenceTests : IDisposable
	{
		private readonly EmailPathResolver pathResolver = new EmailPathResolver("C:\\temp\\emailTests");
		private readonly EmailMessage msg = new EmailMessage()
		{
			To = "z@test.hu",
			Subject = "test email" + string.Concat(Path.GetInvalidFileNameChars()),
			PlainBody = "restsadasd",
			HtmlBody = "<s><s><s>",
			HtmlResources = new List<EmailResource>() 
				{ 
					new EmailResource() 
					{ 
						ContentId = Guid.NewGuid().ToString(),
						Data = new  byte[] {34,24,124}, 
						MediaType = "image/png",
					},
				}
		};

		private readonly EmailMessage msg2 = new EmailMessage()
		{
			To = "z@te32st.hu" + new string('L', 1000),
			Subject = "tesewrdsft email" + string.Concat(Path.GetInvalidFileNameChars()),
			PlainBody = "restsdsfdsfadasd",
			HtmlBody = "<s><sdsf><s>",
			HtmlResources = new List<EmailResource>() 
				{ 
					new EmailResource() 
					{ 
						ContentId = Guid.NewGuid().ToString(),
						Data = new  byte[] {34,24,24}, 
						MediaType = "image/bmp",
					},
				}
		};

		private void CleanUp()
		{
			if (Directory.Exists(pathResolver.GetRootDir()))
			{
				Directory.Delete(pathResolver.GetRootDir(), true);
			}
		}

		public EmailPersistenceTests()
		{
			CleanUp();
		}

		[Fact]
		public void CanSerializeEmail()
		{
			long size;
			PersistenceHelper.Save(pathResolver, msg, out size);
			var fileName = Path.Combine(pathResolver.GetRootDir(), pathResolver.GetFilePath(msg));
			Assert.True(File.Exists(fileName));
			Assert.True(size > 0);
			Assert.Equal(size, new FileInfo(fileName).Length);
			EmailMessage loadedMsg;
			PersistenceHelper.Load(fileName, out loadedMsg);
			Assert.Equal(msg.Id, loadedMsg.Id);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg, loadedMsg);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg.HtmlResources[0], loadedMsg.HtmlResources[0]);
			Assert.True(Enumerable.SequenceEqual(msg.HtmlResources[0].Data, loadedMsg.HtmlResources[0].Data));
		}

		[Fact]
		public void CanSerializeEmailLongName()
		{
			long size;
			PersistenceHelper.Save(pathResolver, msg2, out size);
			var fileName = Path.Combine(pathResolver.GetRootDir(), pathResolver.GetFilePath(msg2));
			Assert.True(File.Exists(fileName));
			Assert.True(size > 0);
			Assert.Equal(size, new FileInfo(fileName).Length);
			EmailMessage loadedMsg;
			PersistenceHelper.Load(fileName, out loadedMsg);
			Assert.Equal(msg2.Id, loadedMsg.Id);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg2, loadedMsg);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg2.HtmlResources[0], loadedMsg.HtmlResources[0]);
			Assert.True(Enumerable.SequenceEqual(msg2.HtmlResources[0].Data, loadedMsg.HtmlResources[0].Data));
		}

		[Fact]
		public void CanDeleteEmail()
		{
			PersistenceHelper.Save(pathResolver, msg);
			var fileName = Path.Combine(pathResolver.GetRootDir(), pathResolver.GetFilePath(msg));
			Assert.True(File.Exists(fileName));
			PersistenceHelper.Delete(pathResolver, msg);
			Assert.False(File.Exists(fileName));
		}

		[Fact]
		public void CanLoadEmails()
		{
			PersistenceHelper.Save(pathResolver, msg);
			PersistenceHelper.Save(pathResolver, msg2);
			var emails = PersistenceHelper.LoadAllFromRootDir(pathResolver).ToList();
			Assert.Equal(2, emails.Count);

			var loadedMsg = emails.Where(n => n.To == msg.To).Single();
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg, loadedMsg);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg.HtmlResources[0], loadedMsg.HtmlResources[0]);
			Assert.True(Enumerable.SequenceEqual(msg.HtmlResources[0].Data, loadedMsg.HtmlResources[0].Data));

			loadedMsg = emails.Where(n => n.To == msg2.To).Single();
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg2, loadedMsg);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(msg2.HtmlResources[0], loadedMsg.HtmlResources[0]);
			Assert.True(Enumerable.SequenceEqual(msg2.HtmlResources[0].Data, loadedMsg.HtmlResources[0].Data));
		}

		public void Dispose()
		{
			CleanUp();
		}
	}
}
