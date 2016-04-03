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
            this.readBytes();
            this.readFloats();
        }

        private void readBytes() {
            this.Reader.Read(this.Samples, 0, (int)this.Reader.Length);
            this.Reader.Position = 0;
        }
    }
}