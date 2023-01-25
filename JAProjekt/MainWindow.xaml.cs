using Microsoft.Win32;
using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using NAudio;
using NAudio.Wave;

namespace JAProjekt
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SoundPlayer player;
		private string inputPath = "";
		private string outputPath = "";
		private float[] Stereo = Array.Empty<float>();
		private float[] Mono = Array.Empty<float>();


		public void ReadWav(string path)
		{
			using (MediaFoundationReader media = new MediaFoundationReader(path))
			{
				int _byteBuffer32_length = (int)media.Length * 2;
				int _floatBuffer_length = _byteBuffer32_length / sizeof(float);

				IWaveProvider stream32 = new Wave16ToFloatProvider(media);
				WaveBuffer _waveBuffer = new(_byteBuffer32_length);
				stream32.Read(_waveBuffer, 0, (int)_byteBuffer32_length);
				floatBuffer = new float[_floatBuffer_length];

				for (int i = 0; i < _floatBuffer_length; i++)
				{
					floatBuffer[i] = _waveBuffer.FloatBuffer[i];
				}
			}
		}

		public void WriteWav(float[] floatOutput)
		{
			WaveFormat waveFormat = new WaveFormat(sampleRate, bitsPerSample, 1);
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
			player = new(inputPath);
			player.Play();

			Stop.IsEnabled = true;
		}

		private void Play_Mono_Click(object sender, RoutedEventArgs e)
		{
			//TODO
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			player.Stop();
			Stop.IsEnabled = false;
		}

		private void Run_Click(object sender, RoutedEventArgs e)
		{
			if(CSharp.IsChecked == true)
			{

			}
			else if(ASM.IsChecked == true)
			{

			}
		}
	}
}
