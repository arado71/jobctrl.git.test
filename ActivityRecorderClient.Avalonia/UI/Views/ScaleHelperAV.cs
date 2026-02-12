using System;

namespace ActivityRecorderClientAV
{
	public static class ScaleHelperAV
	{
		public static event Action? ScaleChanged;

		private static double _globalWindowScale;
		public static double GlobalWindowScale
		{
			get => _globalWindowScale;
			set
			{
				if (_globalWindowScale != value)
				{
					_globalWindowScale = value;
					ScaleChanged?.Invoke();
				}
			}
		}

		// Initialize with the current scale
		static ScaleHelperAV()
		{
			_globalWindowScale = 1.0; // Default scale
		}
	}
}