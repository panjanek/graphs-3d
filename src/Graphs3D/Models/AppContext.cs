using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Gpu;
using Graphs3D.Graphs;
using Graphs3D.Gui;

namespace Graphs3D.Models
{
    public class AppContext
    {
        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public ConfigWindow configWindow;

        public void StartNewGraph(IGraph graph)
        {
            lock (this)
            {
                simulation.StartNewGraph(graph);
                renderer.UploadGraph();
                renderer.ResetOrigin();
            }
        }
    }
}
