using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Graphs3D.Models;

namespace Graphs3D.Graphs
{
    public abstract class GraphBase
    {
        public List<Node> Nodes => nodes;

        public List<Edge> Edges => edges;

        private List<Node> nodes = new List<Node>();

        private List<Edge> edges = new List<Edge>();

        private HashSet<string> existingEdges = new HashSet<string>();

        protected void AddResultNode(Node n) => nodes.Add(n);

        protected void AddResultEdge(int parentIdx, int childIdx, int player = 0)
        {
            edges.Add(new Edge() { a = (uint)parentIdx, b = (uint)childIdx, player = (uint)player });
            existingEdges.Add($"{parentIdx}-{childIdx}");
        }

        protected bool EdgeExists(int a, int b) => existingEdges.Contains($"{a}-{b}");

    }
}
