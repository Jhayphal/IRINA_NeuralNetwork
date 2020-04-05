using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;

namespace Irina
{
    [Serializable]
    public class IRINA_NN
    {
        public int LayersCount { get; private set; }

        public int InputLayerNeuronsCount { get; private set; }
        public int OutputLayerNeuronsCount { get; private set; }

        public readonly double LearningRate;

        Matrix<double>[] Neurons;
        Matrix<double>[] Synapses;

        public IRINA_NN(int[] schema, double learningRate)
        {
            if (schema == null)
                throw new ArgumentNullException("Не указано ни одного слоя для создания сети.");

            else if (schema.Length < 2)
                throw new ArgumentOutOfRangeException("Сеть должна иметь хотя бы два слоя.");

            LearningRate = learningRate;

            SetupNetwork(schema);
        }

        void SetupNetwork(int[] schema)
        {
            SetupNeurons(schema);
            SetupSynapses(schema);
        }

        void SetupNeurons(int[] schema)
        {
            LayersCount = schema.Length;

            var inputLayerIndex = 0;
            var outputLayerIndex = LayersCount - 1;

            InputLayerNeuronsCount = schema[inputLayerIndex];
            OutputLayerNeuronsCount = schema[outputLayerIndex];

            Neurons = new DenseMatrix[LayersCount];
        }

        void SetupSynapses(int[] schema)
        {
            int outputLayerIndex = LayersCount - 1;

            Synapses = new DenseMatrix[outputLayerIndex];

            var random = new Random();

            for (int index = 0; index < outputLayerIndex; ++index)
            {
                var fromLayerNeuronsCount = schema[index];
                var toLayerNeuronsCount = schema[index + 1];

                Synapses[index] = DenseMatrix
                    .Build
                    .Dense(toLayerNeuronsCount, fromLayerNeuronsCount)
                    .Map(x => 
                    {
                        var maxWeight = Math.Sqrt(fromLayerNeuronsCount);
                        return random.NextDouble() * maxWeight - maxWeight / 2;
                    });

                //Synapses[index] = DenseMatrix.Build.Random(
                //    toLayerNeuronsCount,
                //    fromLayerNeuronsCount,
                //    new Normal(0.0, Math.Sqrt(fromLayerNeuronsCount))
                //);
            }
        }

        public double[] Predict(double[] input)
        {
            if (input.Length != InputLayerNeuronsCount)
                throw new ArgumentOutOfRangeException("Архитектура сети не подходит для решения этой задачи. Количество входов отличается от количества нейронов входного слоя.");

            var inputLayerIndex = 0;

            Neurons[inputLayerIndex] = DenseMatrix.Build.Dense(InputLayerNeuronsCount, 1);

            for (int index = 0; index < InputLayerNeuronsCount; ++index)
                Neurons[inputLayerIndex][index, 0] = input[index];

            var firstHidenLayerIndex = inputLayerIndex + 1;
            Neurons[firstHidenLayerIndex] = Synapses[inputLayerIndex] * Neurons[inputLayerIndex];

            for (int currentLayerIndex = firstHidenLayerIndex + 1; currentLayerIndex < LayersCount; ++currentLayerIndex)
            {
                var previousLayerIndex = currentLayerIndex - 1;

                Neurons[previousLayerIndex].MapInplace(x => ActivationSigmoid(x));
                Neurons[currentLayerIndex] = Synapses[previousLayerIndex] * Neurons[previousLayerIndex];
            }

            var outputLayerIndex = LayersCount - 1;
            var outputLayer = Neurons[outputLayerIndex];
            outputLayer.MapInplace(x => ActivationSigmoid(x));

            var result = new double[OutputLayerNeuronsCount];

            for (int index = 0; index < OutputLayerNeuronsCount; ++index)
                result[index] = outputLayer[index, 0];

            return result;
        }

        public void Train(double[] input, double[] target)
        {
            var output = Predict(input);

            var error = DenseMatrix.Build.Dense(OutputLayerNeuronsCount, 1);

            for (int index = 0; index < OutputLayerNeuronsCount; ++index)
                error[index, 0] = target[index] - output[index];

            var outputLayer = LayersCount - 1;

            for (int currentLayer = outputLayer; currentLayer > 0; --currentLayer)
            {
                var previousLayer = currentLayer - 1;

                var delta = LearningRate * error.PointwiseMultiply(Neurons[currentLayer]).PointwiseMultiply(1 - Neurons[currentLayer]) * Neurons[previousLayer].Transpose();

                error = Synapses[previousLayer].Transpose() * error;

                Synapses[previousLayer] += delta;
            }
        }

        static double ActivationSigmoid(double value)
        {
            return 1d / (1d + Math.Pow(Math.E, -value));
        }

        public static void Export(IRINA_NN network, string fileName)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(fileStream, network);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка экспорта", e);
            }
        }

        public static IRINA_NN Import(string fileName)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    return (IRINA_NN)formatter.Deserialize(fileStream);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка импорта", e);
            }
        }

        public static double MSE(double predicted, double ideal)
        {
            return Math.Pow(predicted - ideal, 2D);
        }

        public static double MSE(double[] predicted, double[] ideal)
        {
            if (predicted == null)
                throw new ArgumentNullException();

            if (ideal == null)
                throw new ArgumentNullException();

            var count = predicted.Length;

            if (count == 0)
                throw new InvalidOperationException();

            if (ideal.Length != count)
                throw new InvalidOperationException();

            var error = 0D;

            for (int item = 0; item < count; ++item)
                error += MSE(predicted[item], ideal[item]);

            return error / count;
        }

        public static double MSE(double[][] predicted, double[][] ideal)
        {
            if (predicted == null)
                throw new ArgumentNullException();

            if (predicted == null)
                throw new ArgumentNullException();

            int count = predicted.Length;

            if (count == 0)
                throw new InvalidOperationException();

            if (ideal.Length != count)
                throw new InvalidOperationException();

            var error = 0D;

            for (int item = 0; item < count; ++item)
                error += MSE(predicted[item], ideal[item]);

            return error / count;
        }
    }

}
