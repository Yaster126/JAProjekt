﻿//Konwerter plików WAVE stereo na mono
//Algorytm w języku asemblera
//29.01.2023, sem. 5, Grzegorz Nowak

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
    }
}