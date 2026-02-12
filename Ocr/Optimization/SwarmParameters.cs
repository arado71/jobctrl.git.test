namespace Ocr.Optimization
{
	public class SwarmParameters
	{
		public SwarmParameters()
		{
			SwarmCount = 5;
			SwarmParticleCount = 5;
			InertiaWeight = 0.729;
			CognitiveWeight = 1.49445;
			SocialWeight = 1.49445;
			GlobalWeight = 0.3645;
			DeathChance = 0.005;
			ImmigrationChance = 0.005;
		}

		public uint SwarmCount { get; set; }

		public uint SwarmParticleCount { get; set; }

		public double InertiaWeight { get; set; }

		public double CognitiveWeight { get; set; }

		public double SocialWeight { get; set; }

		public double DeathChance { get; set; }

		public double ImmigrationChance { get; set; }

		public double GlobalWeight { get; set; }
	}
}