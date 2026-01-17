using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Graphs3D.Graphs.Bloxorz;
using Graphs3D.Utils;

namespace Graphs3D.Graphs.Klotski
{
    public class KlotskiGraph : GraphBase<KlotskiNode>, IGraph
    {
        public const int NormalColor = 2;

        public const int WinColor = 1;

        private int width;

        private int height;

        public static KlotskiXY[] AllDirections = new KlotskiXY[4] { new KlotskiXY(-1, 0), new KlotskiXY(0, -1), new KlotskiXY(1, 0), new KlotskiXY(0, 1) };

        public static KlotskiXY[] VerticalDirections = new KlotskiXY[2] { new KlotskiXY(0, -1), new KlotskiXY(0, 1) };

        public static KlotskiXY[] HorizontalDirections = new KlotskiXY[2] { new KlotskiXY(-1, 0), new KlotskiXY(1, 0) };

        private KlotskiPresenter presenter;

        public KlotskiGraph(string resourceName)
        {
            var root = new KlotskiNode(ResourceUtil.LoadStringFromResource(resourceName));
            width = root.map.GetLength(0);
            height = root.map.GetLength(1);
            AddNode(root);
            presenter = new KlotskiPresenter(this);
        }

        protected override void InternalExpandNode(KlotskiNode parent)
        {
            var moves = parent.GenerateMoves();
            foreach (var move in moves)
            {
                var next = new KlotskiNode(parent, move);
                AddNode(next);
            }
        }

        public override void PostExpandActions()
        {
            for(int i=0; i<internalEdges.Count; i++)
            {
                var n1 = graphNodes[(int)internalEdges[i].a];
                var n2 = graphNodes[(int)internalEdges[i].b];
                var n1_moves = n1.GenerateMoves();
                var n2_moves = n2.GenerateMoves();
                var c = n1_moves.Count + n2_moves.Count;

                var restLen = 10f;
                if (c <= 4 && n1.expanded && n2.expanded && n1_moves.Count > 1 && n2_moves.Count > 1)
                    restLen = 100.0f;
                SetInternalEdgeAttributes(i, (int)internalEdges[i].color, restLen);
            }
        }

        public List<KlotskiTransition> GetAvailableTransitions(KlotskiNode parent)
        {
            var moves = parent.GenerateMoves();
            var result = new List<KlotskiTransition>();
            foreach (var move in moves)
            {
                var next = new KlotskiNode(parent, move);
                if (keyedNodes.ContainsKey(next.Key))
                {
                    result.Add(new KlotskiTransition() { move = move, node = keyedNodes[next.Key] });
                }
            }

            return result;
        }

        public override bool DrawPosition(int idx, Canvas canvas) => presenter.Draw(canvas, graphNodes[idx]);

        public override void Click(double x, double y) => presenter.Click(x, y);
    }
}
