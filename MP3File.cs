using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Sound_Editor {
    class MP3File : AudioFile {
        public override dynamic Reader { get; set; }

        public MP3File(Mp3FileReader reader, BlockAlignReductionStream stream, string path) : base(stream, path) {
            this.Reader = reader;
        }
    }
}