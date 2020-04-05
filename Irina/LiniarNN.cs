using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irina
{
	class LiniarNN
	{
		double k;
		double bias;
		double learningRate;

		public LiniarNN(double learningRate)
		{
			this.learningRate = learningRate;

			var random = new Random();

			k = random.NextDouble();
			bias = 0; // random.NextDouble();
		}

		public void Train(double input, double target)
		{
			var output = Predict(input);
			var error = target - output;
			k = error * learningRate;
		}

		public double Predict(double input)
		{
			return k * input + bias;
		}
	}
}
