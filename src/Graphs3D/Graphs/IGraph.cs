using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Models;

namespace Graphs3D.Graphs
{
    public interface IGraph
    {
        int Expand();

        void ExpandNode(int parentIdx);

        List<Node> Nodes { get; }

        List<Edge> Edges { get; }
    }
}
