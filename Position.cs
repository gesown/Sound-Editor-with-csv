using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sound_Editor {
    public class Position {
        private ToolStripLabel currentTimeLabel;
        private TimeSpan currentTime;

        public TimeSpan CurrentTime {
            get {
                return currentTime;
            }
            set {
                currentTime = value;
                currentTimeLabel.Text = getTimeString(currentTime);
            }
        }

        public Position(ToolStripLabel label) {
            this.currentTimeLabel = label;
        }

        public static string getTimeString(TimeSpan time) {
            return String.Format("{0:00}:{1:00}:{2:000}", time.Minutes, time.Seconds, time.Milliseconds);
        }
    }
}
