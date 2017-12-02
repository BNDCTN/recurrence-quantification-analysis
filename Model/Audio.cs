using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Threading;

namespace Model
{
    static public class Audio
    {
        static public event Action<int, int> AudioProgress;

        public static float[] prepare(String wavePath)
        {
            float[] data;
            byte[] wave;
            System.IO.FileStream WaveFile = System.IO.File.OpenRead(wavePath);
            wave = new byte[WaveFile.Length];

            int step = (wave.Length - 44) / 4 / 400;

            data = new float[(wave.Length - 44) / 4 / step];           
            WaveFile.Read(wave, 0, Convert.ToInt32(WaveFile.Length));  
                                                                        
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (float)((BitConverter.ToInt32(wave, 44 + i * step * 4)) / 4294967296.0);
                AudioProgress?.Invoke(data.Length, i);
            }
            //if (data.Length>400) 
            /*
            using (MemoryStream ms = new MemoryStream(wave))
            {
                SoundPlayer player = new SoundPlayer(ms);
                player.Play();
            }
            */
            return data.Take(400).ToArray();
        }
    }
}
