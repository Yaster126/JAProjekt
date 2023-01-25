using Microsoft.Win32;
using System;
using System.Windows;
using System.Media;
using NAudio.Wave;
using System.Runtime.InteropServices;
using StereoToMonoConverterCSharp;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using System.Reflection;
using System.Threading;

namespace JAProjekt
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		SoundPlayer player;
		private string inputPath = "";
		private string outputPath = "";
		private int sampleRate = 0;
		private int bitsPerSample = 0;
		private int channels = 0;
		private float[] Stereo = Array.Empty<float>();
		private float[] Mono = Array.Empty<float>();

		[DllImport(@"C:\Grzesia\Projekty\Visual Studio 2022\JAProjekt\x64\Debug\StereoToMonoConverterASM.dll")]
		static extern void StereoToMonoAsm(float[] a, int b, float[] c);


		public void ReadWav(string path)
		{
			using MediaFoundationReader media = new(path);

			sampleRate = media.WaveFormat.SampleRate;
			bitsPerSample = media.WaveFormat.BitsPerSample;
			channels = media.WaveFormat.Channels;

			int byteLength = (int)media.Length * 2;
			int tabSize = byteLength / sizeof(float);

			IWaveProvider stream = new Wave16ToFloatProvider(media);
			WaveBuffer waveBuffer = new(byteLength);
			stream.Read(waveBuffer, 0, byteLength);
			Stereo = new float[tabSize];

			for (int i = 0; i < tabSize; i++)
			{
				Stereo[i] = waveBuffer.FloatBuffer[i];
			}

			outputPath = GenerateFileName(inputPath);
		}

		public static string GenerateFileName(string path)
		{
			int idx = path.LastIndexOf('.');

			if (idx != -1)
			{
				return string.Concat(path.AsSpan(0, idx), "_MONO.", path.AsSpan(idx + 1));
			}
			return "";
		}

		public void WriteWav(float[] floatOutput)
		{
			WaveFormat waveFormat = new(sampleRate, bitsPerSample, 1);
			using WaveFileWriter writer = new(outputPath, waveFormat);
			writer.WriteSamples(floatOutput, 0, floatOutput.Length);
		}

		public MainWindow()
		{
			InitializeComponent();
			Thread.Value = Environment.ProcessorCount;
		}

		private void Choose_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Audio file (*.wav)|*wav";
			if (openFileDialog.ShowDialog() == true)
			{
				inputPath = openFileDialog.FileName;
				FileName.Content = openFileDialog.SafeFileName;

				ReadWav(inputPath);

				Run.IsEnabled = true;
				Play_Stereo.IsEnabled = true;
			}


		}

		private void Play_Stereo_Click(object sender, RoutedEventArgs e)
		{
			/*SoundPlayer */player = new(inputPath);
			player.Load();
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Play_Mono_Click(object sender, RoutedEventArgs e)
		{
			/*SoundPlayer */player = new(outputPath);
			player.Load();
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			//SoundPlayer player = new();
			player.Stop();
			Stop.IsEnabled = false;
		}

		public static IEnumerable<IEnumerable<T>> Split<T>(T[] array, int size)
		{
			for (var i = 0; i < (float)array.Length / size; i++)
			{
				yield return array.Skip(i * size).Take(size);
			}
		}

		private void Tup(object data, float[][] result)
		{
			Tuple<int, float[]> tuple = (Tuple<int, float[]>)data;
			int index = tuple.Item1;
			float[] temp = (float[])tuple.Item2.Clone();
			float[] tempOut = StereoToMonoConverter.StereoToMono(temp, index, channels);
			//float[] tempOut = new float[1];
			//Array.Copy(tempOut, 0, Mono, index, tempOut.Length);
			result[index] = (float[])tempOut.Clone();
		}


		private void Run_Click(object sender, RoutedEventArgs e)
		{
			int size = Stereo.Length / 8;
			float[][] temp = new float[size][];
			for (int i = 0; i < temp.Length; ++i)
			{
				temp[i] = new float[8];
				for (int j = 0; j < 8; ++j)
					temp[i][j] = Stereo[i * 8 + j];
			}

			float[][] result = new float[temp.Length][];

			if (CSharp.IsChecked == true)
			{
				// var temp = Split(Stereo, 8);

				var watch = System.Diagnostics.Stopwatch.StartNew();
				var task = Task.Run(() =>
				{
					Parallel.For(0, Stereo.Length, new ParallelOptions { MaxDegreeOfParallelism = (int)Thread.Value }, i =>
					{
						Tuple<int, float[]> tuple = Tuple.Create(i, (float[])temp[i]);
						Tup(tuple, result);
					});
				});
				task.Wait();
				watch.Stop();
				for(int i = 0; i< result.Length; ++i)
					Array.Copy(result[i], 0, Mono, i*4, result.Length);

				WriteWav(Mono);
				Timer.Content = "Czas: " + watch.ElapsedMilliseconds + " ms";

				Play_Mono.IsEnabled = true;
			}
			else if (ASM.IsChecked == true)
			{
				////private object f = new object();
				//int f = 0;

				//var watch = System.Diagnostics.Stopwatch.StartNew();

				////lock (f)
				////{
				//	Parallel.ForEach(tempIn, i =>
				//	{
				//		StereoToMonoAsm(i, i.Length, tempOut[f]);
				//		++f;
				//	});
				////}
				//watch.Stop();

				//for (int i = 0; i < tempOut.Length; i++)
				//	Mono = Mono.Concat(tempOut[i]).ToArray();

				//WriteWav(Mono);
				//Timer.Content = "Czas: " + watch.ElapsedMilliseconds + " ms";

				//Play_Mono.IsEnabled = true;
			}
		}
	}
}
