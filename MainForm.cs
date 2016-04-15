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

        public static Position originalPosition = null;
        public static TimePeriod allocatedPeriod = null;
        public static TimePeriod viewPeriod = null;

        private List<AudioFile> files = new List<AudioFile>();
        private AudioFile currentAudio = null;
        private WaveOut output = null;

        private WaveIn sourceStream = null;
        private WaveFileWriter waveWriter = null;

        private void MainForm_Load(object sender, EventArgs e) {
            spectrumViewer.PenColor = Color.GreenYellow;
            spectrumViewer.PenWidth = 2;

            originalSpectrogramViewer.Area = originalVizualizationTab.TabPages[2];

            originalPosition = new Position(originalCurrentTime);
            originalPosition.CurrentTime = new TimeSpan(0);

            allocatedPeriod = new TimePeriod(timePeriods.Items[0]);
            allocatedPeriod.StartTime = new TimeSpan(0);
            allocatedPeriod.EndTime = new TimeSpan(0);

            viewPeriod = new TimePeriod(timePeriods.Items[2]);
            viewPeriod.StartTime = new TimeSpan(0);
            viewPeriod.EndTime = new TimeSpan(0);
        }

        private void initAudio(AudioFile f) {
            currentAudio = f;

            originalSpectrogramViewer.Audio = f;

            originalWaveViewer.Spectrogram = originalSpectrogramViewer;
            originalWaveViewer.Audio = f;
            originalWaveViewer.FitToScreen();

            spectrumViewer.Audio = f;

            originalVizualizationTab.TabPages[0].Text = "Редактор: " + f.Name + "." + f.Format;
            audioRate.Text = f.SampleRate + " Hz";
            audioSize.Text = Math.Round(f.Size, 1).ToString() + " MB";
            audioLength.Text = Position.getTimeString(f.Duration);
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
            item.SubItems.Add(Position.getTimeString(file.Duration));
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
                    originalPlayTimer.Enabled = false;
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
                    originalPlayTimer.Enabled = true;
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
                    originalPlayTimer.Enabled = false;
                    originalPosition.CurrentTime = new TimeSpan(0);
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
            //DisposeWave();
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

            originalPosition.CurrentTime = currentTime;
        }

        private void trackBarOriginal_Scroll(object sender, EventArgs e) {
            if (output != null) {
                output.Volume = trackBarOriginal.Value / 10f;
            }
        }

        private void spectrumTimer_Tick(object sender, EventArgs e) {
            spectrumViewer.Refresh();
        }

        /* Методы обработки входного сигнала */
        private string saveFileName; // tmp variable
        // Создание пустого wav файла на диске
        private void newToolStripButton_Click(object sender, EventArgs e) {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Wave File (*.wav)|*.wav;";
            if (save.ShowDialog() != DialogResult.OK) return;
            this.saveFileName = save.FileName;  // tmp operation
        }

        // Обновление списка доступных записывающих устройств
        private void refreshDeviceListButton_Click(object sender, EventArgs e) {
            List<WaveInCapabilities> sources = new List<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; i++) {
                sources.Add(WaveIn.GetCapabilities(i));
            }
            devicesListView.Items.Clear();
            foreach (var source in sources) {
                ListViewItem item = new ListViewItem(source.ProductName);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, source.Channels.ToString()));
                devicesListView.Items.Add(item);
            }
        }

        private void startRecordButton_Click(object sender, EventArgs e) {
            if (devicesListView.SelectedItems.Count == 0) return;
            int deviceNumber = devicesListView.SelectedItems[0].Index;
            this.sourceStream = new WaveIn();
            this.sourceStream.DeviceNumber = deviceNumber;
            this.sourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);

            this.sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(SourceStream_DataAvailable);
            this.waveWriter = new WaveFileWriter(this.saveFileName, this.sourceStream.WaveFormat);

            this.sourceStream.StartRecording();
        }

        private void SourceStream_DataAvailable(object sender, WaveInEventArgs e) {
            if (this.waveWriter == null) return;
            this.waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            this.waveWriter.Flush();
        }

        private void stopRecordButton_Click(object sender, EventArgs e) {
            if (this.sourceStream != null) {
                this.sourceStream.StopRecording();
                this.sourceStream.Dispose();
                this.sourceStream = null;
            }
            if (this.waveWriter != null) {
                this.waveWriter.Dispose();
                this.waveWriter = null;
            }
        }
    }
}
