using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Graphs.Bloxorz;
using Graphs3D.Utils;

namespace Graphs3D.Graphs.Klotski
{
    public class KlotskiGraph : GraphBase<KlotskiNode>, IGraph
    {
        public const int NormalColor = 2;

        private int width;

        private int height;

        public static KlotskiXY[] Directions = new KlotskiXY[4] { new KlotskiXY(-1, 0), new KlotskiXY(0, -1), new KlotskiXY(1, 0), new KlotskiXY(0, 1) };

        public KlotskiGraph(string resourceName)
        {
            var root = new KlotskiNode(ResourceUtil.LoadStringFromResource(resourceName));
            width = root.map.GetLength(0);
            height = root.map.GetLength(1);
            AddNode(root);
            //presenter = new BloxorzPresenter(this);
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
    }
}
