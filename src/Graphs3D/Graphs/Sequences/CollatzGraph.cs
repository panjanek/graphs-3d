using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Navigation;
using Graphs3D.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using CheckBox = System.Windows.Forms.CheckBox;

namespace Graphs3D.Graphs.Geometry
{
    public class CollatzGraph : GraphBase<CollatzNode>, IGraph
    {
        private const int EvenColor = 4;

        private const int OddColor = 3;
        public CollatzGraph()
        {
            var root = new CollatzNode() { x = 1, level = 1 };
            AddNode(root);
        }

        protected override void InternalExpandNode(CollatzNode parent)
        {
            TryAdd(parent, parent.x*2, EvenColor);
            if (parent.x % 6 == 4)
                TryAdd(parent, (parent.x - 1)/3, OddColor);
        }

        protected override GraphNodeBase GetBestNodeToExpand() =>
            graphNodes.Where(n => !n.expanded).OrderBy(n => n.x).FirstOrDefault();

        public override void PostExpandActions()
        {
            /*
            var maxLevel = graphNodes.Max(n => n.level);
            for (int i = 0; i < internalEdges.Count; i++)
            {
                var n1 = graphNodes[(int)internalEdges[i].a];
                var n2 = graphNodes[(int)internalEdges[i].b];

                var max = Math.Max(n1.level, n2.level);
                
                var restLen = 10f;
                restLen = (float)(20f * Math.Exp(-2*max/maxLevel));
                SetInternalEdgeAttributes(i, (int)internalEdges[i].color, restLen);
                
            }*/
        }

        private void TryAdd(CollatzNode parent, long newX, int color)
        {
            var newNode = new CollatzNode() { x = newX, parentIdx = parent.idx, color =  color };
            AddNode(newNode);
        }

    }

    public class CollatzNode : GraphNodeBase
    {
        public long x;
        public override string Key => $"{x}";
    }
}
