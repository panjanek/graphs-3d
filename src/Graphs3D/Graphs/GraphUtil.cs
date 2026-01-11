using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Graphs
{
    public static class GraphUtil
    {
        public static void Copy2D(int[,] src, int[,] dst)
        {
            if (src.GetLength(0) != dst.GetLength(0) || src.GetLength(1) != dst.GetLength(1))
                throw new Exception($"Arrays 2D have different sizes {src.GetLength(0)}x{src.GetLength(1)} vs {dst.GetLength(0)}x{dst.GetLength(1)}");

            for (int y = 0; y < src.GetLength(1); y++)
                for (int x = 0; x < src.GetLength(0); x++)
                    dst[x, y] = src[x, y];
        }
    }
}
