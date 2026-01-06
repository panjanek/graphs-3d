using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Graphs3D.Models
{
    public class Simulation
    {
        public const int MaxSpeciesCount = 10;

        public const int KeypointsCount = 6;

        public ShaderConfig config;

        public float cameraFollowSpeed = 1f;

        public float particleSize = 0.3f;

        [JsonIgnore]
        public Node[] nodes;

        public Edge[] edges;

        public int seed = 11;

        public float followDistance = 150;

        public static Random globalRandom = new Random(1);

        public Simulation()
        {
            config = new ShaderConfig();
        }

        public void StartSimulation(int particlesCount, float size)
        {
            config.speciesCount = 1;
            config.fieldSize = size;
            config.nodesCount = 1;
            nodes = new Node[config.nodesCount];
            config.edgesCount = 0;
            edges = new Edge[config.edgesCount];
            nodes[0].position = new Vector4(config.fieldSize / 2, config.fieldSize / 2, config.fieldSize / 2, 1.0f);
            
            
            for (int i = 0; i < 5; i++)
              AddRandomNodes(particlesCount/5);
            //InitializeParticles(particlesCount);
            //InitializeRandomEdges();
        }

        public void AddRandomNodes(int addNodesCount)
        {
            var oldNodes = nodes;
            nodes = new Node[oldNodes.Length + addNodesCount];
            Array.Copy(oldNodes, nodes, oldNodes.Length);
            var oldEdges = edges;
            edges = new Edge[oldEdges.Length + addNodesCount];
            Array.Copy(oldEdges, edges, oldEdges.Length);
            for (int i=0; i< addNodesCount; i++)
            {
                int connectToIdx = globalRandom.Next(oldNodes.Length);
                var connectToPos = oldNodes[connectToIdx].position.Xyz;
                var newPos = connectToPos + new Vector3((float)globalRandom.NextDouble() - 0.5f, (float)globalRandom.NextDouble() - 0.5f, (float)globalRandom.NextDouble() - 0.5f) * 1;
                nodes[oldNodes.Length + i].position = new Vector4(newPos, 1.0f);
                nodes[oldNodes.Length + i].velocity = new Vector4();
                edges[oldEdges.Length + i].a = (uint)connectToIdx;
                edges[oldEdges.Length + i].b = (uint)(oldNodes.Length + i);
                edges[oldEdges.Length + i].restLength = 10;
            }

            config.nodesCount = nodes.Length;
            config.edgesCount = edges.Length;
        }

        private void InitializeRandomEdges(int edgeCount)
        {
            var rnd = new Random(seed);
            config.edgesCount = config.nodesCount / 2;
            edges = new Edge[config.edgesCount];
            for(int i=0; i< config.edgesCount; i++)
            {
                edges[i].a = (uint)rnd.Next(0, config.nodesCount);
                edges[i].b = (uint)rnd.Next(0, config.nodesCount);
                edges[i].restLength = 10;
            }
        }

        public void InitializeParticles(int count)
        {
            if (nodes == null || nodes.Length != count)
                nodes = new Node[count];

            var rnd = new Random(1);
            for(int i=0; i< count; i++)
            {
                nodes[i].position = new Vector4((float)(config.fieldSize * rnd.NextDouble()), 
                                                    (float)(config.fieldSize * rnd.NextDouble()),
                                                    (float)(config.fieldSize * rnd.NextDouble()),
                                                    0);
                nodes[i].velocity = new Vector4((float)(100 * config.dt * (rnd.NextDouble()-0.5)), 
                                                    (float)(100 * config.dt * (rnd.NextDouble()-0.5)),
                                                    (float)(100 * config.dt * (rnd.NextDouble() - 0.5)),
                                                    0);
                nodes[i].species = rnd.Next(config.speciesCount);
            }
        }
    }
}
