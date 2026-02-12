using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using JcSend.InterProcessReference;

namespace JcSend
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string [] arguments)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (arguments.Length > 0)
			{
				try
				{
					if (arguments[0] == InterProcessCommand.AddProjectAndWorkByRule.ToString() && arguments.Length == 5)
					{
						var projectKey = arguments[1];
						var workName = arguments[2];
						var workKey = arguments[3];
						int ruleId;
						if (int.TryParse(arguments[4], out ruleId))
							using (var client = new InterProcessClient())
								client.AddProjectAndWorkByRule(projectKey, workName, workKey, ruleId);
						else
							MessageBox.Show(Messages.ErrorInvalidRuleIdInvalidNumber, Messages.ErrorTitle);
						return;
					}
					if (arguments[0] == InterProcessCommand.StartWork.ToString() && arguments.Length == 2)
					{
						int workId;
						if (int.TryParse(arguments[1], out workId))
							using (var client = new InterProcessClient()) client.StartWork(workId);
						else
							MessageBox.Show(Messages.ErrorTaskIdInvalidNumber, Messages.ErrorTitle);
						return;
					}
					if (arguments[0] == InterProcessCommand.StopWork.ToString() && arguments.Length == 1)
					{
						using (var client = new InterProcessClient()) client.StopWork();
						return;
					}
					if (arguments[0] == InterProcessCommand.SwitchWork.ToString() && arguments.Length == 2)
					{
						int workId;
						if (int.TryParse(arguments[1], out workId))
							using (var client = new InterProcessClient()) client.SwitchWork(workId);
						else
							MessageBox.Show(Messages.ErrorTaskIdInvalidNumber, Messages.ErrorTitle);
						return;
					}
					if (arguments[0] == InterProcessCommand.AddExtText.ToString() && arguments.Length == 2)
					{
						var text = arguments[1];
						using (var client = new InterProcessClient()) client.AddExtText(text);
						return;
					}
				}
				catch (EndpointNotFoundException)
				{
					MessageBox.Show(Messages.ErrorApplicationUnreachable, Messages.ErrorTitle);
					return;
				}
				catch (Exception)
				{
					MessageBox.Show(Messages.ErrorUnknownError, Messages.ErrorTitle);
					return;
				}
			}
			var imageName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			MessageBox.Show(
				  imageName + " " + InterProcessCommand.StartWork + " <task id>" + Console.Out.NewLine
				+ imageName + " " + InterProcessCommand.StopWork + Console.Out.NewLine
				+ imageName + " " + InterProcessCommand.SwitchWork + " <task id>" + Console.Out.NewLine
				+ imageName + " " + InterProcessCommand.AddExtText + " <pattern>" + Console.Out.NewLine
				// + imageName + " " + InterProcessCommand.AddProjectAndWorkByRule + " <projectKey> <workName> <key> <rule id>" + Console.Out.NewLine
				, Messages.UsageTitle);
		}
	}
}
