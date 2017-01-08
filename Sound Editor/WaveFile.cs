using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Sound_Editor {
    class WaveFile : AudioFile {
        public WaveFileReader Reader { get; set; }

        public WaveFile(WaveFileReader reader, BlockAlignReductionStream stream, string path) : base(stream, path) {
            this.Reader = reader;
            this.Samples = new byte[this.Reader.Length];
            this.Channels = reader.WaveFormat.Channels;
            this.callRead();
        }

        protected override void readBytes() {
            this.Reader.Read(this.Samples, 0, (int)this.Reader.Length);
            this.Reader.Position = 0;
        }

        protected override void readShorts() {
            this.ShortSamples = new short[this.Reader.Length / 2];
            for (int i = 0; i < this.Reader.Length - 1; i++) {
                this.ShortSamples[i / 2] = (short)((this.Samples[i] << 8) | this.Samples[i + 1]);
            }
        }
    }
}