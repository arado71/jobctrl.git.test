using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Ocr.Optimization
{
	public partial class ParticleSwarm<T>
	{
		private class Particle
		{
			private static readonly Random random = new Random();
			private readonly SimulationParameters<T> options;

			public Particle(SimulationParameters<T> options, SwarmState.ParticleState state)
			{
				this.options = options;
				BestPosition = state.BestPosition;
				BestFitness = state.BestFitness;
				Position = DenseVector.OfArray(state.Position);
				Speed = DenseVector.OfArray(state.Speed);
			}

			public Particle(SimulationParameters<T> options)
			{
				this.options = options;
				BestPosition = null;
				BestFitness = double.NegativeInfinity;
				Randomize();
			}

			private DenseVector Position { get; set; }

			private DenseVector Speed { get; set; }

			public DenseVector BestPosition { get; private set; }

			public double BestFitness { get; private set; }

			public SwarmState.ParticleState GetState()
			{
				return new SwarmState.ParticleState
				{
					BestFitness = BestFitness,
					BestPosition = BestPosition.ToArray(),
					Position = Position.ToArray(),
					Speed = Speed.ToArray()
				};
			}

			private void Randomize()
			{
				Debug.Assert(options.LowerBound.Count == options.UpperBound.Count);
				Position = DenseVector.Create(options.LowerBound.Count,
					i => random.NextDouble() * (options.UpperBound[i] - options.LowerBound[i]) + options.LowerBound[i]);
				Speed = DenseVector.Create(options.LowerBound.Count,
					i => (random.NextDouble() * (options.UpperBound[i] - options.LowerBound[i]) + options.LowerBound[i]) / 2.0);
			}

			public T Evaluate()
			{
				var processed = options.ProcessFunction(Position);
				var currentFitness = options.EvaluateFunction(processed);
				if (currentFitness > BestFitness)
				{
					BestFitness = currentFitness;
					BestPosition = Position;
					return processed;
				}

				return null;
			}

			public void Update(DenseVector swarmBest, DenseVector globalBest)
			{
				if (rand.NextDouble() < options.SwarmParameters.DeathChance)
				{
					Randomize();
				}
				else
				{
					Speed = Speed * options.SwarmParameters.InertiaWeight;
					if (BestPosition != null)
						Speed += rand.NextDouble() * options.SwarmParameters.CognitiveWeight * (BestPosition - Position);
					if (swarmBest != null) Speed += rand.NextDouble() * options.SwarmParameters.SocialWeight * (swarmBest - Position);
					if (globalBest != null)
						Speed += rand.NextDouble() * options.SwarmParameters.GlobalWeight * (globalBest - Position);
					Position = Position + Speed;
					for (var i = 0; i < Position.Count; i++)
					{
						if (Position[i] < options.LowerBound[i])
						{
							Position[i] = 2 * options.LowerBound[i] - Position[i];
							Speed[i] = -Speed[i];
							if (Position[i] > options.UpperBound[i])
							{
								Position[i] = options.UpperBound[i];
								Speed[i] = 0;
							}
						}

						if (Position[i] > options.UpperBound[i])
						{
							Position[i] = 2 * options.UpperBound[i] - Position[i];
							Speed[i] = -Speed[i];
							if (Position[i] < options.LowerBound[i])
							{
								Position[i] = options.LowerBound[i];
								Speed[i] = 0;
							}
						}
					}
				}
			}
		}
	}
}