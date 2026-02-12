using System;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Tct.ActivityRecorderClient.View
{
	public class FadeAnimation : IDisposable
	{
		public delegate void AnimationHandler(double progress);

		private const int FrameRate = 40;
		private readonly Timer timer = new Timer();
		private int animationStart;
		private Action animationState = () => { };
		private bool disposed;
		private float progress;

		public event AnimationHandler Animate;
		public event EventHandler FadeInComplete;
		public event EventHandler FadeOutComplete;

		public TimeSpan FadeInDuration { get; set; }
		public TimeSpan FadeOutDuration { get; set; }
		public bool Playing { get; private set; }
		public bool IsDisposed { get { return disposed; } }

		public FadeAnimation()
		{
			timer.Interval = 1000 / FrameRate;
			timer.Tick += TimerElapsed;
			FadeInDuration = TimeSpan.FromMilliseconds(500);
			FadeOutDuration = TimeSpan.FromMilliseconds(400);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void FadeIn()
		{
			animationState = FadeInState;
			animationStart = Environment.TickCount;
			timer.Start();
			Playing = true;
		}

		public void FadeOut()
		{
			animationState = FadeOutState;
			animationStart = (int)(Environment.TickCount - (FadeOutDuration.TotalMilliseconds * (1.0 - progress)));
			timer.Start();
			Playing = true;
		}

		public static bool IsEnabled()
		{
			return !ConfigManager.EnvironmentInfo.IsRemoteDesktop;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				timer.Dispose();
			}

			disposed = true;
		}

		protected void RaiseFadeInComplete()
		{
			EventHandler del = FadeInComplete;
			if (del != null) del(this, EventArgs.Empty);
		}

		protected void RaiseFadeOutComplete()
		{
			EventHandler del = FadeOutComplete;
			if (del != null) del(this, EventArgs.Empty);
		}

		private void TimerElapsed(object sender, EventArgs e)
		{
			animationState();
		}

		private void FadeInState()
		{
			progress = Math.Min(1.0F, (Environment.TickCount - animationStart) / (float)FadeInDuration.TotalMilliseconds);
			AnimationHandler del = Animate;
			if (del != null) del(progress);
			if (progress == 1)
			{
				Stop();
				RaiseFadeInComplete();
			}
		}

		private void FadeOutState()
		{
			progress = Math.Max(1 - (Environment.TickCount - animationStart) / (float)FadeOutDuration.TotalMilliseconds, 0.0F);
			AnimationHandler del = Animate;
			if (del != null) del(progress);
			if (progress == 0)
			{
				Stop();
				RaiseFadeOutComplete();
			}
		}

		public void Stop()
		{
			timer.Stop();
			animationState = () => { };
			Playing = false;
		}
	}
}