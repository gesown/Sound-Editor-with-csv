using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;

namespace Sound_Editor {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private List<AudioFile> files = new List<AudioFile>();
        private BlockAlignReductionStream stream = null;
        private DirectSoundOut output = null;

        private void openToolStripButton_Click(object sender, EventArgs e) {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (open.ShowDialog() != DialogResult.OK) return;
            DisposeWave();

            if (open.FileName.EndsWith(".mp3")) {
                WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(open.FileName));
                stream = new BlockAlignReductionStream(pcm);
            } else if (open.FileName.EndsWith(".wav")) {
                WaveStream pcm = new WaveChannel32(new WaveFileReader(open.FileName));
                stream = new BlockAlignReductionStream(pcm);
            } else throw new InvalidOperationException("Неверный формат аудиофайла");

            AudioFile file = new AudioFile(stream, open.FileName);
            files.Add(file);
            ListViewItem item = new ListViewItem(file.Name);
            item.SubItems.Add(String.Format("{0:00}:{1:00}:{2:000}", file.Duration.Minutes, file.Duration.Seconds, file.Duration.Milliseconds));
            item.SubItems.Add(file.SampleRate.ToString() + " Hz");
            item.SubItems.Add(file.Format.ToString());
            item.SubItems.Add(file.Path.ToString());
            listAudio.Items.Add(item);

            output = new DirectSoundOut();
            output.Init(stream);
        }

        // Pause
        private void toolStripButton3_Click(object sender, EventArgs e) {
            if (output != null) {
                if (output.PlaybackState == PlaybackState.Playing) {
                    output.Pause();
                }
            }
        }

        // Play
        private void toolStripButton1_Click(object sender, EventArgs e) {
            if (output != null) {
                if (output.PlaybackState != PlaybackState.Playing) {
                    output.Play();
                }
            }
        }

        // Stop
        private void toolStripButton2_Click(object sender, EventArgs e) {
            if (output != null) {
                if (output.PlaybackState != PlaybackState.Stopped) {
                    stream.Position = 0;
                    output.Stop();
                }
            }
        }

        private void DisposeWave() {
            if (output != null) {
                if (output.PlaybackState == PlaybackState.Playing) {
                    output.Stop();
                }
                output.Dispose();
                output = null;
            }
            if (stream != null) {
                stream.Dispose();
                stream = null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            DisposeWave();
        }
    }
}
