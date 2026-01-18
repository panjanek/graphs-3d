using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Graphs3D.Graphs;
using Graphs3D.Graphs.TicTacToe;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Graphs3D.Models
{
    public class Simulation
    {
        public const int MaxSpeciesCount = 10;

        public const int KeypointsCount = 6;

        public ShaderConfig config;

        public float particleSize = 0.3f;

        public float lineWidth = 300f;

        public float expansionSpeed = 100;

        public float fogDensity = 0.0002f;

        public float unhighlightedAlpha = 0.25f;

        public Node[] nodes = new Node[0];

        public Edge[] edges = new Edge[0];

        public int[] nodeFlags = new int[0];

        public int seed = 11;

        public float followDistance = 100;

        public float rotationSpeed = 0.001f;

        public float cameraPeriod = 100f;

        public static Random globalRandom = new Random(1);

        public IGraph graph;

        public Simulation()
        {
            config = new ShaderConfig();
        }

        public void StartNewGraph(IGraph graph)
        {
            this.graph = graph;
            config.fieldSize = 100;
            nodes = graph.Nodes.ToArray();
            edges = graph.Edges.ToArray();
            nodeFlags = new int[nodes.Length];
            config.nodesCount = nodes.Length;
            config.edgesCount = edges.Length;
            nodes[0].position = new Vector4(config.fieldSize / 2, config.fieldSize / 2, config.fieldSize / 2, 1.0f);
            config.marker1 = 0;
            Expand();
        }

        public void Expand()
        {
            lock (this)
            {
                if (graph.IsFinished())
                    return;

                int wantNodesCount = (int)expansionSpeed;
                int generatedNodes = 0;
                int expandedCount = 0;
                do
                {
                    graph.ExpandMany(wantNodesCount/2);
                    expandedCount = graph.Nodes.Count - nodes.Length;
                    ExpandBuffers();
                    generatedNodes += expandedCount;
                } while (generatedNodes < wantNodesCount && !graph.IsFinished());
            }
        }

        public bool ExpandOne(int idx)
        {
            lock (this)
            {
                if (graph.ExpandNode(idx))
                {
                    ExpandBuffers();
                    return true;
                }

                return false;
            }
        }

        private void ExpandBuffers()
        {
            var newNodes = graph.Nodes.ToArray();
            var newEdges = graph.Edges.ToArray();
            float randomRadius = 0.1f;
            if (newNodes.Length > nodes.Length)
            {
                var tmp = new Node[newNodes.Length];
                Array.Copy(nodes, tmp, nodes.Length);
                Array.Copy(newNodes, nodes.Length, tmp, nodes.Length, newNodes.Length - nodes.Length);
                for (int i = nodes.Length; i < newNodes.Length; i++)
                {
                    var centerIdx = tmp[i].parent;
                    tmp[i].position = tmp[centerIdx].position + new Vector4((float)(globalRandom.NextDouble() - 0.5f)*randomRadius, 
                                                                            (float)(globalRandom.NextDouble() - 0.5f)*randomRadius,
                                                                            (float)(globalRandom.NextDouble() - 0.5f)*randomRadius, 1);
                }

                nodes = tmp;
                config.nodesCount = nodes.Length;

                var tmpFlags = new int[newNodes.Length];
                for (int i = 0; i < nodeFlags.Length; i++)
                    tmpFlags[i] = nodeFlags[i];
                nodeFlags = tmpFlags;
            }

            if (newEdges.Length > edges.Length)
            {
                var tmp = new Edge[newEdges.Length];
                Array.Copy(edges, tmp, edges.Length);
                Array.Copy(newEdges, edges.Length, tmp, edges.Length, newEdges.Length - edges.Length);
                edges = tmp;
                config.edgesCount = edges.Length;
            }

            //always copy attributes
            for (int i = 0; i < newNodes.Length; i++)
            {
                nodes[i].color = newNodes[i].color;
                nodes[i].leaf = newNodes[i].leaf;
            }

            for (int e = 0; e < newEdges.Length; e++)
                edges[e].color = newEdges[e].color;
        }

        public List<int> GetChildren(int parentIdx)
        {
            var parent = nodes[parentIdx];
            List<int> childrenIdx = new List<int>();
            for (int i = 0; i < edges.Length; i++)
            {
                var edge = edges[i];
                if (edge.a == parentIdx || edge.b == parentIdx)
                {
                    var otherIdx = edge.a == parentIdx ? edge.b : edge.a;
                    var other = nodes[otherIdx];
                    if (other.level > parent.level && !childrenIdx.Contains((int)otherIdx))
                        childrenIdx.Add((int)otherIdx);
                }
            }

            return childrenIdx;
        }

        public List<int> FindPath(int startIdx, int targetIdx)
        {
            for (int i = 0; i < edges.Length; i++)
                if ((edges[i].a == startIdx && edges[i].b == targetIdx) || (edges[i].b == startIdx && edges[i].a == targetIdx))
                    return [startIdx, targetIdx];

            var startPath = PathToRoot(startIdx);
            startPath.Reverse();
            var targetPath = PathToRoot(targetIdx);
            targetPath.Reverse();
            var shorterLen = Math.Min(startPath.Count, targetPath.Count);
            var commonDescendantIdx = 0;
            for (int i = 0; i < shorterLen; i++)
                if (startPath[i] == targetPath[i])
                    commonDescendantIdx = startPath[i];
            startPath.Reverse();
            var sub1 = GetSubPath(startPath, startIdx, commonDescendantIdx);
            var sub2 = GetSubPath(targetPath, commonDescendantIdx, targetIdx);
            if (sub1.Count > 0 && sub2.Count > 0 && sub1.Last() == sub2.First())
                sub1.RemoveAt(sub1.Count-1);
            sub1.AddRange(sub2);
            return sub1;
        }

        private List<int> GetSubPath(List<int> path, int startIdx, int targetIdx)
        {
            int start = path.IndexOf(startIdx);
            int stop = path.IndexOf(targetIdx);
            return path.Skip(start).Take(stop - start+1).ToList();
        }

        public List<int> PathToRoot(int startIdx)
        {
            var result = new List<int>();
            int parentIdx;
            result.Add(startIdx);
            while ( (parentIdx = nodes[startIdx].parent) >= 0)
            {
                result.Add(parentIdx);
                startIdx = parentIdx;
            }

            return result;
        }

        public List<int> GetWinningPath()
        {
            var winIdx = GetWinningNode();
            if (winIdx.HasValue)
            {
                var path = PathToRoot(winIdx.Value);
                path.Reverse();
                return path;
            }

            return [];
        }

        public int? GetWinningNode()
        {
            int? win = null;
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].win > 0)
                    win = i;
            return win;
        }
    }
}
