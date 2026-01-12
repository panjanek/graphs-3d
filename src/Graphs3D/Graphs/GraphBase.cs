using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Graphs3D.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Graphs3D.Graphs
{
    public abstract class GraphBase<TNode>
        where TNode : GraphNodeBase
    {
        public List<Node> Nodes => internalNodes;

        public List<Edge> Edges => internalEdges;

        public Action<int> NavigateTo { get; set; }

        protected List<Node> internalNodes = new List<Node>();

        protected List<Edge> internalEdges = new List<Edge>();

        private HashSet<string> existingEdges = new HashSet<string>();

        protected Dictionary<string, TNode> keyedNodes = new Dictionary<string, TNode>();

        protected List<TNode> graphNodes = new List<TNode>();

        public int Expand()
        {
            var parent = GetBestNodeToExpand();
            if (parent == null)
                return 0;

            ExpandNode(parent.idx);

            PostExpandActions();
            return parent.idx;
        }

        public List<int> ExpandMany(int count)
        {
            var expanded = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var parent = GetBestNodeToExpand();
                if (parent != null)
                {
                    ExpandNode(parent.idx);
                    expanded.Add(parent.idx);
                }
            }

            PostExpandActions();
            return expanded;
        }

        protected virtual GraphNodeBase GetBestNodeToExpand()
        {
            return graphNodes.Where(n => !n.expanded).OrderBy(n => n.level).FirstOrDefault();
        }

        public bool IsFinished() => !graphNodes.Any(n => !n.expanded);

        public bool ExpandNode(int parentIdx)
        {
            if (parentIdx < Nodes.Count)
            {
                var parent = graphNodes[parentIdx];
                if (!parent.expanded)
                {
                    InternalExpandNode(parent);
                    parent.expanded = true;
                    PostExpandActions();
                    return true;
                }
            }

            return false;
        }
        public virtual bool DrawPosition(int idx, Canvas canvas)
        {
            canvas.Children.Clear();
            return false;
        }

        public virtual void Click(double x, double y) { }

        public virtual void PostExpandActions() { }

        protected abstract void InternalExpandNode(TNode parent);

        protected void AddEdge(int parentIdx, int childIdx, int player = 0)
        {
            if (!EdgeExists(parentIdx, childIdx))
            {
                internalEdges.Add(new Edge() { a = (uint)parentIdx, b = (uint)childIdx, player = (uint)player });
                existingEdges.Add($"{parentIdx}-{childIdx}");
            }
        }

        protected void AddNode(TNode node)
        {
            if (keyedNodes.TryGetValue(node.Key, out var existing))
            {
                if (node.parentIdx.HasValue)
                    AddEdge(node.parentIdx.Value, existing.idx, node.player);
            }
            else
            {
                var count = Nodes.Count;
                node.idx = count;
                node.level = node.parentIdx.HasValue ? graphNodes[node.parentIdx.Value].level + 1 : 0;
                keyedNodes[node.Key] = node;
                internalNodes.Add(node.ToInternalNode());
                graphNodes.Add(node);
                if (node.parentIdx.HasValue)
                    AddEdge(node.parentIdx.Value, node.idx, node.player);
            }
        }

        protected void SetInternalNodeAttributes(int idx, int player)
        {
            var internalNode = internalNodes[idx];
            internalNode.player = player;
            internalNodes[idx] = internalNode;
        }

        protected void SetInternalEdgeAttributes(int idx, int player)
        {
            var internalEdge = internalEdges[idx];
            internalEdge.player = (uint)player;
            internalEdges[idx] = internalEdge;
        }

        protected bool EdgeExists(int a, int b) => existingEdges.Contains($"{a}-{b}");
    }

    public abstract class GraphNodeBase
    {
        public int idx;
        public int? parentIdx;
        public int level;
        public int player;
        public int win;
        public bool leaf;
        public bool expanded;
        public abstract string Key { get; }

        public Node ToInternalNode()
        {
            return new Node() { level = level, player = player, win = win, leaf = leaf ? 1 : 0, parent = parentIdx ?? -1 };
        }
    }
}
