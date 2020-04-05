using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Irina
{
	internal static class ExtensionMethods
	{
		const int DPI = 96;

		public static byte[] Inverse(this byte[] value)
		{
			var count = value.Length;
			var result = new byte[count];

			for (int i = 0; i < count; ++i)
				result[i] = (byte)(0xFF - value[i]);

			return result;
		}

		public static BitmapSource ToBitmapSource(this byte[] value, int width, int height)
		{
			var pixelFormat = PixelFormats.Gray8;
			var palette = BitmapPalettes.Gray256;
			var stride = (width * pixelFormat.BitsPerPixel) / 8;

			return BitmapSource.Create(width, height, DPI, DPI, pixelFormat, palette, value, stride);
		}

		public static int IndexOfMax<T>(this T[] value) where T : IComparable<T>
		{
			T max = default;
			var indexOf = -1;

			for (int i = 0; i < 10; ++i)
				if (value[i].CompareTo(max) > 0)
					max = value[indexOf = i];

			return indexOf;
		}
	}
}
