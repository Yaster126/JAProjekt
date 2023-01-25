using Microsoft.Win32;
using System;
using System.Windows;
using System.Media;
using NAudio.Wave;
using System.Runtime.InteropServices;
using StereoToMonoConverterCSharp;

namespace JAProjekt
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//private SoundPlayer player;
		private string inputPath = "";
		private string outputPath = "";
		private int sampleRate = 0;
		private int bitsPerSample = 0;
		private int channels = 0;
		private float[] Stereo = Array.Empty<float>();
		private float[] Mono = Array.Empty<float>();

		[DllImport(@"C:\Grzesia\Projekty\Visual Studio 2022\JAProjekt\x64\Debug\StereoToMonoConverterASM.dll")]
		static extern int StereoToMonoAsm(int a, int b);


		public void ReadWav(string path)
		{
			using MediaFoundationReader media = new MediaFoundationReader(path);
			
			sampleRate = media.WaveFormat.SampleRate;
			bitsPerSample = media.WaveFormat.BitsPerSample;
			channels = media.WaveFormat.Channels;

			int byteLength = (int)media.Length * 2;
			int tabSize = byteLength / sizeof(float);

			IWaveProvider stream = new Wave16ToFloatProvider(media);
			WaveBuffer _waveBuffer = new(byteLength);
			stream.Read(_waveBuffer, 0, byteLength);
			Stereo = new float[tabSize];

			for (int i = 0; i < tabSize; i++)
			{
				Stereo[i] = _waveBuffer.FloatBuffer[i];
			}
		}

		public void GenerateFileName()
		{
			int idx = inputPath.LastIndexOf('.');

			if (idx != -1)
			{
				outputPath = string.Concat(inputPath.AsSpan(0, idx), "_MONO", inputPath.AsSpan(idx + 1));
			}
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

				Play_Stereo.IsEnabled = true;
			}


		}

		private void Play_Stereo_Click(object sender, RoutedEventArgs e)
		{
			SoundPlayer player = new(inputPath);
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Play_Mono_Click(object sender, RoutedEventArgs e)
		{
			//TODO
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			SoundPlayer player = new();
			player.Stop();
			Stop.IsEnabled = false;
		}

		private void Run_Click(object sender, RoutedEventArgs e)
		{
			if(CSharp.IsChecked == true)
			{
				Mono = StereoToMonoConverter(Stereo);
			}
			else if(ASM.IsChecked == true)
			{

			}
		}
	}
}
