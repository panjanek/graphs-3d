using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Gui
{
    public class AnimationTimer
    {
        private System.Timers.Timer timer;

        private Action<double> step;

        private Action finished;

        private DateTime start;

        private DateTime end;

        public AnimationTimer(double intervalMs, double totalMs, Action<double> step, Action finished)
        {
            this.step = step;
            this.finished = finished;
            start = DateTime.Now;
            end = start.AddMilliseconds(totalMs);
            timer = new System.Timers.Timer();
            timer.Interval = intervalMs;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            if (now >= end)
            {
                timer.Stop();
                finished();
                timer = null;
            }
            else
            {
                if (timer != null)
                {
                    double progress = (now - start).TotalMilliseconds / (end - now).TotalMilliseconds;
                    step(progress);
                }
            }
        }
    }
}
