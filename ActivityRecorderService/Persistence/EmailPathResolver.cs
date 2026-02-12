using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;

namespace Tct.ActivityRecorderService.Persistence
{
	/// <summary>
	/// Thread-safe class for resolving paths for EmailMessages
	/// </summary>
	public class EmailPathResolver : IPathResolver<EmailMessage>
	{
		private static readonly HashSet<char> invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());

		private readonly string rootDir;

		public EmailPathResolver(string rootDir)
		{
			this.rootDir = rootDir;
		}

		public string GetFilePath(EmailMessage message)
		{
			return string.Concat(
				(message.To + "-" + message.Subject)
				.Where(n => !invalidChars.Contains(n))
				.Take(25)) //avoid too long file names
				+ "_" + message.Id.ToString("N")
				+ ".email";
		}

		public string GetFilePath(object obj)
		{
			return GetFilePath(obj as EmailMessage);
		}

		public string GetRootDir()
		{
			return rootDir;
		}

		public string GetSearchPattern()
		{
			return "*.email";
		}
	}
}
