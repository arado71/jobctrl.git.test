using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Ocr.Optimization
{
	public partial class ParticleSwarm<T>
	{
		private class SubSwarm
		{
			private readonly SimulationParameters<T> options;
			private readonly List<Particle> particles = new List<Particle>();

			public SubSwarm(SimulationParameters<T> options, SwarmState.SubSwarmState state)
			{
				this.options = options;
				BestFitness = state.BestFitness;
				BestPosition = DenseVector.OfArray(state.BestPosition);
				foreach (var particleState in state.Particles)
					particles.Add(new Particle(options, particleState));
			}

			public SubSwarm(SimulationParameters<T> options)
			{
				this.options = options;
				BestFitness = double.NegativeInfinity;

				for (var i = 0; i < options.SwarmParameters.SwarmParticleCount; i++)
					particles.Add(new Particle(options));
			}

			public DenseVector BestPosition { get; private set; }

			public double BestFitness { get; private set; }

			public void AddParticle(Particle p)
			{
				particles.Add(p);
			}

			public Particle RemoveParticle()
			{
				var p = particles[rand.Next(particles.Count)];
				particles.Remove(p);
				return p;
			}

			public SwarmState.SubSwarmState GetState()
			{
				return new SwarmState.SubSwarmState
				{
					BestFitness = BestFitness,
					BestPosition = BestPosition.ToArray(),
					Particles = particles.Select(x => x.GetState()).ToArray()
				};
			}

			public void Update(DenseVector globalBest)
			{
				foreach (var particle in particles)
					particle.Update(BestPosition, globalBest);
			}

			public T Evaluate()
			{
				var newBest = (T) null;

				foreach (var particle in particles)
				{
					var candidate = particle.Evaluate();
					if (candidate != null)
						if (particle.BestFitness > BestFitness)
						{
							BestPosition = particle.BestPosition;
							BestFitness = particle.BestFitness;
							newBest = candidate;
						}
				}

				return newBest;
			}
		}
	}
}