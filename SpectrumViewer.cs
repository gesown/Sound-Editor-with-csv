using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sound_Editor {
    public class SpectrumViewer : System.Windows.Forms.UserControl {
        public Color PenColor { get; set; }
        public int PenWidth { get; set; }

        public double[] spectrum { get; set; }
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

        protected override void OnPaint(PaintEventArgs e) {
            if (this.spectrum != null) {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen linePen = new Pen(this.PenColor, this.PenWidth);
                linePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                float max = (float)this.spectrum[1];
                for (int i = 2; i < this.spectrum.Length; i++) {
                    if (spectrum[i] > max)
                        max = (float)spectrum[i];
                }
                float koef = this.Height / Math.Max(max, 1);
                float step = (float)(this.Width) / spectrum.Length;
                float x = e.ClipRectangle.X;
                float y = (float)this.Height;
                float x1, y1;
                for (int i = 1; i < this.spectrum.Length; i++) {
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
