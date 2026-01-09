using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Graphs3D.Gpu;
using Graphs3D.Models;
using System.Numerics;

namespace Graphs3D.Utils
{
    public class DebugUtil
    {
        public static bool Debug = true;

        public static string LogFile = "log.txt";

        private static Node[] nodes;

        private static int[] nodeIndices;

        public static void Log(string message)
        {
            File.AppendAllText(LogFile, $"{message}\n");
        }

        public static void DebugSolver(bool bufferB, ShaderConfig config, SolverProgram solver)
        {
            if (nodes == null || nodes.Length != config.nodesCount)
            {
                nodes = new Node[config.nodesCount];
                nodeIndices = new int[config.nodesCount];
            }

            solver.DownloadNodes(nodes, bufferB);
            var counts = solver.cellCounts;
            var offsets = solver.cellOffsets;
            GpuUtil.DownloadIntBuffer(nodeIndices, solver.nodeIndicesBuffer, config.nodesCount);

            var cellSize = config.cellSize;

            List<int>[] expected = new List<int>[config.totalCellCount];
            for(int i=0; i<expected.Length; i++)
                expected[i] = new List<int>();
            for (int idx = 0; idx<config.nodesCount; idx++)
            {
                var p = nodes[idx];
                var gridX = p.cellIndex % config.cellCount;
                var gridY = (p.cellIndex / config.cellCount) % config.cellCount;
                var gridZ = p.cellIndex / (config.cellCount * config.cellCount);

                var gridBoxMin = new Vector3(config.minBound.X + gridX * cellSize, config.minBound.Y + gridY * cellSize, config.minBound.Z + gridZ * cellSize);
                var gridBoxMax = new Vector3(config.minBound.X + (gridX+1) * cellSize, config.minBound.Y + (gridY+1) * cellSize, config.minBound.Z + (gridZ+1) * cellSize);

                var pos = p.position;
                if (pos.X < config.minBound.X) pos.X = config.minBound.X;
                if (pos.X > config.minBound.X + config.gridSize) pos.X = config.minBound.X + config.gridSize;
                if (pos.Y < config.minBound.Y) pos.Y = config.minBound.Y;
                if (pos.Y > config.minBound.Y + config.gridSize) pos.Y = config.minBound.Y + config.gridSize;
                if (pos.Z < config.minBound.Z) pos.Z = config.minBound.Z;
                if (pos.Z > config.minBound.Z + config.gridSize) pos.Z = config.minBound.Z + config.gridSize;

                if (pos.X < gridBoxMin.X-1 || pos.X > gridBoxMax.X+1)
                    throw new Exception("x outside bounds");
                else if (pos.Y < gridBoxMin.Y-1 || pos.Y > gridBoxMax.Y+1)
                    throw new Exception("y outside bounds");                  
                else if (pos.Z < gridBoxMin.Z-1 || pos.Z > gridBoxMax.Z+1)
                    throw new Exception("z outside bounds");
                else
                    expected[p.cellIndex].Add(idx);
            }

            for(int cellIdx=0; cellIdx < config.totalCellCount; cellIdx++)
            {
                var expectedList = expected[cellIdx].OrderBy(x => x).ToArray();
                var computed = nodeIndices.Skip(offsets[cellIdx]).Take(counts[cellIdx]).OrderBy(x => x).ToArray();
                if (expectedList.Length != computed.Length)
                    throw new Exception("invalid counts");

                for (int i = 0; i < computed.Length; i++)
                    if (expectedList[i] != computed[i])
                        throw new Exception($"difference at {i} for {cellIdx}");
            }
            Console.WriteLine("seems ok");


        }
    }
}
