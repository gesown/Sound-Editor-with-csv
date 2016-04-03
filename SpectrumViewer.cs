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
        public AudioFile Audio { get; set; }

        private System.ComponentModel.Container components = null;

        public SpectrumViewer() {
            InitializeComponent();

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
                float koef = this.Height / this.Audio.Avg;
                float step = (float)(this.Width) / spectrum.Length;
                float x = e.ClipRectangle.X;
                float y = (float)this.Height;
                float x1, y1;
                for (int i = 1; i < spectrum.Length; i++) {
                    x1 = x + step;
                    y1 = this.Height - (float)(spectrum[i] * koef);
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
