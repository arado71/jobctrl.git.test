using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Controls;
using Tct.ActivityRecorderClient.Screenshots;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ScreenshotAnalyzerSettingsForm : FixedMetroForm, ILocalizableControl
	{
		private static readonly Dictionary<string, SettingConfiguration> settingConfigurations = new Dictionary<string, SettingConfiguration>()
		{
			{ nameof(ScreenshotAnalyzerConfigs.BackgroundColorPercent), new SettingConfiguration { DisplayName = "", MinimumValue = 1, MaximumValue = 100, DefaultValue = 2 } },
			{ nameof(ScreenshotAnalyzerConfigs.SimilarColorDistanceP2), new SettingConfiguration { DisplayName = "", MinimumValue = 10, MaximumValue = 1000, DefaultValue = 200 } },
			{ nameof(ScreenshotAnalyzerConfigs.FillAreaRatio), new SettingConfiguration { DisplayName = "", MinimumValue = 1, MaximumValue = 100, DefaultValue = 60 } },
			{ nameof(ScreenshotAnalyzerConfigs.SizeMinPixels), new SettingConfiguration { DisplayName = "", MinimumValue = 10, MaximumValue = 500, DefaultValue = 100 } },
			{ nameof(ScreenshotAnalyzerConfigs.StepPixels), new SettingConfiguration { DisplayName = "", MinimumValue = 1, MaximumValue = 50, DefaultValue = 20 } },
			{ nameof(ScreenshotAnalyzerConfigs.DetColorStepPixels), new SettingConfiguration { DisplayName = "", MinimumValue = 1, MaximumValue = 50, DefaultValue = 20 } },
			{ nameof(ScreenshotAnalyzerConfigs.AspectRatioLimit), new SettingConfiguration { DisplayName = "", MinimumValue = 1, MaximumValue = 100, DefaultValue = 5 } },
			{ nameof(ScreenshotAnalyzerConfigs.IndividualColorsLimit), new SettingConfiguration { DisplayName = "", MinimumValue = 1, MaximumValue = 500, DefaultValue = 50 } },
		};

		private ScreenshotAnalyzerConfigs configs;

		public ScreenshotAnalyzerSettingsForm()
		{
			InitializeComponent();
			LoadConfigSliders();
		}

		public ScreenshotAnalyzerConfigs Configs
		{
			get => configs;
			set
			{
				configs = value;
				for (var i = 0; i < tableLayoutPanel1.RowCount - 1; i++)
				{
					var control = tableLayoutPanel1.GetControlFromPosition(1, i) as MetroTrackBar;
					Debug.Assert(control != null, "Not a MetroTrackBar");
					var fieldInfo = control.Tag as FieldInfo;
					control.Value = (int)fieldInfo.GetValue(configs);
				}
			}
		}

		private void LoadConfigSliders()
		{
			foreach (var fieldInfo in typeof(ScreenshotAnalyzerConfigs).GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				Console.WriteLine(fieldInfo.Name);
				var settingConf = settingConfigurations[fieldInfo.Name];
				var rowIdx = tableLayoutPanel1.RowCount - 1;
				tableLayoutPanel1.RowCount++;
				tableLayoutPanel1.RowStyles.Insert(rowIdx, new RowStyle(SizeType.AutoSize));
				tableLayoutPanel1.Controls.Add(new MetroLabel() { Text = string.IsNullOrEmpty(settingConf.DisplayName) ? fieldInfo.Name : settingConf.DisplayName, TextAlign = ContentAlignment.MiddleLeft, AutoSize = true, Dock = DockStyle.Fill }, 0, rowIdx);
				var trackBar = new CustomMetroTrackBar() { Dock = DockStyle.Fill, Minimum = settingConf.MinimumValue, Maximum = settingConf.MaximumValue, Value = settingConf.DefaultValue, Tag = fieldInfo };
				trackBar.SlidingChanged += TrackBarSlidingChanged;
				trackBar.ValueChanged += TrackBarValueChanged;
				tableLayoutPanel1.Controls.Add(trackBar, 1, rowIdx);
				tableLayoutPanel1.Controls.Add(new MetroLabel() { Text = settingConf.DefaultValue.ToString(), TextAlign = ContentAlignment.MiddleCenter, AutoSize = true, Dock = DockStyle.Fill }, 2, rowIdx);
				Height += trackBar.Height;
				tableLayoutPanel1.Height += trackBar.Height;
			}

		}

		public void Localize()
		{
			throw new NotImplementedException();
		}

		public event EventHandler<bool> SlidingChanged;

		public string WorkTimeStatText
		{
			get => lblWorkTimeStat.Text;
			set => lblWorkTimeStat.Text = value;
		}

		private void TrackBarValueChanged(object sender, EventArgs e)
		{
			if (configs == null) return;
			var control = sender as MetroTrackBar;
			Debug.Assert(control != null, "Not a MetroTrackBar");
			var fieldInfo = control.Tag as FieldInfo;
			fieldInfo.SetValue(configs, control.Value);
			var label = tableLayoutPanel1.GetControlFromPosition(2, tableLayoutPanel1.GetRow(control)) as MetroLabel;
			label.Text = control.Value.ToString();
		}

		private void TrackBarSlidingChanged(object sender, bool e)
		{
			SlidingChanged?.Invoke(this, e);
		}

		private class SettingConfiguration
		{
			public string DisplayName { get; set; }
			public int MinimumValue { get; set; }
			public int MaximumValue { get; set; }
			public int DefaultValue { get; set; }
		}
	}

}
