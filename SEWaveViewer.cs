using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using NAudio.Wave;

namespace Sound_Editor {
    public class SEWaveViewer : System.Windows.Forms.UserControl {
        public SpectrogramViewer Spectrogram { get; set; }
        public SpectrumViewer Spectrum { get; set; }
        public Color penColor { get; set; }
        public float PenWidth { get; set; }

        private System.ComponentModel.Container components = null;

        private WaveStream waveStream;  // WaveFileReader or MP3FileReader
        private AudioFile audio;
        private int samplesPerPixel = 128;
        private long startPosition;
        private int bytesPerSample;
        private double millisecondsPerSample;

        public SEWaveViewer() {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.DoubleBuffered = true;

            this.penColor = Color.DodgerBlue;
            this.PenWidth = 1;
        }

        public void FitToScreen() {
            if (waveStream == null || this.Width == 0) return;
            int samples = (int)(waveStream.Length / bytesPerSample);
            startPosition = 0;
            SamplesPerPixel = samples / this.Width;
            millisecondsPerSample = samples / audio.Duration.TotalMilliseconds;
            MainForm.viewPeriod.StartTime = new TimeSpan(0);
            MainForm.viewPeriod.EndTime = new TimeSpan(0,0,0,0, (int)(samples / millisecondsPerSample));
            Spectrogram.StartPosition = 0;
            Spectrogram.Count = (int)(this.WaveStream.Length / 2) / 1024;
        }

        public void Zoom(int leftSample, int rightSample) {
            startPosition = leftSample * bytesPerSample;
            int samples = (rightSample - leftSample);
            SamplesPerPixel = samples / this.Width;
            if (this.inverseMouseDrag) {
                MainForm.viewPeriod.StartTime = new TimeSpan(0, 0, 0, 0, (int)(startPosition / bytesPerSample / millisecondsPerSample));
                MainForm.viewPeriod.EndTime = clickPosition;
            } else {
                MainForm.viewPeriod.StartTime = clickPosition;
                MainForm.viewPeriod.EndTime = new TimeSpan(0, 0, 0, 0, (int)(startPosition / bytesPerSample / millisecondsPerSample + samples / millisecondsPerSample));
            }
            // Указываем контролу спектрограммы начальный сэмпл и их количество для отображения
            Spectrogram.StartPosition = startPosition / 2;
            Spectrogram.Count = (samples * 2) / 1024;
        }

        private Point mousePos, startPos;
        private bool mouseDrag = false;
        private bool inverseMouseDrag = false;
        private TimeSpan clickPosition = new TimeSpan();

        protected override void OnMouseDown(MouseEventArgs e) {
            if (this.WaveStream == null) return;
            if (e.Button == MouseButtons.Left) {
                startPos = e.Location;
                if (startPos.X < 0 || startPos.X > this.Width) return;
                WaveStream.Position = StartPosition + startPos.X * bytesPerSample * samplesPerPixel;
                MainForm.originalPosition.CurrentTime = WaveStream.CurrentTime;
                clickPosition = new TimeSpan(WaveStream.CurrentTime.Days, WaveStream.CurrentTime.Hours, WaveStream.CurrentTime.Minutes, WaveStream.CurrentTime.Seconds, WaveStream.CurrentTime.Milliseconds);
                mousePos = new Point(-1, -1);
                mouseDrag = true;
                DrawVerticalLine(e.X);
                this.Spectrum.Refresh();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;
            if (mouseDrag && e.Location.X <= this.Width && e.Location.X >= 0) {
                DrawVerticalLine(e.X);
                if (mousePos.X != -1) {
                    DrawVerticalLine(mousePos.X);
                }
                mousePos = e.Location;
                int endMilliseconds = 0;
                if (mousePos.X <= startPos.X) {
                    WaveStream.Position = StartPosition + mousePos.X * bytesPerSample * samplesPerPixel;
                    MainForm.originalPosition.CurrentTime = WaveStream.CurrentTime;
                    endMilliseconds = (int)(StartPosition / bytesPerSample / millisecondsPerSample + startPos.X * samplesPerPixel / millisecondsPerSample);
                    MainForm.allocatedPeriod.StartTime = waveStream.CurrentTime;
                    MainForm.allocatedPeriod.EndTime = new TimeSpan(0, 0, 0, 0, endMilliseconds);
                } else {
                    MainForm.allocatedPeriod.StartTime = clickPosition;
                    endMilliseconds = (int)(startPosition / bytesPerSample / millisecondsPerSample + mousePos.X * samplesPerPixel / millisecondsPerSample);
                    TimeSpan current = new TimeSpan(0, 0, 0, 0, endMilliseconds);
                    MainForm.allocatedPeriod.EndTime = current;
                }                
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (mouseDrag && e.Button == MouseButtons.Left) {
                
                MainForm.allocatedPeriod.StartTime = new TimeSpan(0);
                MainForm.allocatedPeriod.EndTime = new TimeSpan(0);

                mouseDrag = false;
                DrawVerticalLine(startPos.X);
                if (mousePos.X == -1) return;
                this.inverseMouseDrag = (mousePos.X < startPos.X) ? true : false;
                DrawVerticalLine(mousePos.X);
                int leftSample = (int)(StartPosition / bytesPerSample + samplesPerPixel * Math.Min(startPos.X, mousePos.X));
                int rightSample = (int)(StartPosition / bytesPerSample + samplesPerPixel * Math.Max(startPos.X, mousePos.X));
                Zoom(leftSample, rightSample);
            } else if (e.Button == MouseButtons.Right) {
                this.FitToScreen();
            }
            base.OnMouseUp(e);
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            this.FitToScreen();
        }

        private void DrawVerticalLine(int x) {
            ControlPaint.DrawReversibleLine(PointToScreen(new Point(x, 0)), PointToScreen(new Point(x, Height)), Color.White);
        }

        public AudioFile Audio {
            get {
                return audio;
            }
            set {
                audio = value;
                if (audio != null) {
                    if (audio.Format == Enums.AudioFormats.MP3) {
                        MP3File file = audio as MP3File;
                        WaveStream = file.Reader;
                    } else if (audio.Format == Enums.AudioFormats.WAV) {
                        WaveFile file = audio as WaveFile;
                        WaveStream = file.Reader;
                    }
                }
            }
        }

        public WaveStream WaveStream {
            get {
                return waveStream;
            }
            set {
                waveStream = value;
                if (waveStream != null) {
                    bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8) * waveStream.WaveFormat.Channels;
                }
                this.Invalidate();
            }
        }

        public int SamplesPerPixel {
            get {
                return samplesPerPixel;
            }
            set {
                samplesPerPixel = Math.Max(1, value);
                this.Invalidate();
            }
        }

        public long StartPosition {
            get {
                return startPosition;
            }
            set {
                startPosition = value;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (waveStream != null) {
                int bytesToRead = samplesPerPixel * bytesPerSample;
                byte[] waveData = new byte[bytesToRead];
                long position = startPosition + (e.ClipRectangle.Left * bytesPerSample * samplesPerPixel);
                using (Pen linePen = new Pen(this.penColor, this.PenWidth)) {
                    for (float x = e.ClipRectangle.X; x < e.ClipRectangle.Right; x += 1) {
                        short low = 0;
                        short high = 0;
                        for (int i = 0; i < bytesToRead; i++) {
                            waveData[i] = Audio.Samples[position + i];
                        }
                        position += bytesToRead;
                        for (int n = 0; n < bytesToRead; n += 2) {
                            short sample = BitConverter.ToInt16(waveData, n);
                            if (sample < low) low = sample;
                            if (sample > high) high = sample;
                        }
                        float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                        float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);
                        e.Graphics.DrawLine(linePen, x, this.Height * lowPercent, x, this.Height * highPercent);
                    }
                }
            }
            base.OnPaint(e);
        }

        #region Component Designer generated code
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
        }
        #endregion
    }
}