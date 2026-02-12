using System.Runtime.Serialization;

namespace Ocr.Optimization
{
	[DataContract]
	public class SwarmState
	{
		[DataMember]
		public double[] UpperBound { get; set; }

		[DataMember]
		public double[] LowerBound { get; set; }

		[DataMember]
		public int Iteration { get; set; }

		[DataMember]
		public double[] BestPosition { get; set; }

		[DataMember]
		public SubSwarmState[] SubSwarms { get; set; }

		[DataContract]
		public struct SubSwarmState
		{
			[DataMember]
			public double BestFitness { get; set; }

			[DataMember]
			public double[] BestPosition { get; set; }

			[DataMember]
			public ParticleState[] Particles { get; set; }
		}

		[DataContract]
		public struct ParticleState
		{
			[DataMember]
			public double BestFitness { get; set; }

			[DataMember]
			public double[] BestPosition { get; set; }

			[DataMember]
			public double[] Position { get; set; }

			[DataMember]
			public double[] Speed { get; set; }
		}
	}
}