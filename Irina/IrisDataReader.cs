using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNISTDataReader;

namespace Irina
{
	public class IrisDataReader : IDataReader<IrisDataReader.IrisDataItem>
	{
		public class IrisDataItem
		{
			public double SepalLength;
			public double SepalWidth;
			public double PetalLength;
			public double PetalWidth;
			public string ClassName;

			public static IrisDataItem FromCommaSeparatedString(string data)
			{
				var parts = data.Split(',');

				if (parts.Length != FieldsCount)
					throw new InvalidOperationException();

				var result = new IrisDataItem();

				if (double.TryParse(parts[SepalLengthIndex].Replace('.', ','), out double value))
					result.SepalLength = value;

				if (double.TryParse(parts[SepalWidthIndex].Replace('.', ','), out value))
					result.SepalWidth = value;

				if (double.TryParse(parts[PetalLengthIndex].Replace('.', ','), out value))
					result.PetalLength = value;

				if (double.TryParse(parts[PetalWidthIndex].Replace('.', ','), out value))
					result.PetalWidth = value;

				result.ClassName = parts[ClassNameIndex];

				return result;
			}

			const int FieldsCount = 5;
			const int SepalLengthIndex = 0;
			const int SepalWidthIndex = 1;
			const int PetalLengthIndex = 2;
			const int PetalWidthIndex = 3;
			const int ClassNameIndex = 4;
		}

		Dictionary<int, IrisDataItem> cache;

		public IrisDataReader(string fileName)
		{
			using (var input = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(input))
			{
				cache = new Dictionary<int, IrisDataItem>();

				int index = 0;

				while (!reader.EndOfStream)
				{
					var data = reader.ReadLine();

					if (!string.IsNullOrWhiteSpace(data))
						cache.Add(index++, IrisDataItem.FromCommaSeparatedString(data));
				}
			}
		}

		public IrisDataItem Read(int index)
		{
			if (!cache.TryGetValue(index, out IrisDataItem result))
				throw new IndexOutOfRangeException();

			return result;
		}
	}
}
