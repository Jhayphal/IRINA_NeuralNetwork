using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Irina
{
	/// <summary>
	/// Логика взаимодействия для ImagesViewer.xaml
	/// </summary>
	public partial class ImagesViewer : Window
	{
		IRINA_NN ira;
		MNISTDataReader.ImageDataReader reader;
		MNISTDataReader.LabelDataReader lablesReader;
		int currentImageIndex = 0;

		public ImagesViewer()
		{
			InitializeComponent();
		}

		double[] prepareInput(byte[] image)
		{
			var count = image.Length;
			var result = new double[count];

			for (int i = 0; i < count; ++i)
				result[i] = image[i] / 255d * 0.99 + 0.01;

			return result;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ira = IRINA_NN.Import(@"mnist.nn");
			reader = new MNISTDataReader.ImageDataReader(@"D:\NeuralNetworks\DataSets\MNIST\t10k-images.idx3-ubyte");
			lablesReader = new MNISTDataReader.LabelDataReader(@"D:\NeuralNetworks\DataSets\MNIST\t10k-labels.idx1-ubyte");

			Predict();

			//IrisDataReader irisDataReader = new IrisDataReader(@"D:\NeuralNetworks\DataSets\Iris Plants Database\bezdekIris.data");

			//var rand = new Random();

			//while(true)
			//{
			//	irisDataReader.Read(rand.Next(0, 150));
			//}

			//calcError();
		}

		void Predict()
		{
			var image = reader.Read(currentImageIndex);

			viewPort.Source = image.Inverse().ToBitmapSource(reader.NumberOfColumns, reader.NumberOfRows);

			var result = ira.Predict(prepareInput(image));

			prediction.Text = result.IndexOfMax().ToString();
			expected.Text = lablesReader.Read(currentImageIndex).ToString();
		}

		void calcError()
		{
			int count = reader.ItemsCount;
			List<int> errors = new List<int>(count);

			for (int i = 0; i < count; ++i)
				errors.Add(ira.Predict(prepareInput(reader.Read(i))).IndexOfMax() == lablesReader.Read(i) ? 1 : 0);

			MessageBox.Show(errors.Average().ToString("p"));
		}

		private void prev_Click(object sender, RoutedEventArgs e)
		{
			if (currentImageIndex == 0)
				return;

			--currentImageIndex;

			Predict();
		}

		private void next_Click(object sender, RoutedEventArgs e)
		{
			if (currentImageIndex == reader.ItemsCount)
				return;

			++currentImageIndex;

			Predict();
		}
	}
}
