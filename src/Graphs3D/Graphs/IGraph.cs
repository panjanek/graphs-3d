using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Graphs3D.Models;

namespace Graphs3D.Graphs
{
    public interface IGraph
    {
        int Expand();

        void ExpandNode(int parentIdx);

        bool IsFinished();

        List<Node> Nodes { get; }

        List<Edge> Edges { get; }

        void DrawPosition(int idx, Canvas canvas);
    }
}
