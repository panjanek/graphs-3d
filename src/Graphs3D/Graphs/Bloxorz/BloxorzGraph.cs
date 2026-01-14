using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Graphs3D.Graphs.Sokoban;
using Graphs3D.Utils;

namespace Graphs3D.Graphs.Bloxorz
{
    //https://www.crazygames.pl/gra/bloxorz
    public class BloxorzGraph : GraphBase<BloxorzNode>, IGraph
    {
        public const int ColorOk = 4;

        public static BloxorzCoord[] PossibleMoves = new BloxorzCoord[4] { new BloxorzCoord(-1,0), new BloxorzCoord(0, -1), new BloxorzCoord(1, 0), new BloxorzCoord(0, 1) };

        private int width;

        private int height;

        private BloxorzPresenter presenter;

        public BloxorzGraph(string resourceName)
        {
            var root = new BloxorzNode(ResourceUtil.LoadStringFromResource(resourceName));
            width = root.map.GetLength(0);
            height = root.map.GetLength(1);
            AddNode(root);
            presenter = new BloxorzPresenter(this);
        }

        protected override void InternalExpandNode(BloxorzNode parent)
        {
            var moves = parent.GenerateMoves();
            foreach (var move in moves)
            {
                var next = new BloxorzNode(parent, move);
                AddNode(next);
            }
        }

        public List<BloxorzTransition> GetAvailableTransitions(BloxorzNode parent)
        {
            var moves = parent.GenerateMoves();
            var result = new List<BloxorzTransition>();
            foreach (var move in moves)
            {
                var next = new BloxorzNode(parent, move);
                if (keyedNodes.ContainsKey(next.Key))
                {
                    result.Add(new BloxorzTransition() { move = move, node = keyedNodes[next.Key] });
                }
            }

            return result;
        }

        public override bool DrawPosition(int idx, Canvas canvas) => presenter.Draw(canvas, graphNodes[idx]);

        public override void Click(double x, double y) => presenter.Click(x, y);
    }
}
