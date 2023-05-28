using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Helper
{
    using Microsoft.AspNetCore.Mvc;
    using NAudio.Wave;
    using System;
    using System.IO;
    using System.Media;

    public class AudioConverter
    {
        public void ByteToM4AToWAV(byte[] audioData, string m4aFilePath, string wavFilePath)
        {
            if (!File.Exists(m4aFilePath))
            {
                File.WriteAllBytes(m4aFilePath, audioData);
            }

            if (!File.Exists(wavFilePath))
            {
                using (var reader = new MediaFoundationReader(m4aFilePath))
                {
                    WaveFileWriter.CreateWaveFile(wavFilePath, reader);
                }
            }
        }


        public void ConvertToM4A(byte[] audioData, string outputPath)
        {
            File.WriteAllBytes(outputPath, audioData);
        }

        public void ConvertM4AToWAV(string m4aFilePath, string wavFilePath)
        {
            using (var reader = new MediaFoundationReader(m4aFilePath))
            {
                WaveFileWriter.CreateWaveFile(wavFilePath, reader);
            }
        }
    }
}
