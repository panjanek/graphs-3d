using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Navigation;
using Graphs3D.Models;

namespace Graphs3D.Graphs
{
    public class Lattice : IGraph
    {
        private LatticeNode root;

        private int sizeX;

        private int sizeY;

        private bool wrapHorizontal;

        private bool wrapVertical;

        private Dictionary<string, LatticeNode> keyedNodes = new Dictionary<string, LatticeNode>();

        private HashSet<string> existingEdges = new HashSet<string>();

        private List<LatticeNode> lattice = new List<LatticeNode>();

        private List<Node> nodes = new List<Node>();

        private List<Edge> edges = new List<Edge>();

        public List<Node> Nodes => nodes;

        public List<Edge> Edges => edges;

        public Lattice(int sizeX, int sizeY, bool wrapHorizontal, bool wrapVertical)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.wrapVertical = wrapVertical;
            this.wrapHorizontal = wrapHorizontal;
            this.root = new LatticeNode();
            AddNode(this.root);
        }
        public int? ExpandNode(int? idx)
        {
            int parentIdx = 0;
            if (!idx.HasValue)
            {
                var l = lattice.Where(n => !n.expanded).OrderBy(n => n.level).FirstOrDefault();
                if (l == null)
                    return null;
                parentIdx = (int)l.idx;
            }
            else
            {
                parentIdx = idx.Value;
            }

            if (parentIdx < nodes.Count)
            {
                var parent = lattice[parentIdx];
                if (!parent.expanded)
                {
                    TryAdd(parent, -1, 0);
                    TryAdd(parent, 1, 0);
                    TryAdd(parent, 0, -1);
                    TryAdd(parent, 0, 1);
                    parent.expanded = true;
                }
            }

            return parentIdx;
        }

        private void TryAdd(LatticeNode parent, int dx, int dy)
        {
            int nx = parent.posX + dx;
            int ny = parent.posY + dy;
            if (wrapHorizontal) nx = (nx + sizeX) % sizeX;
            if (wrapVertical) ny = (ny + sizeY) % sizeY;
            if (nx < 0) return;
            if (nx >= sizeX) return;
            if (ny < 0) return;
            if (ny >= sizeY) return;
            var newKey = LatticeNode.GetNodeKey(nx, ny);
            if (keyedNodes.TryGetValue(newKey, out var existing))
            {
                if (!EdgeExists(parent.idx, existing.idx))
                    AddEdge(parent, existing);
            }
            else
            {
                var newNode = new LatticeNode() { posX = nx, posY = ny, level = parent.level + 1, parent = parent };
                AddNode(newNode);
                AddEdge(parent, newNode);
            }
        }

        public Node GetRoot()
        {
            return root.ToNode();
        }

        private void AddNode(LatticeNode node)
        {
            var count = nodes.Count;
            node.idx = (uint)count;
            keyedNodes[node.key] = node;
            nodes.Add(node.ToNode());
            lattice.Add(node);
            if (node.parent != null)
                node.parent.children.Add(node);
        }

        private void AddEdge(LatticeNode from, LatticeNode to)
        {
            var edge = new Edge() { a = from.idx, b = to.idx };
            edges.Add(edge);
            existingEdges.Add($"{edge.a}-{edge.b}");
        }

        private bool EdgeExists(uint a, uint b) => existingEdges.Contains($"{a}-{b}");
    }

    public class LatticeNode
    {
        public uint idx;
        public int posX;
        public int posY;
        public int level;
        public bool expanded;
        public LatticeNode parent;
        public List<LatticeNode> children = new List<LatticeNode>();
        public string key => GetNodeKey(posX, posY);

        public static string GetNodeKey(int x, int y) => $"{x},{y}";

        public Node ToNode()
        {
            return new Node() { level = level, player = 0 };
        }
    }
}
