using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using VoxCTRL.View;

namespace VoxCTRL.Voice
{
	public class AutoGainController
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static TimeSpan controlTimeInterval = TimeSpan.FromSeconds(5);
		private static TimeSpan historyLength = TimeSpan.FromMinutes(15);
		private const double noiseCutLevel = 0.15;
		private readonly RecorderForm recForm;
		private readonly List<double> samples = new List<double>(1000);
		private const int reqSamplesNum = 50;								//5sec
		private const double maxLevelMultiplier = 0.8;
		private const int reqLoudSamples = 10;
		private const double targetVoiceMinVolume = 0.4;
		private const double targetVoiceMaxVolume = 0.7;
		private const int decWindow = 25;
		private const int incWindow = 50;
		private const int volumeStep = 2;


		public AutoGainController(RecorderForm recForm)
		{
			this.recForm = recForm;
		}

		public void Clear()
		{
			samples.Clear();
		}

		public void AddSample(double sample)
		{
			samples.Add(sample);
			if (samples.Count > 1000)
			{
				samples.RemoveAt(0);
			}
			CalculateVolume();
		}

		private void CalculateVolume()
		{
			//ver1
			//var validSamples = samples.Where(IsSampleValid).ToArray();
			//if (validSamples.Any())
			//{
			//	var avg = validSamples.Average();
			//	log.Debug("Average: " + avg);
			//}

			//ver2
			//if (samples.Count < reqSamplesNum) { return; }
			//var ordered = samples.OrderByDescending(s => s);
			//var loudLevel = samples.Max() * maxLevelMultiplier;
			//var loudSamples = samples.Where(w => w > loudLevel);
			//if (loudSamples.Count() > reqLoudSamples)
			//{
			//	var voiceLevel = loudSamples.Average();
			//	log.Debug("loud: " + voiceLevel);
			//	if (voiceLevel < targetVoiceMinVolume)
			//	{
			//		//increase volume
			//		var currentVolume = recForm.GetVolume();
			//		//var diff = Math.Max(currentVolume);
			//		recForm.SetVolume(++currentVolume);
			//		log.DebugFormat("Volume increased to {0}", currentVolume);
			//	}
			//	else if (voiceLevel > targetVoiceMaxVolume)
			//	{
			//		var currentVolume = recForm.GetVolume();
			//		recForm.SetVolume(--currentVolume);
			//		log.DebugFormat("Volume decreased to {0}", currentVolume);
			//	}
			//}

			if (samples.Count < reqSamplesNum) { return; }
			if (samples.Count > decWindow && samples.GetRange(samples.Count - decWindow, decWindow).Average() > targetVoiceMaxVolume)
			{
				ChangeVolume(false);
			}
			else if (samples.Count > incWindow && samples.GetRange(samples.Count - incWindow, incWindow).Max() < targetVoiceMinVolume)
			{
				ChangeVolume(true);
			}

		}

		private void ChangeVolume(bool increase)
		{
			var currentVolume = recForm.GetVolume();
			currentVolume = increase ? currentVolume + volumeStep : currentVolume - volumeStep;
			recForm.SetVolume(currentVolume);
			log.DebugFormat("Volume changed to {0}", currentVolume);
			samples.Clear();
		}

		private static bool IsSampleValid(double sample)
		{
			return sample > noiseCutLevel;
		}
	}
}
