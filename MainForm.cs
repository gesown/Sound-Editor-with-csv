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
using NAudio.Dsp;

namespace Sound_Editor {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private List<AudioFile> files = new List<AudioFile>();
        private AudioFile currentAudio = null;
        private WaveOut output = null;

        private void MainForm_Load(object sender, EventArgs e) {
            originalPlayTimer.Enabled = true;
            spectrumViewer.PenColor = Color.GreenYellow;
            spectrumViewer.PenWidth = 2;
        }

        private void initAudio(AudioFile f) {
            currentAudio = f;

            originalWaveViewer.Audio = f;
            originalWaveViewer.FitToScreen();

            spectrumViewer.Audio = f;

            VizualizationTab.TabPages[0].Text = "Редактор: " + f.Name + "." + f.Format;
            audioRate.Text = f.SampleRate + " Hz";
            audioSize.Text = Math.Round(f.Size, 1).ToString() + " MB";
            audioLength.Text = String.Format("{0:00}:{1:00}:{2:00}", f.Duration.Minutes, f.Duration.Seconds, f.Duration.Milliseconds);
            output.Init(f.Stream);
        }

        private void openToolStripButton_Click(object sender, EventArgs e) {
            AudioFile file = null;
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (open.ShowDialog() != DialogResult.OK) return;
            if (open.FileName.EndsWith(".mp3")) {
                Mp3FileReader reader = new Mp3FileReader(open.FileName);
                WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(reader);
                BlockAlignReductionStream stream = new BlockAlignReductionStream(pcm);
                file = new MP3File(reader, stream, open.FileName);
            } else if (open.FileName.EndsWith(".wav")) {
                WaveFileReader reader = new WaveFileReader(open.FileName);
                WaveStream pcm = new WaveChannel32(reader);
                BlockAlignReductionStream stream = new BlockAlignReductionStream(pcm);
                file = new WaveFile(reader, stream, open.FileName);
            } else throw new InvalidOperationException("Неверный формат аудиофайла");
            files.Add(file);
            ListViewItem item = new ListViewItem(file.Name);
            item.SubItems.Add(String.Format("{0:00}:{1:00}:{2:000}", file.Duration.Minutes, file.Duration.Seconds, file.Duration.Milliseconds));
            item.SubItems.Add(file.SampleRate.ToString() + " Hz");
            item.SubItems.Add(file.Format.ToString());
            item.SubItems.Add(file.Path.ToString());
            item.SubItems.Add(file.bitDepth.ToString() + " bit");
            listAudio.Items.Add(item);
            if (files.Count == 1) {
                output = new WaveOut();
                output.Volume = 1f;
                this.initAudio(file);
            }
        }

        private void редактироватьToolStripMenuItem_Click(object sender, EventArgs e) {
            if (listAudio.SelectedItems.Count > 0) {
                AudioFile file = files.Find(audio => audio.Name == listAudio.SelectedItems[0].Text && audio.Format == listAudio.SelectedItems[0].SubItems[3].Text);
                if (output.PlaybackState == PlaybackState.Playing) {
                    output.Pause();
                    audioStatus.Text = "Приостановлено: " + currentAudio.Name + "." + currentAudio.Format;
                }
                this.initAudio(file);
            } else {
                MessageBox.Show("Вы не выбрали аудио файл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Pause
        private void toolStripButton3_Click(object sender, EventArgs e) {
            if (output != null) {
                if (output.PlaybackState == PlaybackState.Playing) {
                    output.Pause();
                    spectrumTimer.Enabled = false;
                    audioStatus.Text = "Приостановлено: " + currentAudio.Name + "." + currentAudio.Format;
                }
            }
        }

        // Play
        private void toolStripButton1_Click(object sender, EventArgs e) {
            if (output != null) {
                if (output.PlaybackState != PlaybackState.Playing) {
                    output.Play();
                    spectrumTimer.Enabled = true;
                    audioStatus.Text = "Воспроизведение: " + currentAudio.Name + "." + currentAudio.Format;
                }
            }
        }

        // Stop
        private void toolStripButton2_Click(object sender, EventArgs e) {
            if (output != null) {
                if (output.PlaybackState != PlaybackState.Stopped) {
                    currentAudio.Stream.Position = 0;
                    output.Stop();
                    originalCurrentTime.Text = "00:00:000";
                    spectrumTimer.Enabled = false;
                    audioStatus.Text = "Остановлено: " + currentAudio.Name + "." + currentAudio.Format;
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
            if (currentAudio.Stream != null) {
                currentAudio.Stream.Dispose();
                currentAudio.Stream = null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            DisposeWave();
        }

        private void originalPlayTimer_Tick(object sender, EventArgs e) {
            if (currentAudio == null) return;
            TimeSpan currentTime = new TimeSpan();
            if (currentAudio.Format == "mp3") {
                MP3File file = currentAudio as MP3File;
                currentTime = file.Reader.CurrentTime;
            } else if (currentAudio.Format == "wav") {
                WaveFile file = currentAudio as WaveFile;
                currentTime = file.Reader.CurrentTime;
            }
            if (currentAudio.Duration == currentTime) {
                toolStripButton2_Click(sender, e);
            }

            originalCurrentTime.Text = String.Format("{0:00}:{1:00}:{2:00}", currentTime.Minutes, currentTime.Seconds, currentTime.Milliseconds);
        }

        private void trackBarOriginal_Scroll(object sender, EventArgs e) {
            if (output != null) {
                output.Volume = trackBarOriginal.Value / 10f;
            }
        }

        private void spectrumTimer_Tick(object sender, EventArgs e) {
            spectrumViewer.Refresh();
        }
    }
}
