using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Dsp;

namespace Sound_Editor {
    public class SpectrogramViewer : System.Windows.Forms.UserControl {
        public long StartPosition { get; set; }
        public TabPage Area { get; set; }
        public AudioFile Audio { get; set; }
        private Bitmap bitMap;
        private double max;
        private int count;
        public int Count {
            get {
                return count;
            }
            set {
                if (this.Area != null) {
                    this.count = value;
                    this.Area.AutoScrollMinSize = new Size(this.count, 512);
                    this.Width = this.count;
                    this.bitMap = null;
                    this.max = 0;
                }
            }
        }

        private System.ComponentModel.Container components = null;

        public SpectrogramViewer() {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(1, 19, 1);
        }

        private void findMax() {
            long position = 0;
            for (int i = 0; i < this.Audio.FloatSamples.Length / 1024; i++) {
                double[] spectrum = SpectrumViewer.getSpectrum(this.Audio, position);
                position += 1024;
                for (int j = 0; j < spectrum.Length; j++) {
                    if (spectrum[j] > this.max) {
                        this.max = spectrum[j];
                    }
                }
            }
        }

        private void drawBitMap() {
            if (this.max == 0) this.findMax();
            if (this.count == 0) throw new Exception();
            this.bitMap = new Bitmap(this.count, 512);
            long position = 0;
            position = this.StartPosition;
            double koef; int x = 0;
            for (int i = 0; i < this.count; i++, x++) {
                double[] spectrum = SpectrumViewer.getSpectrum(this.Audio, position);
                position += 1024;
                koef = 135 / max * 16;
                int color;
                for (int j = 0; j < spectrum.Length; j++) {
                    color = 20 + (int)(spectrum[j] * koef);
                    if (color > 255) color = 255;
                    this.bitMap.SetPixel(x, 512 - j - 1, Color.FromArgb(0, color, 0));
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (this.Audio != null) {
                if (this.bitMap == null) {
                    try {
                        this.drawBitMap();
                    } catch (Exception) {
                        base.OnPaint(e);
                        return;
                    }
                }
                e.Graphics.DrawImage(this.bitMap, new PointF(0, 0));
                if (this.count < this.Width) {
                    e.Graphics.DrawLine(new Pen(Color.White, 1), this.count + 1, 0, this.count + 1, this.Height);
                }
            }
            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
        }
        #endregion
    }
}
