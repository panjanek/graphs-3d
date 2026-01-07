using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Navigation;
using Graphs3D.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Graphs3D.Graphs
{
    public class LatticeGraph : GraphBase<LatticeNode>, IGraph
    {
        private int sizeX;

        private int sizeY;

        private bool wrapHorizontal;

        private bool wrapVertical;

        public LatticeGraph(int sizeX, int sizeY, bool wrapHorizontal, bool wrapVertical)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.wrapVertical = wrapVertical;
            this.wrapHorizontal = wrapHorizontal;
            var root = new LatticeNode();
            AddNode(root);
        }

        protected override void InternalExpandNode(LatticeNode parent)
        {
            TryAdd(parent, -1, 0);
            TryAdd(parent, 1, 0);
            TryAdd(parent, 0, -1);
            TryAdd(parent, 0, 1);
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
            var newNode = new LatticeNode() { posX = nx, posY = ny, parentIdx = parent.idx, player = 1-parent.player };
            AddNode(newNode);
        }
    }

    public class LatticeNode : GraphNodeBase
    {
        public int posX;
        public int posY;
        public override string Key => $"{posX},{posY}";
    }
}
