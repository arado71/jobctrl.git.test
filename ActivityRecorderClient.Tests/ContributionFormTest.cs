using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class ContributionFormTest
	{
		[Fact]
		public void WindowTest()
		{
			using (var form = new ContributionForm(new mockup().GetData()))
			{
				form.ShowInTaskbar = false;
				form.ShowDialog();
			}
		}
		class mockup
		{
			private List<ContributionItem> data;
			public List<ContributionItem> GetData()
			{
				var path = @"c:\Work\jobCTRL_client\ActivityRecorderClient.Tests\bin\Debug\";
				return new List<ContributionItem>
			{
				new ContributionItem
				{
					Image = Image.FromFile(path + "sample.bmp")
				},
				new ContributionItem
				{
					Image = Image.FromFile(path + "sample0.bmp")
				},
				new ContributionItem
				{
					Image = Image.FromFile(path + "sample2.png")
				},
				new ContributionItem
				{
					Image = Image.FromFile(path + "sample3.png")
				},
				new ContributionItem
				{
					Image = Image.FromFile(path + "sample4.png")
				},
				//new ContributionItem
				//{
				//	Image = Image.FromFile(path + "sample5.png")
				//}
			};
			}
		}
	}
}
