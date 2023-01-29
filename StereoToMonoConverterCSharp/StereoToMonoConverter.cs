using Microsoft.VisualBasic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace StereoToMonoConverterCSharp
{
    public class StereoToMonoConverter
    {
        public static float[] StereoToMono(float[] data, int channels)
        {
            int resultSize = data.Length / channels;
            float[] result = new float[resultSize];
            for (int i = 0; i < resultSize; i++)
            {
                float temp = 0;

                for (int j = 0; j < channels; j++)
                {
                    int x = (i * channels) + j;
                    temp += data[x];
                }

                result[i] = (temp / channels);
            }

            return result;
        }

        public static void StereoToMono(float[] data, int indexIn, float[] result, int channels)
        {
            float temp = 0;

            for (int j = 0; j < channels; j++)
            {

                temp += data[indexIn + j];

            }
            result[indexIn / 2] = temp / channels;

        }
    }
}