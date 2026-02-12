using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Ocr.Optimization
{
	public class SimulationParameters<T>
	{
		public SwarmParameters SwarmParameters { get; set; }
		public DenseVector UpperBound { get; set; }
		public DenseVector LowerBound { get; set; }
		public Func<DenseVector, T> ProcessFunction { get; set; }
		public Func<T, double> EvaluateFunction { get; set; }
	}
}