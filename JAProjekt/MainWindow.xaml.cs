﻿using Microsoft.Win32;
using System;
using System.Windows;
using System.Media;
using NAudio.Wave;
using System.Runtime.InteropServices;
using StereoToMonoConverterCSharp;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace JAProjekt
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
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
			if (tabSize % 2 == 0)
			{
				Stereo = new float[tabSize];

				for (int i = 0; i < tabSize; i++)
				{
					Stereo[i] = waveBuffer.FloatBuffer[i];
				}
			}
			else
			{
				Stereo = new float[tabSize + 1];

				for (int i = 0; i < tabSize; i++)
				{
					Stereo[i] = waveBuffer.FloatBuffer[i];
				}
				Stereo[tabSize] = 0f;
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
				if (Test_mode.IsChecked == false)
				{
					Run.IsEnabled = true;
					Play_Stereo.IsEnabled = true;
				}
				else
					Test.IsEnabled = true;
			}
		}

		private void Play_Stereo_Click(object sender, RoutedEventArgs e)
		{
			SoundPlayer player = new(inputPath);
			player.Load();
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Play_Mono_Click(object sender, RoutedEventArgs e)
		{
			SoundPlayer player = new(outputPath);
			player.Load();
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			SoundPlayer player = new();
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
				Timer.Content = "Czas: " + watch.Elapsed.TotalMilliseconds + " ms";
				Play_Mono.IsEnabled = true;
			}
			else if (ASM.IsChecked == true)
			{
				Parallel.For(0, result.Length, new ParallelOptions { MaxDegreeOfParallelism = 4 }, index =>
				{
					result[index] = new float[temp[index].Length / 2];
				});


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

				Timer.Content = "Czas: " + watch.Elapsed.TotalMilliseconds + " ms";

				Play_Mono.IsEnabled = true;
			}
		}

		private void Test_Click(object sender, RoutedEventArgs e)
		{
			float[][] temp = Split(Stereo, 16);
			float[][] result = new float[temp.Length][];
			Mono = new float[Stereo.Length / channels];

			StreamWriter file = new("Results.txt", append: true);
			file.WriteLine("\n" + inputPath);

			file.WriteLine("C#");
			for (int thread = 1; thread < 65; ++thread)
			{
				file.WriteLine("Wątków: " + thread);
				for (int i = 0; i < 11; ++i)
				{
					var watch = System.Diagnostics.Stopwatch.StartNew();
					Parallel.For(0, temp.Length, new ParallelOptions { MaxDegreeOfParallelism = thread }, index =>
					{
						result[index] = StereoToMonoConverter.StereoToMono(temp[index], channels);
					});
					watch.Stop();

					file.WriteLine("\t" + i + ": " + watch.Elapsed.TotalMilliseconds.ToString());
				}
			}

			Parallel.For(0, result.Length, new ParallelOptions { MaxDegreeOfParallelism = 4 }, index =>
			{
				result[index] = new float[temp[index].Length / 2];
			});

			file.WriteLine("ASM");
			for (int thread = 1; thread < 65; ++thread)
			{
				file.WriteLine("Wątków: " + thread);
				for (int i = 0; i < 11; ++i)
				{
					var watch = System.Diagnostics.Stopwatch.StartNew();
					Parallel.For(0, temp.Length, new ParallelOptions { MaxDegreeOfParallelism = thread }, index =>
					{
						StereoToMonoAsm(temp[index], temp[index].Length, result[index]);
					});
					watch.Stop();

					file.WriteLine("\t" + i + ": " + watch.Elapsed.TotalMilliseconds.ToString());
				}
			}

			file.Close();
			MessageBox.Show("Done");

		}

		private void Test_Checked(object sender, RoutedEventArgs e)
		{
			if (inputPath != "")
				Test.IsEnabled = true;
			Run.IsEnabled = false;
			ASM.IsEnabled = false;
			CSharp.IsEnabled = false;
			Thread.IsEnabled = false;
			Play_Stereo.IsEnabled = false;
		}

		private void Test_Unchecked(object sender, RoutedEventArgs e)
		{
			Test.IsEnabled = false;
			if (inputPath != "")
			{
				Run.IsEnabled = true;
				Play_Stereo.IsEnabled = true;
			}
			ASM.IsEnabled = true;
			CSharp.IsEnabled = true;
			Thread.IsEnabled = true;
		}
	}
}
