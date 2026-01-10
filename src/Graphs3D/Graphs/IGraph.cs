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

        bool ExpandNode(int parentIdx);

        bool IsFinished();

        List<Node> Nodes { get; }

        List<Edge> Edges { get; }

        bool DrawPosition(int idx, Canvas canvas);

        void Click(double x, double y);

        Action<int> NavigateTo { get; set; }
    }
}
