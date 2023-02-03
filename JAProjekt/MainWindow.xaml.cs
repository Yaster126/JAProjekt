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
			/*SoundPlayer */
			player = new(inputPath);
			player.Load();
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Play_Mono_Click(object sender, RoutedEventArgs e)
		{
			/*SoundPlayer */
			player = new(outputPath);
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

		public static float[][] Split(float[] array, int size)
		{
			return Enumerable.Range(0, (array.Length / size) + 1).Select(i => array.Skip(i * size).Take(size).ToArray()).ToArray();
		}

		private void Run_Click(object sender, RoutedEventArgs e)
		{
			float[][] temp = Split(Stereo, 16);
			float[][] result = new float[temp.Length][];
			var thread = (int)Thread.Value;
			Mono = new float[Stereo.Length / channels];

			if (CSharp.IsChecked == true)
			{
				var watch = System.Diagnostics.Stopwatch.StartNew();
				Parallel.For(0, temp.Length, new ParallelOptions { MaxDegreeOfParallelism = thread }, index =>
				{
					result[index] = StereoToMonoConverter.StereoToMono(temp[index], channels);
				});
				watch.Stop();
				for (int i = 0; i < result.Length; ++i)
				{
					int t = result[i].Length;
					for (int j = 0; j < t; ++j)
					{
						Mono[(i * result[i].Length) + j] = result[i][j];
					}
				}
				WriteWav(Mono);
				Timer.Content = "Czas: " + watch.ElapsedMilliseconds + " ms";
				Play_Mono.IsEnabled = true;
			}
			else if (ASM.IsChecked == true)
			{
				//Parallel.For(0, result.Length, new ParallelOptions { MaxDegreeOfParallelism = 4 }, index =>
				//{
				//	result[index] = new float[temp[index].Length/2];
				//});
				for(int i = 0; i < temp.Length; ++i)
				{
					result[i] = new float[(temp[i].Length)/2];
				}
				var watch = System.Diagnostics.Stopwatch.StartNew();
				Parallel.For(0, temp.Length, new ParallelOptions { MaxDegreeOfParallelism = thread }, index =>
				{
					StereoToMonoAsm(temp[index], temp[index].Length, result[index]);
				});
				watch.Stop();

				for (int i = 0; i < result.Length; ++i)
				{
					int t = result[i].Length;
					for (int j = 0; j < t; ++j)
					{
						Mono[(i * result[i].Length) + j] = result[i][j];
					}
				}
				WriteWav(Mono);
				Timer.Content = "Czas: " + watch.ElapsedMilliseconds + " ms";

				Play_Mono.IsEnabled = true;
			}
		}

		private void Test_Click(object sender, RoutedEventArgs e)
		{
			float[] tab = new float[16] { 0.1f, 0.1f, 0.2f, 0.2f, 0.3f, 0.3f, 0.4f, 0.4f, 0.5f, 0.5f, 0.6f, 0.6f, 0.7f, 0.7f, 0.8f, 0.8f };
			float[] wyj = new float[8];

			StereoToMonoAsm(tab, tab.Length, wyj);
		}
	}
}
