using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Irina
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

		int samplesInBatch = 100;
		int count = 1000;
		int even = 10;
		IChartValues values;

		public MainWindow()
		{
			InitializeComponent();

			values = new ChartValues<double>();

			SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Title = "Ошибка",
					Values = values
				}
			};

			Labels = new string[count];

			for (int i = 0; i < count / even; ++i)
				Labels[i] = ((i + 1) * even).ToString();

			YFormatter = value => (value).ToString("p");

			DataContext = this;
		}

		double[] prepareInput(byte[] image)
		{
			var count = image.Length;
			var result = new double[count];

			for (int i = 0; i < count; ++i)
				result[i] = image[i] / 255d * 0.99 + 0.01;

			return result;
		}

		double[] prepareTarget(int label)
		{
			const int labelsCount = 10;

			var result = new double[labelsCount];

			for (int i = 0; i < labelsCount; ++i)
				result[i] = 0.01;

			result[label] = 0.99;

			return result;
		}

		void drawMNIST()
		{
			var ira = new IRINA_NN(new int[] { 784, 200, 10 }, 0.2);

			var imageReader = new MNISTDataReader.ImageDataReader(@"D:\NeuralNetworks\DataSets\MNIST\train-images.idx3-ubyte");

			var labelReader = new MNISTDataReader.LabelDataReader(@"D:\NeuralNetworks\DataSets\MNIST\train-labels.idx1-ubyte");

			var itemsCount = imageReader.ItemsCount;
			var random = new Random();

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			for (int epoch = 0; epoch < count; ++epoch)
			{
				//var errors = new List<double>();

				for (int sample = 0; sample < samplesInBatch; ++sample)
				{
					var realIndex = random.Next(0, itemsCount);
					var input = prepareInput(imageReader.Read(realIndex));
					var label = labelReader.Read(realIndex);
					var target = prepareTarget(label);

					ira.Train(input, target);

					//var predicted = ira.Predict(input);
					//errors.Add(0.99 - predicted[label]);
				}

				//SeriesCollection[0].Values.Add(errors.Average());
			}

			stopwatch.Stop();

			IRINA_NN.Export(ira, @"mnist.nn");

			MessageBox.Show($"Complete { count } epochs, { samplesInBatch } samples per one. Estimated time: { stopwatch.ElapsedMilliseconds }ms");
		}

		void drawXor()
		{
			var ira = new IRINA_NN(new int[] { 2, 4, 4, 1 }, 0.2);

			var e1 = ira.Predict(input: new double[] { 0.99, 0.99 })[0];
			var e2 = ira.Predict(input: new double[] { 0.99, 0.01 })[0];
			var e3 = ira.Predict(input: new double[] { 0.01, 0.99 })[0];
			var e4 = ira.Predict(input: new double[] { 0.01, 0.01 })[0];

			var error = (e1 + e2 + e3 + e4) / 4d;

			for (int epoch = 0; epoch < count; ++epoch)
			{
				ira.Train(
					input: new double[] { 0.01, 0.01 },
					target: new double[] { 0.01 }
				);

				e1 = IRINA_NN.MSE(ira.Predict(new double[] { 0.01, 0.01 }), new double[] { 0.01 });

				ira.Train(
					input: new double[] { 0.99, 0.01 },
					target: new double[] { 0.99 }
				);

				e2 = IRINA_NN.MSE(ira.Predict(new double[] { 0.99, 0.01 }), new double[] { 0.99 });

				ira.Train(
					input: new double[] { 0.01, 0.99 },
					target: new double[] { 0.99 }
				);

				e3 = IRINA_NN.MSE(ira.Predict(new double[] { 0.01, 0.99 }), new double[] { 0.99 });

				ira.Train(
					input: new double[] { 0.99, 0.99 },
					target: new double[] { 0.01 }
				);

				e4 = IRINA_NN.MSE(ira.Predict(new double[] { 0.99, 0.99 }), new double[] { 0.01 });

				error = (e1 + e2 + e3 + e4) / 4d;

				//lbOutput.Items.Add($"Ошибка эпохи { epoch }: { error }");

				if (epoch % even == 0)
					SeriesCollection[0].Values.Add(error);
			}

			e2 = ira.Predict(input: new double[] { 0.99, 0.01 })[0];
			e1 = ira.Predict(input: new double[] { 0.99, 0.99 })[0];
			e4 = ira.Predict(input: new double[] { 0.01, 0.01 })[0];
			e3 = ira.Predict(input: new double[] { 0.01, 0.99 })[0];
			e1 = e2 = e3 = e4;
		}

		void drawCelsiusFaringate()
		{
			var ira = new IRINA_NN(new int[] { 8, 16, 32, 16, 8 }, 0.2);

			for (int epoch = 0; epoch < count; ++epoch)
			{
				ira.Train( // 40 - 104
					input: new double[] { 0.01, 0.01, 0.99, 0.01, 0.99, 0.01, 0.01, 0.01 },
					target: new double[] { 0.01, 0.99, 0.99, 0.01, 0.99, 0.01, 0.01, 0.01 }
				);

				var e1 = IRINA_NN.MSE(ira.Predict(new double[] { 0.01, 0.01, 0.99, 0.01, 0.99, 0.01, 0.01, 0.01 }), new double[] { 0.01, 0.99, 0.99, 0.01, 0.99, 0.01, 0.01, 0.01 });

				ira.Train( // 60 - 140
					input: new double[] { 0.01, 0.01, 0.99, 0.99, 0.99, 0.99, 0.01, 0.01 },
					target: new double[] { 0.99, 0.01, 0.01, 0.01, 0.01, 0.99, 0.99, 0.01 }
				);

				var e2 = IRINA_NN.MSE(ira.Predict(new double[] { 0.01, 0.01, 0.99, 0.99, 0.99, 0.99, 0.01, 0.01 }), new double[] { 0.99, 0.01, 0.01, 0.01, 0.01, 0.99, 0.99, 0.01 });

				ira.Train( // 15 - 59
					input: new double[] { 0.01, 0.01, 0.01, 0.01, 0.99, 0.99, 0.99, 0.99 },
					target: new double[] { 0.01, 0.01, 0.99, 0.99, 0.99, 0.01, 0.99, 0.99 }
				);

				var e3 = IRINA_NN.MSE(ira.Predict(new double[] { 0.01, 0.01, 0.01, 0.01, 0.99, 0.99, 0.99, 0.99 }), new double[] { 0.01, 0.01, 0.99, 0.99, 0.99, 0.01, 0.99, 0.99 });

				ira.Train( // 25 - 77
					input: new double[] { 0.01, 0.01, 0.01, 0.99, 0.99, 0.01, 0.01, 0.99 },
					target: new double[] { 0.01, 0.99, 0.01, 0.01, 0.99, 0.99, 0.01, 0.99 }
				);

				var e4 = IRINA_NN.MSE(ira.Predict(new double[] { 0.01, 0.01, 0.01, 0.99, 0.99, 0.01, 0.01, 0.99 }), new double[] { 0.01, 0.99, 0.01, 0.01, 0.99, 0.99, 0.01, 0.99 });

				var error = (e1 + e2 + e3 + e4) / 4d;

				if (epoch % even == 0)
					SeriesCollection[0].Values.Add(error);
			}

			var predicted = ira.Predict(new double[] { 0.01, 0.01, 0.01, 0.99, 0.99, 0.01, 0.01, 0.99 });
			var target = new double[] { 0.01, 0.99, 0.01, 0.01, 0.99, 0.99, 0.01, 0.99 };

			var err = IRINA_NN.MSE(predicted, target);

			predicted = ira.Predict(new double[] { 0.01, 0.01, 0.99, 0.01, 0.01, 0.01, 0.99, 0.99 });
			target = new double[] { 0.01, 0.99, 0.01, 0.99, 0.99, 0.99, 0.99, 0.99 };

			err = IRINA_NN.MSE(predicted, target);
		}

		void drawCelsiusFaringate2()
		{
			var ira = new IRINA_NN(new int[] { 1, 6, 9, 3, 1 }, 0.2);

			var input = new List<double[]>
			{
				new double[] { 0.040 },
				new double[] { 0.035 },
				new double[] { 0.037 },
				new double[] { 0.048 },
				new double[] { -0.012 },
				new double[] { -0.040 },
				new double[] { 0.057 },
				new double[] { 0.089 },
				new double[] { -0.005 },
				new double[] { 0.100 },
				new double[] { -0.042 }
			};

			var target = new List<double[]>
			{
				new double[] { 0.104 },
				new double[] { 0.095 },
				new double[] { 0.0986 },
				new double[] { 0.1184 },
				new double[] { 0.0104 },
				new double[] { -0.040 },
				new double[] { 0.1346 },
				new double[] { 0.1922 },
				new double[] { 0.023 },
				new double[] { 0.212 },
				new double[] { -0.0436 }
			};

			var samplesCount = input.Count;

			var random = new Random();

			for (int epoch = 0; epoch < count; ++epoch)
			{
				var index = random.Next(0, samplesCount);

				var inputItem = input[index];
				var targetItem = target[index];

				ira.Train(inputItem, targetItem);

				var error = IRINA_NN.MSE(ira.Predict(inputItem), inputItem);

				if (epoch % even == 0)
					SeriesCollection[0].Values.Add(error);
			}

			var result = ira.Predict(new double[] { 0.67 })[0];
			result = 0d;
		}

		void drawIris()
		{
			var ira = new IRINA_NN(new int[] { 4, 16, 64, 24, 3 }, 0.02);

			IrisDataReader irisDataReader = new IrisDataReader(@"D:\NeuralNetworks\DataSets\Iris Plants Database\bezdekIris.data");

			var rand = new Random();

			for (int epoch = 0; epoch < count; ++epoch)
			{
				var irisData = irisDataReader.Read(rand.Next(0, 150));
				var (input, target) = irisToNN(irisData);

				ira.Train(input, target);

				var predict = ira.Predict(input);

				var error = IRINA_NN.MSE(predict, target);

				if (epoch % even == 0)
					SeriesCollection[0].Values.Add(error);
			}

			var data = irisDataReader.Read(25);
			var (inputRes, targetRes) = irisToNN(data);

			var predictRes = ira.Predict(inputRes);

			var errorRes = IRINA_NN.MSE(predictRes, targetRes);

			data = irisDataReader.Read(72);
			(inputRes, targetRes) = irisToNN(data);

			predictRes = ira.Predict(inputRes);

			errorRes = IRINA_NN.MSE(predictRes, targetRes);

			data = irisDataReader.Read(119);
			(inputRes, targetRes) = irisToNN(data);

			predictRes = ira.Predict(inputRes);

			errorRes = IRINA_NN.MSE(predictRes, targetRes);
		}

		(double[] Input, double[] Target) irisToNN(IrisDataReader.IrisDataItem irisData)
		{
			var input = new double[4];
			input[0] = irisData.SepalLength / 10d;
			input[1] = irisData.SepalWidth / 10d;
			input[2] = irisData.PetalLength / 10d;
			input[3] = irisData.PetalWidth / 10d;

			var target = new double[3];
			target[0] = (irisData.ClassName.Equals("Iris-setosa", StringComparison.OrdinalIgnoreCase) ? 0.99 : 0.01);
			target[1] = (irisData.ClassName.Equals("Iris-versicolor", StringComparison.OrdinalIgnoreCase) ? 0.99 : 0.01);
			target[2] = (irisData.ClassName.Equals("Iris-virginica", StringComparison.OrdinalIgnoreCase) ? 0.99 : 0.01);

			return (Input: input, Target: target);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//var viewer = new ImagesViewer();
			//viewer.Show();

			Task.Factory.StartNew(() => drawCelsiusFaringate2());
		}
	}
}
