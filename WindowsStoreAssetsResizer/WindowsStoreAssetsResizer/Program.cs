using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageResizer;

namespace WindowsStoreAssetsResizer
{
	public struct Size
	{
		public Size(double width, double height)
		{
			Width = width;
			Height = height;
		}

		public double Width;
		public double Height;
	}
	public class Program
	{
		public const string Wa81Square70x70 = "Wa81Square70x70";
		public const string Wa81Square150x150 = "Wa81Square150x150";
		public const string Wa81Wide310x150 = "Wa81Wide310x150";
		public const string Wa81Square310x310 = "Wa81Square310x310";
		public const string Wa81Square30x30 = "Wa81Square30x30";
		public const string Wa81Store = "Wa81Store";
		public const string Wa81SplashScreen = "Wa81SplashScreen";
		public const string Wa81Badge = "Wa81Badge";
		public const string Wa81CustomSize = "Wa81CustomSize";

		public const string Wpa81Square71x71 = "Wpa81Square71x71";
		public const string Wpa81Square150x150 = "Wpa81Square150x150";
		public const string Wpa81Wide310x150 = "Wpa81Wide310x150";
		public const string Wpa81Square44x44 = "Wpa81Square44x44";
		public const string Wpa81Store = "Wpa81Store";
		public const string Wpa81Badge = "Wpa81Badge";
		public const string Wpa81SplashScreen = "Wpa81SplashScreen";
		public const string Wpa81CustomSize = "Wpa81CustomSize";

		private static readonly Dictionary<string, Size> _sizes;
		private static readonly Dictionary<string, double[]> _scales;

		private const string ImageFormat = "{0}.{1}.scale-{2:###}.png";
		private const string ImageCustomSizeFormat = "{0}.scale-{1:###}.png";
		private const string Image30X30TargetSizeFormat = "{0}.{1}.targetsize-{2:###}.png";

		//options
		private const string OptionHelpQuestionMark = "/?";
		private const string OptionDestination = "/dest";
		private const string OptionSizes = "/sizes";
		private const string OptionWidth = "/w";
		private const string OptionHelp = "/help";
		private const string OptionHeight = "/h";
		private const string OptionWpa81 = "/wpa81";
		private const string OptionWa81 = "/wa81";

		static Program()
		{
			_sizes = new Dictionary<string, Size>(StringComparer.OrdinalIgnoreCase)
			{
				{Wa81Square70x70,			new Size(70, 70)},
				{Wa81Square150x150,			new Size(150, 150)},
				{Wa81Wide310x150,			new Size(310, 150)},
				{Wa81Square310x310,			new Size(310, 310)},
				{Wa81Square30x30,			new Size(30, 30)},
				{Wa81Store,					new Size(50, 50)},
				{Wa81SplashScreen,			new Size(620, 300)},
				{Wa81Badge,					new Size(24, 24)},

				{Wpa81Square71x71,			new Size(71, 71)},
				{Wpa81Square150x150,		new Size(150, 150)},
				{Wpa81Wide310x150,			new Size(310, 150)},
				{Wpa81Square44x44,			new Size(44, 44)},
				{Wpa81Store,				new Size(50, 50)},
				{Wpa81SplashScreen,			new Size(480, 800)},
				{Wpa81Badge,				new Size(24, 24)},
			};
			_scales = new Dictionary<string, double[]>()
			{
				{Wa81Square70x70,			new[] { 0.8, 1, 1.4, 1.8 }},
				{Wa81Square150x150,			new[] { 0.8, 1, 1.4, 1.8 }},
				{Wa81Wide310x150,			new[] { 0.8, 1, 1.4, 1.8 }},
				{Wa81Square310x310,			new[] { 0.8, 1, 1.4, 1.8 }},
				{Wa81Square30x30,			new[] { 0.8, 1, 1.4, 1.8 }},
				{Wa81Store,					new[] { 1, 1.4, 1.8 }},
				{Wa81SplashScreen,			new[] { 1, 1.4, 1.8 }},
				{Wa81Badge,					new[] { 1, 1.4, 1.8 }},
				{Wa81CustomSize,			new[] { 1, 1.4, 1.8 }},

				{Wpa81Square71x71,			new[] { 1, 1.4, 2.4 }},
				{Wpa81Square150x150,		new[] { 1, 1.4, 2.4 }},
				{Wpa81Wide310x150,			new[] { 1, 1.4, 2.4 }},
				{Wpa81Square44x44,			new[] { 1, 1.4, 2.4 }},
				{Wpa81Store,				new[] { 1, 1.4, 2.4 }},
				{Wpa81SplashScreen,			new[] { 1, 1.4, 2.4 }},
				{Wpa81Badge,				new[] { 1, 1.4, 2.4 }},
				{Wpa81CustomSize,			new[] { 1, 1.4, 2.4 }},
			};
		}

		private static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Error. No file specified.");
				return;
			}
			if (args[0] == OptionHelpQuestionMark || args[0] == OptionHelp)
			{
				DisplayHelp();
				return;
			}
			var filePath = args[0];
			if (!File.Exists(filePath))
			{
				Console.WriteLine("Error. The file does not exist.");
				return;
			}

			var optionList = args.ToList();

			var destinationPathIndex = optionList.FindIndex(s => s.Equals(OptionDestination, StringComparison.OrdinalIgnoreCase));
			string destinationPath;
			if (destinationPathIndex == -1)
			{
				destinationPath = Path.GetDirectoryName(filePath);
			}
			else
			{
				if (destinationPathIndex + 1 >= optionList.Count)
				{
					Console.WriteLine("Error. The destination folder is invalid");
					return;
				}
				destinationPath = optionList[destinationPathIndex + 1];
				if (!Directory.Exists(destinationPath))
				{
					Directory.CreateDirectory(destinationPath);
				}
			}

			IEnumerable<Action> resizeActions;
			var sizeOption = optionList.FindIndex(s => s.Equals(OptionSizes, StringComparison.OrdinalIgnoreCase));
			if (sizeOption == -1 && optionList.Contains(OptionWidth) && optionList.Contains(OptionHeight))
			{
				var wIndex = optionList.IndexOf(OptionWidth);
				var hIndex = optionList.IndexOf(OptionHeight);
				if (wIndex + 1 >= optionList.Count || hIndex + 1 >= optionList.Count)
				{
					Console.WriteLine("Error. A custom size has been specified but is not valid.");
					return;
				}
				int h;
				int w;
				if (!int.TryParse(optionList[hIndex + 1], out h))
				{
					Console.WriteLine("Error. A custom size has been specified but the height is not valid.");
					return;
				}
				if (!int.TryParse(optionList[wIndex + 1], out w))
				{
					Console.WriteLine("Error. A custom size has been specified but the width is not valid.");
					return;
				}
				var platforms = GetCustomSizePlatform(optionList);
				resizeActions = ResizeImageCustomSize(filePath, destinationPath, w, h, platforms);
			}
			else if (sizeOption + 1 >= optionList.Count)
			{
				Console.WriteLine("Error. The sizes are not specified.");
				return;
			}
			else
			{
				var sizes = optionList[sizeOption + 1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				if (!sizes.All(s => _sizes.ContainsKey(s)))
				{
					Console.WriteLine("Error. Some of the sizes do not exist.");
					Console.WriteLine("The values allowed are :\n" + string.Join("\n", _sizes.Select(k => k.Key)));
					return;
				}
				resizeActions = sizes.SelectMany(size =>
					{
						var a = ResizeImage(filePath, destinationPath, size);
						if (size == Wa81Square30x30)
						{
							a = a.Concat(Resize30X30TargetSizes(filePath, destinationPath));
						}
						return a;
					});
			}
			Parallel.ForEach(resizeActions, action => action());
		}

		private static string[] GetCustomSizePlatform(List<string> optionList)
		{
			var platforms = new List<string>();
			if (optionList.Contains(OptionWa81, StringComparer.OrdinalIgnoreCase))
			{
				platforms.Add(Wa81CustomSize);
			}
			if (optionList.Contains(OptionWpa81, StringComparer.OrdinalIgnoreCase))
			{
				platforms.Add(Wpa81CustomSize);
			}
			if (platforms.Count == 0)
			{
				platforms.Add(Wa81CustomSize);
				platforms.Add(Wpa81CustomSize);
			}
			return platforms.ToArray();
		}

		private static IEnumerable<Action> ResizeImage(string filePath, string destinationPath, string size)
		{
			var originalFileName = Path.GetFileNameWithoutExtension(filePath);
			var targetSize = _sizes[size];
			var scales = _scales[size];
			foreach (var scale in scales)
			{
				Action action = () =>
				{
					var fileName = Path.Combine(destinationPath, string.Format(ImageFormat, originalFileName, size, scale * 100));
					ImageBuilder.Current.Build(
						filePath,
						fileName,
						new ResizeSettings(
							(int)(scale * targetSize.Width),
							(int)(scale * targetSize.Height),
							FitMode.None,
							null
							));
					Console.WriteLine("Created " + Path.GetFileName(fileName));
				};
				yield return action;
			}

		}

		private static IEnumerable<Action> ResizeImageCustomSize(string filePath, string destinationPath, int width, int height, string[] platforms)
		{
			var originalFileName = Path.GetFileNameWithoutExtension(filePath);
			var scales = platforms.Select(p => _scales[p])
								  .SelectMany(p => p)
								  .Distinct()
								  .ToArray();
			return scales.Select(scale => (Action)(() =>
			{
				var fileName = Path.Combine(destinationPath, string.Format(ImageCustomSizeFormat, originalFileName, scale * 100));
				ImageBuilder.Current.Build(
					filePath,
					fileName,
					new ResizeSettings(
						(int)(scale * width),
						(int)(scale * height),
						FitMode.None,
						null
						));
				Console.WriteLine("Created " + Path.GetFileName(fileName));
			}));

		}

		private static IEnumerable<Action> Resize30X30TargetSizes(string filePath, string destinationPath)
		{
			var targetSizes = new[] { 16, 32, 48, 256 };
			var originalFileName = Path.GetFileNameWithoutExtension(filePath);
			return targetSizes.Select(targetSize => (Action)(() =>
			{
				var fileName = Path.Combine(destinationPath, string.Format(Image30X30TargetSizeFormat, originalFileName, Wa81Square30x30, targetSize));
				ImageBuilder.Current.Build(
					filePath,
					fileName,
					new ResizeSettings(
						targetSize,
						targetSize,
						FitMode.None,
						null
						));
				Console.WriteLine("Created " + Path.GetFileName(fileName));
			}));
		}

		private static void DisplayHelp()
		{
			Console.WriteLine();
			Console.WriteLine("aresizer <source> [/dest <destination path>] [/sizes <size1,size2,..> | /w <width> /h <height> [/wa81] [/wpa81]]");
			Console.WriteLine();
			Console.WriteLine("/dest : Define the destination folder");
			Console.WriteLine("/sizes <{0}>", string.Join(",", _sizes.Select(k => k.Key)));
			Console.WriteLine("Define the manifest asset sizes");
			Console.WriteLine("/w : The target width of the asset. For custom size.");
			Console.WriteLine("/h : The target height of the asset. For custom size.");
			Console.WriteLine("/wpa81 : If using a custom size, use only Windows Phone 8.1 scales.");
			Console.WriteLine("/wa81 : If using a custom size, Use only Windows 8.1 scales.");
		}
	}
}
