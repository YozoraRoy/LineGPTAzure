namespace LineGPTAzureFunctions.Helper
{
    using LineGPTAzureFunctions.Audio;
    using NAudio.Wave;
    using System.IO;

    public class AudioConverter
    {
        public void ByteToM4AToWAV(byte[] audioData, string m4aFilePath, string wavFilePath)
        {
            if (!File.Exists(m4aFilePath))
            {
                File.WriteAllBytes(m4aFilePath, audioData);
            }

            //if (!File.Exists(wavFilePath))
            //{
            //    using (var reader = new MediaFoundationReader(m4aFilePath))
            //    {
            //        WaveFileWriter.CreateWaveFile(wavFilePath, reader);
            //    }
            //}
        }

        public void ConvertToM4A(byte[] audioData, string outputPath)
        {
            File.WriteAllBytes(outputPath, audioData);
        }

        //public void ConvertM4AToWAV(string m4aFilePath, string wavFilePath)
        //{
        //    using (var reader = new MediaFoundationReader(m4aFilePath))
        //    {
        //        WaveFileWriter.CreateWaveFile(wavFilePath, reader);
        //    }
        //}

        public MemoryStream ConvertM4aStreamToWavStream(byte[] bytesResult)
        {
            string resutle = string.Empty;
 
            MemoryStream memoryStream = new MemoryStream(bytesResult);

            // Azure cognitive speech  
            Speech speech = new Speech();

            using (var m4aStream = new MemoryStream(bytesResult))
            {
                using (var reader = new StreamMediaFoundationReader(m4aStream))
                {

                    //   NAudio.Wave.WaveStream to MemoryStream
                    MemoryStream wavStream = new MemoryStream();
                    using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        waveStream.CopyTo(wavStream);
                    }
                    wavStream.Position = 0;
                    //log.LogInformation($"wavStream-Success");
                    //resutle = await speech.StreamToText(wavStream);
                    return wavStream;
                }
            }

        }



    }
}
