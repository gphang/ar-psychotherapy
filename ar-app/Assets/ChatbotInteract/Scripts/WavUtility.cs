// WavUtility.cs
using System;
using System.IO;
using UnityEngine;

// Helper script to convert raw WAV byte data into a playable AudioClip
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavData)
    {
        using (MemoryStream stream = new MemoryStream(wavData))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Parse WAV header
                reader.ReadBytes(4); // RIFF
                reader.ReadInt32(); // chunk size
                reader.ReadBytes(4); // WAVE
                reader.ReadBytes(4); // fmt
                reader.ReadInt32(); // subchunk 1 size
                int audioFormat = reader.ReadInt16();
                int numChannels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byte rate
                reader.ReadInt16(); // block align
                int bitsPerSample = reader.ReadInt16();
                reader.ReadBytes(4); // data
                int dataSize = reader.ReadInt32();

                if (audioFormat != 1 || (bitsPerSample != 16 && bitsPerSample != 32))
                {
                    Debug.LogError("Unsupported WAV format. Only 16-bit or 32-bit PCM is supported.");
                    return null;
                }

                // Read audio data
                byte[] audioBytes = reader.ReadBytes(dataSize);
                float[] samples;

                if (bitsPerSample == 16)
                {
                    samples = new float[dataSize / 2];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        samples[i] = (float)BitConverter.ToInt16(audioBytes, i * 2) / 32768.0f;
                    }
                }
                else // 32-bit
                {
                    samples = new float[dataSize / 4];
                    Buffer.BlockCopy(audioBytes, 0, samples, 0, dataSize);
                }

                AudioClip audioClip = AudioClip.Create("TTS_Audio", samples.Length / numChannels, numChannels, sampleRate, false);
                audioClip.SetData(samples, 0);
                return audioClip;
            }
        }
    }
}