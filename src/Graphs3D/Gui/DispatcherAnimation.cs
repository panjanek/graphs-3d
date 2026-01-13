using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Graphs3D.Gui
{
    public class DispatcherAnimation
    {
        private DispatcherTimer timer;

        private Action action;
        public DispatcherAnimation(int ms, Action step) 
        {
            this.action = step;
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(ms) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (timer.IsEnabled)
                action();
        }

        public void Stop()
        {
            timer.Stop();
        }
    }
}
