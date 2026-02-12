using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Ocr.Optimization
{
	public partial class ParticleSwarm<T> where T : class
	{
		private static readonly Random rand = new Random();
		private readonly SimulationParameters<T> parameters;
		private readonly List<SubSwarm> swarms = new List<SubSwarm>();

		public ParticleSwarm(SimulationParameters<T> parameters, SwarmState state)
		{
			this.parameters = parameters;
			BestPosition = DenseVector.OfArray(state.BestPosition);
			Iterations = state.Iteration;
			Best = parameters.ProcessFunction(BestPosition);
			BestFitness = parameters.EvaluateFunction(Best);
			foreach (var subSwarm in state.SubSwarms)
				swarms.Add(new SubSwarm(parameters, subSwarm));
		}

		public ParticleSwarm(SimulationParameters<T> parameters)
		{
			this.parameters = parameters;
			BestFitness = double.NegativeInfinity;
			BestPosition = null;
			Iterations = 0;
			for (var i = 0; i < parameters.SwarmParameters.SwarmCount; i++)
				swarms.Add(new SubSwarm(parameters));
		}

		public DenseVector BestPosition { get; protected set; }
		public T Best { get; protected set; }
		public double BestFitness { get; protected set; }
		public int Iterations { get; protected set; }

		public SwarmState GetState()
		{
			return new SwarmState
			{
				BestPosition = BestPosition.ToArray(),
				Iteration = Iterations,
				LowerBound = parameters.LowerBound.ToArray(),
				UpperBound = parameters.UpperBound.ToArray(),
				SubSwarms = swarms.Select(x => x.GetState()).ToArray()
			};
		}

		public bool Hint(DenseVector position)
		{
			var processed = parameters.ProcessFunction(position);
			var hintFitness = parameters.EvaluateFunction(processed);
			if (hintFitness <= BestFitness) return false;
			BestPosition = position;
			BestFitness = hintFitness;
			Best = processed;
			return true;
		}

		public DenseVector Iterate(long iterationCount)
		{
			for (long i = 0; i < iterationCount; i++)
				Iterate();

			return BestPosition;
		}

		public bool Iterate()
		{
			Iterations++;
			var newBest = false;
			if (rand.NextDouble() < parameters.SwarmParameters.ImmigrationChance)
			{
				var sw1 = rand.Next(swarms.Count);
				var sw2 = (sw1 + rand.Next(1, swarms.Count)) % swarms.Count;
				var p1 = swarms[sw1].RemoveParticle();
				var p2 = swarms[sw2].RemoveParticle();
				swarms[sw1].AddParticle(p2);
				swarms[sw2].AddParticle(p1);
			}

			foreach (var swarm in swarms)
			{
				var candidate = swarm.Evaluate();
				if (candidate != null)
					if (swarm.BestFitness > BestFitness)
					{
						BestPosition = swarm.BestPosition;
						BestFitness = swarm.BestFitness;
						Best = candidate;
						newBest = true;
					}
			}

			foreach (var swarm in swarms)
				swarm.Update(BestPosition);

			return newBest;
		}
	}
}