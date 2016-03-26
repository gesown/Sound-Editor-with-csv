using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using NAudio.Wave;

namespace Sound_Editor {
    public class SEWaveViewer : System.Windows.Forms.UserControl {
        public Color penColor { get; set; }
        public float PenWidth { get; set; }

        private System.ComponentModel.Container components = null;

        private WaveStream waveStream;  // WaveFileReader or MP3FileReader
        private AudioFile audio;
        private int samplesPerPixel = 128;
        private long startPosition;
        private int bytesPerSample;

        public SEWaveViewer() {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.DoubleBuffered = true;

            this.penColor = Color.DodgerBlue;
            this.PenWidth = 1;
        }

        public void FitToScreen() {
            if (waveStream == null) return;
            int samples = (int)(waveStream.Length / bytesPerSample);
            startPosition = 0;
            SamplesPerPixel = samples / this.Width;
        }

        public void Zoom(int leftSample, int rightSample) {
            startPosition = leftSample * bytesPerSample;
            SamplesPerPixel = (rightSample - leftSample) / this.Width;
        }

        private Point mousePos, startPos;
        private bool mouseDrag = false;

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                startPos = e.Location;
                WaveStream.Position = StartPosition + startPos.X * bytesPerSample * samplesPerPixel;
                MainForm.originalPosition.CurrentTime = WaveStream.CurrentTime;
                mousePos = new Point(-1, -1);
                mouseDrag = true;
                DrawVerticalLine(e.X);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (mouseDrag) {
                DrawVerticalLine(e.X);
                if (mousePos.X != -1) {
                    DrawVerticalLine(mousePos.X);
                }
                mousePos = e.Location;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (mouseDrag && e.Button == MouseButtons.Left) {
                mouseDrag = false;
                DrawVerticalLine(startPos.X);
                if (mousePos.X == -1) return;
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

        public AudioFile Audio
        {
            get
            {
                return audio;
            }
            set
            {
                audio = value;
                if (audio.Format == "mp3") {
                    MP3File file = audio as MP3File;
                    WaveStream = file.Reader;
                } else if (audio.Format == "wav") {
                    WaveFile file = audio as WaveFile;
                    WaveStream = file.Reader;
                }
            }
        }

        public WaveStream WaveStream
        {
            get
            {
                return waveStream;
            }
            set
            {
                waveStream = value;
                if (waveStream != null) {
                    bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8) * waveStream.WaveFormat.Channels;
                }
                this.Invalidate();
            }
        }

        public int SamplesPerPixel
        {
            get
            {
                return samplesPerPixel;
            }
            set
            {
                samplesPerPixel = Math.Max(1, value);
                this.Invalidate();
            }
        }

        public long StartPosition
        {
            get
            {
                return startPosition;
            }
            set
            {
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