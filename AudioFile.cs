using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Sound_Editor {
    class AudioFile {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Position { get; set; }
        public int SampleRate { get; set; }
        public int bitDepth { get; set; }
        public string Format { get; set; }
        public string Path { get; set; }

        public AudioFile(BlockAlignReductionStream stream, string path) {
            int startIndexOfName = path.LastIndexOf('\\') + 1;
            int startIndexOfFormat = path.LastIndexOf('.') + 1;
            int nameLength = startIndexOfFormat - startIndexOfName - 1;

            this.Name = path.Substring(startIndexOfName, nameLength);
            this.Duration = stream.TotalTime;
            this.Position = stream.CurrentTime;
            this.SampleRate = stream.WaveFormat.SampleRate;
            this.bitDepth = stream.WaveFormat.BitsPerSample;
            this.Format = path.Substring(startIndexOfFormat);
            this.Path = path;
        }
    }
}
