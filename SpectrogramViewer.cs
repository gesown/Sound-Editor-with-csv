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
                }
            }
        }

        private System.ComponentModel.Container components = null;

        public SpectrogramViewer() {
            InitializeComponent();
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
