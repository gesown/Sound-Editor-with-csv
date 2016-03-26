using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sound_Editor {
    public class TimePeriod {
        private ListViewItem listView;
        private TimeSpan startTime;
        private TimeSpan endTime;
        private TimeSpan duration;

        public TimeSpan StartTime {
            get {
                return startTime;
            }
            set {
                startTime = value;
                listView.Text = Position.getTimeString(startTime);
            }
        }

        public TimeSpan EndTime {
            get {
                return endTime;
            }
            set {
                endTime = value;
                listView.SubItems[0].Text = Position.getTimeString(endTime);
                Duration = startTime.Subtract(endTime);
            }
        }

        public TimeSpan Duration {
            get {
                return duration;
            }
            set {
                duration = value;
                listView.SubItems[1].Text = Position.getTimeString(duration);
            }
        }

        public TimePeriod(ListViewItem listView) {
            this.listView = listView;
        }
    }
}
