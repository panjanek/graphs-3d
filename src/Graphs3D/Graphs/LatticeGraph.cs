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
    public class LatticeGraph : GraphBase, IGraph
    {
        private LatticeState root;

        private int sizeX;

        private int sizeY;

        private bool wrapHorizontal;

        private bool wrapVertical;

        private Dictionary<string, LatticeState> keyedNodes = new Dictionary<string, LatticeState>();

        private List<LatticeState> lattice = new List<LatticeState>();

        public LatticeGraph(int sizeX, int sizeY, bool wrapHorizontal, bool wrapVertical)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.wrapVertical = wrapVertical;
            this.wrapHorizontal = wrapHorizontal;
            this.root = new LatticeState();
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

            if (parentIdx < Nodes.Count)
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

        private void TryAdd(LatticeState parent, int dx, int dy)
        {
            int nx = parent.posX + dx;
            int ny = parent.posY + dy;
            if (wrapHorizontal) nx = (nx + sizeX) % sizeX;
            if (wrapVertical) ny = (ny + sizeY) % sizeY;
            if (nx < 0) return;
            if (nx >= sizeX) return;
            if (ny < 0) return;
            if (ny >= sizeY) return;
            var newKey = LatticeState.GetNodeKey(nx, ny);
            if (keyedNodes.TryGetValue(newKey, out var existing))
            {
                if (!EdgeExists(parent.idx, existing.idx))
                    AddResultEdge(parent.idx, existing.idx);
            }
            else
            {
                var newNode = new LatticeState() { posX = nx, posY = ny, level = parent.level + 1, parent = parent };
                AddNode(newNode);
                AddResultEdge(parent.idx, newNode.idx);
            }
        }

        public Node GetRoot()
        {
            return root.ToNode();
        }

        private void AddNode(LatticeState node)
        {
            var count = Nodes.Count;
            node.idx = (uint)count;
            keyedNodes[node.key] = node;
            AddResultNode(node.ToNode());
            lattice.Add(node);
            if (node.parent != null)
                node.parent.children.Add(node);
        }
    }

    public class LatticeState
    {
        public uint idx;
        public int posX;
        public int posY;
        public int level;
        public bool expanded;
        public LatticeState parent;
        public List<LatticeState> children = new List<LatticeState>();
        public string key => GetNodeKey(posX, posY);

        public static string GetNodeKey(int x, int y) => $"{x},{y}";

        public Node ToNode()
        {
            return new Node() { level = level, player = 0 };
        }
    }
}
