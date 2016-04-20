using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;

namespace Sound_Editor {
    public abstract class AudioFile {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public int SampleRate { get; set; }
        public int bitDepth { get; set; }
        public double Size { get; set; }
        public string Format { get; set; }
        public string Path { get; set; }
        public byte[] Samples { get; set; }
        public short[] ShortSamples { get; set; }
        public float[] FloatSamples { get; set; }
        public float Avg { get; set; }
        public BlockAlignReductionStream Stream { get; set; }

        public AudioFile(BlockAlignReductionStream stream, string path) {
            string tmpName, tmpFormat;
            AudioFile.getNameAndFormatFromPath(path, out tmpName, out tmpFormat);
            this.Name = tmpName;
            this.Duration = stream.TotalTime;
            this.SampleRate = stream.WaveFormat.SampleRate;
            this.bitDepth = stream.WaveFormat.BitsPerSample;
            this.Size = new FileInfo(path).Length * Math.Pow(10, -6);
            this.Format = tmpFormat;
            this.Path = path;
            this.Stream = stream;
        }

        protected abstract void readBytes();
        protected abstract void readShorts();

        public static void getNameAndFormatFromPath(string path, out string name, out string format) {
            int startIndexOfName = path.LastIndexOf('\\') + 1;
            int startIndexOfFormat = path.LastIndexOf('.') + 1;
            int nameLength = startIndexOfFormat - startIndexOfName - 1;
            name = path.Substring(startIndexOfName, nameLength);
            format = path.Substring(startIndexOfFormat);
        }

        protected void callRead() {
            this.readBytes();
            this.readShorts();
            this.getFloats();
        }

        private void getFloats() {
            this.FloatSamples = new float[this.ShortSamples.Length];
            for (int i = 0; i < this.FloatSamples.Length; i++) {
                this.FloatSamples[i] = this.ShortSamples[i] / 32768f;
            }
            this.getAvg();
        }

        private void getAvg() {
            this.Avg = 0;
            for (int i = 0; i < this.FloatSamples.Length; i++) {
                if (this.Avg < this.FloatSamples[i]) {
                    this.Avg = this.FloatSamples[i];
                }
            }
            this.Avg /= 32;
        }
    }
}
