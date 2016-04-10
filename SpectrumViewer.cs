using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Dsp;

namespace Sound_Editor {
    public class SpectrumViewer : System.Windows.Forms.UserControl {
        public Color PenColor { get; set; }
        public int PenWidth { get; set; }
        private double freq;
        private AudioFile audio;
        public AudioFile Audio {
            get {
                return this.audio;
            }
            set {
                if (value != null) {
                    this.audio = value;
                    freq = this.audio.SampleRate / 1024.0;
                }
            }
        }

        private System.ComponentModel.Container components = null;

        public SpectrumViewer() {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.PenColor = Color.Red;
            this.PenWidth = 2;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private double[] getSpectrum() {
            float[] buff = new float[1024];
            long position = 0;
            if (Audio.Format == "mp3") {
                MP3File file = Audio as MP3File;
                position = file.Reader.Position / 2;
            } else if (Audio.Format == "wav") {
                WaveFile file = Audio as WaveFile;
                position = file.Reader.Position / 2;
            }
            if (position + 1024 < Audio.FloatSamples.Length) {
                for (int i = 0; i < 1024; i++) {
                    buff[i] = Audio.FloatSamples[position + i];
                }
            }

            Complex[] data = new Complex[1024];
            for (int i = 0; i < 1024; i++) {
                data[i].X = buff[i];
                data[i].Y = 0.0f;
            }
            FastFourierTransform.FFT(true, 10, data);

            double[] res = new double[512];
            for (int i = 0; i < 512; i++) {
                res[i] = Math.Sqrt(data[i].X * data[i].X + data[i].Y * data[i].Y);
            }
            return res;
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (this.Audio != null) {
                double[] spectrum = this.getSpectrum();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen linePen = new Pen(this.PenColor, this.PenWidth);
                linePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                float step = (float)(this.Width - 20) / spectrum.Length;

                // Отрисовка шкалы по оси X
                e.Graphics.DrawLine(Pens.White, 0, this.Height - 20, this.Width, this.Height - 20);
                e.Graphics.DrawString("kHz", new Font(FontFamily.GenericSansSerif, 7.5f), Brushes.White, 0, this.Height - 17);
                int[] freqPointsPercents = { 5, 10, 20, 25, 40, 50, 60, 75, 80, 90, 95 };
                float freqPoint;
                for (int i = 0; i < freqPointsPercents.Length; i++) {
                    freqPoint = 20 + (freqPointsPercents[i] * (this.Width - 20) / 100f);
                    e.Graphics.DrawLine(new Pen(Color.Gray, 1f), freqPoint, 0, freqPoint, this.Height - 20);
                    double sample = (spectrum.Length * freqPointsPercents[i]) / 100.0;
                    string currentFreq = ((sample * freq) * Math.Pow(10, -3)).ToString("0.000");
                    e.Graphics.DrawString(currentFreq, new Font(FontFamily.GenericSansSerif, 7.5f), Brushes.White, freqPoint - 15, this.Height - 17);
                }

                // Отрисовка шкалы по оси Y
                e.Graphics.DrawLine(Pens.White, 20, 0, 20, this.Height);
                int[] gradePointsPercents = { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
                int gradePoint;
                double currentGrade;
                for (int i = 0; i < gradePointsPercents.Length; i++) {
                    gradePoint = gradePointsPercents[i] * (this.Height - 20) / 100;
                    e.Graphics.DrawLine(new Pen(Color.Gray, 1f), 20, gradePoint, this.Width, gradePoint);
                    currentGrade = (this.Height - 20 - gradePoint) * this.Audio.Avg / (this.Height - 20);
                    currentGrade *= 10000;
                    currentGrade = Math.Round(currentGrade);
                    e.Graphics.DrawString(currentGrade.ToString(), new Font(FontFamily.GenericSansSerif, 7f), Brushes.White, 0, gradePoint - 5);
                }

                // Отрисовка спектра
                float koef = this.Height / this.Audio.Avg;
                
                float x = e.ClipRectangle.X + 20;
                float y = (float)(this.Height - 20);
                float x1, y1;
                for (int i = 1; i < spectrum.Length; i++) {
                    x1 = x + step;
                    y1 = (this.Height - 20) - (float)(spectrum[i] * koef);
                    e.Graphics.DrawLine(linePen, x, y, x1, y1);
                    x = x1; y = y1;
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
