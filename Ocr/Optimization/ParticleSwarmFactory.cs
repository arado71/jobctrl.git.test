using System;
using System.Diagnostics;
using log4net;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Ocr.Optimization
{
	public static class ParticleSwarmFactory
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ParticleSwarmFactory));

		public static ParticleSwarm<T> Minimize<T>(Func<DenseVector, T> process, Func<T, double> evaluate,
			DenseVector lowerBound, DenseVector upperBound, SwarmParameters parameters = null) where T : class
		{
			return Create(process, evaluate, x => -x, lowerBound, upperBound, parameters ?? new SwarmParameters());
		}

		public static ParticleSwarm<T> Minimize<T>(Func<DenseVector, T> process, Func<T, double> evaluate, SwarmState state,
			SwarmParameters parameters = null) where T : class
		{
			return Create(process, evaluate, x => -x, state, parameters ?? new SwarmParameters());
		}

		public static ParticleSwarm<T> Maximize<T>(Func<DenseVector, T> process, Func<T, double> evaluate,
			DenseVector lowerBound, DenseVector upperBound, SwarmParameters parameters = null) where T : class
		{
			return Create(process, evaluate, lowerBound, upperBound, parameters ?? new SwarmParameters());
		}

		public static ParticleSwarm<T> Maximize<T>(Func<DenseVector, T> process, Func<T, double> evaluate, SwarmState state,
			SwarmParameters parameters = null) where T : class
		{
			return Create(process, evaluate, state, parameters ?? new SwarmParameters());
		}

		public static ParticleSwarm<T> Seek<T>(Func<DenseVector, T> process, Func<T, double> evaluate, double targetValue,
			DenseVector lowerBound, DenseVector upperBound, SwarmParameters parameters = null) where T : class
		{
			return Create(process, evaluate, x => (targetValue - x) * (x - targetValue), lowerBound, upperBound,
				parameters ?? new SwarmParameters());
		}

		public static ParticleSwarm<T> Seek<T>(Func<DenseVector, T> process, Func<T, double> evaluate, double targetValue,
			SwarmState swarm, SwarmParameters parameters = null) where T : class
		{
			return Create(process, evaluate, x => (targetValue - x) * (x - targetValue), swarm,
				parameters ?? new SwarmParameters());
		}

		private static ParticleSwarm<T> Create<T>(Func<DenseVector, T> process, Func<T, double> evaluate,
			Func<double, double> fitnessTransform, SwarmState state, SwarmParameters parameters) where T : class
		{
			return Create(process, x => fitnessTransform(evaluate(x)), state, parameters);
		}

		private static ParticleSwarm<T> Create<T>(Func<DenseVector, T> process, Func<T, double> evaluate,
			Func<double, double> fitnessTransform, DenseVector lowerBound, DenseVector upperBound, SwarmParameters parameters)
			where T : class
		{
			return Create(process, x => fitnessTransform(evaluate(x)), lowerBound, upperBound, parameters);
		}

		private static ParticleSwarm<T> Create<T>(Func<DenseVector, T> process, Func<T, double> evaluate,
			SwarmState state, SwarmParameters parameters) where T : class
		{
			if (state.LowerBound.Length != state.UpperBound.Length)
				throw new ArgumentException("Constraint dimensions don't match");
			return new ParticleSwarm<T>(new SimulationParameters<T>
			{
				EvaluateFunction = evaluate,
				LowerBound = DenseVector.OfArray(state.LowerBound),
				UpperBound = DenseVector.OfArray(state.UpperBound),
				ProcessFunction = process,
				SwarmParameters = parameters
			}, state);
		}

		private static ParticleSwarm<T> Create<T>(Func<DenseVector, T> process, Func<T, double> evaluate,
			DenseVector lowerBound, DenseVector upperBound, SwarmParameters parameters) where T : class
		{
			Debug.Assert(parameters != null);
			if (lowerBound.Count != upperBound.Count) throw new ArgumentException("Constraint dimensions don't match");
			return new ParticleSwarm<T>(new SimulationParameters<T>
			{
				EvaluateFunction = evaluate,
				LowerBound = lowerBound,
				UpperBound = upperBound,
				ProcessFunction = process,
				SwarmParameters = parameters
			});
		}
	}
}