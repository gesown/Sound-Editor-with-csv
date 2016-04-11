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
    public enum ViewState {
        DEFAULT, LOGARITHM
    }

    public class SpectrumViewer : System.Windows.Forms.UserControl {
        public Color PenColor { get; set; }
        public int PenWidth { get; set; }
        private double freq;
        private ViewState state;
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
            this.state = ViewState.DEFAULT;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public static double[] getSpectrum(AudioFile audio, long startPos) {
            float[] buff = new float[1024];
            if (startPos + 1024 < audio.FloatSamples.Length) {
                for (int i = 0; i < 1024; i++) {
                    buff[i] = audio.FloatSamples[startPos + i];
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

        private void logarithmSpectrum(double[] spectrum) {
            for (int i = 0; i < spectrum.Length; i++) {
                spectrum[i] = 20 * Math.Log10(spectrum[i]);
            }
        }

        protected override void OnClick(EventArgs e) {
            MouseEventArgs args = (MouseEventArgs)e;
            if (args.Button != MouseButtons.Left) return;
            if (this.state == ViewState.DEFAULT) {
                this.state = ViewState.LOGARITHM;
            } else {
                this.state = ViewState.DEFAULT;
            }
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (this.Audio != null) {
                long position = 0;
                if (audio.Format == "mp3") {
                    MP3File file = audio as MP3File;
                    position = file.Reader.Position / 2;
                } else if (audio.Format == "wav") {
                    WaveFile file = audio as WaveFile;
                    position = file.Reader.Position / 2;
                }

                double[] spectrum = SpectrumViewer.getSpectrum(this.Audio, position);
                if (this.state == ViewState.LOGARITHM) {
                    this.logarithmSpectrum(spectrum);
                }

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen linePen = new Pen(this.PenColor, this.PenWidth);
                linePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                float step = (float)(this.Width - 20) / spectrum.Length;

                // Отрисовка шкалы по оси X
                int yLinePos = (this.state == ViewState.DEFAULT) ? this.Height - 20 : 20;
                int yStringPos = (this.state == ViewState.DEFAULT) ? yLinePos + 3 : 3;
                e.Graphics.DrawLine(Pens.White, 0, yLinePos, this.Width, yLinePos);
                e.Graphics.DrawString("kHz", new Font(FontFamily.GenericSansSerif, 7.5f), Brushes.White, 0, yStringPos);
                int[] freqPointsPercents = { 5, 10, 20, 25, 40, 50, 60, 75, 80, 90, 95 };
                float freqPoint;
                for (int i = 0; i < freqPointsPercents.Length; i++) {
                    freqPoint = 20 + (freqPointsPercents[i] * (this.Width - 20) / 100f);
                    if (this.state == ViewState.DEFAULT) {
                        e.Graphics.DrawLine(new Pen(Color.Gray, 1f), freqPoint, 0, freqPoint, this.Height - 20);
                    } else {
                        e.Graphics.DrawLine(new Pen(Color.Gray, 1f), freqPoint, 20, freqPoint, this.Height);
                    }
                    double sample = (spectrum.Length * freqPointsPercents[i]) / 100.0;
                    string currentFreq = ((sample * freq) * Math.Pow(10, -3)).ToString("0.000");
                    e.Graphics.DrawString(currentFreq, new Font(FontFamily.GenericSansSerif, 7.5f), Brushes.White, freqPoint - 15, yStringPos);
                }

                // Отрисовка шкалы по оси Y
                e.Graphics.DrawLine(Pens.White, 20, 0, 20, this.Height);
                if (this.state == ViewState.LOGARITHM) {
                    e.Graphics.DrawString("dB", new Font(FontFamily.GenericSansSerif, 7f), Brushes.White, 2, this.Height - 15);
                }
                int[] gradePointsPercents = { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
                int gradePoint;
                double currentGrade;
                for (int i = 0; i < gradePointsPercents.Length; i++) {
                    gradePoint = gradePointsPercents[i] * (this.Height - 20) / 100;
                    gradePoint = (this.state == ViewState.LOGARITHM) ? gradePoint + 20 : gradePoint;
                    e.Graphics.DrawLine(new Pen(Color.Gray, 1f), 20, gradePoint, this.Width, gradePoint);
                    if (this.state == ViewState.DEFAULT) {
                        currentGrade = (this.Height - 20 - gradePoint) * this.Audio.Avg / (this.Height - 20);
                        currentGrade *= 10000;
                        currentGrade = Math.Round(currentGrade);
                    } else {
                        currentGrade = Math.Round(gradePoint / -1.5);
                    }
                    e.Graphics.DrawString(currentGrade.ToString(), new Font(FontFamily.GenericSansSerif, 7f), Brushes.White, -1, gradePoint - 6);
                }

                // Отрисовка спектра
                float koef = (this.state == ViewState.DEFAULT) ? (this.Height - 20) / this.Audio.Avg : 1.5f;
                
                float x = e.ClipRectangle.X + 20;
                float y = (float)(this.Height - 20);
                y = (this.state == ViewState.LOGARITHM) ? 20 : y;
                float x1, y1;
                for (int i = 1; i < spectrum.Length; i++) {
                    x1 = x + step;
                    y1 = (this.state == ViewState.DEFAULT) ? (this.Height - 20) - (float)(spectrum[i] * koef) : 20 - (float)(spectrum[i] * koef);
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
