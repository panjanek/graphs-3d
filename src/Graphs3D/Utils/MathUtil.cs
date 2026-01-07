using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphs3D.Models;
using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;

namespace Graphs3D.Utils
{
    public static class MathUtil
    {
        public static double GetTorusDistance(double d1, double d2, double size)
        {
            double d = d2 - d1;
            if (Math.Abs(d) > size / 2)
            {
                d = d - size * Math.Sign(d);
            }

            return d;
        }

        public static double Amplify(double x, int pow)
        {
            double a = 1;
            for (int i = 0; i < pow; i++)
                a = a * (1 - x);

            return 1 - a;

        }

        public static float TorusCorrection(float x, float size)
        {
            if (x < 0)
                x += size;
            else if (x > size)
                x -= size;
            return x;
        }

        public static Vector4 TorusCorrection(Vector4 pos, float size)
        {
            return new Vector4(TorusCorrection(pos.X, size), TorusCorrection(pos.Y, size), TorusCorrection(pos.Z, size), pos.W);
        }

        public static long PredictNumberOfOperations(ShaderConfig config, int[] cellCounts)
        {
            long ops = 0;
            for (int mainIdx=0; mainIdx<config.totalCellCount; mainIdx++)
            {
                int mainX = mainIdx % config.cellCount;
                var mainY = (mainIdx / config.cellCount) % config.cellCount;
                var mainZ = mainIdx / (config.cellCount * config.cellCount);
                long mainCount = cellCounts[mainIdx];
                long othersCount = 0;
                for(int dz=-1; dz<=1; dz++)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            var otherX = mainX + dx;
                            var otherY = mainY + dy;
                            var otherZ = mainZ + dz;
                            if (otherX < 0 || otherX >= config.cellCount) continue;
                            if (otherY < 0 || otherY >= config.cellCount) continue;
                            if (otherZ < 0 || otherZ >= config.cellCount) continue;
                            var otherCellIdx = otherZ * config.cellCount * config.cellCount + otherY * config.cellCount + otherX;
                            othersCount += cellCounts[otherCellIdx];
                        }
                ops += mainCount * othersCount;
            }

            return ops;
        }
    }
}
