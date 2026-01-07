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

        public float cameraFollowSpeed = 0.02f;

        public float particleSize = 0.3f;

        [JsonIgnore]
        public Node[] nodes;

        public Edge[] edges;

        public int seed = 11;

        public float followDistance = 10;

        public static Random globalRandom = new Random(1);

        public Simulation()
        {
            config = new ShaderConfig();
        }

        public void StartSimulation(int particlesCount, float size)
        {
            config.fieldSize = size;
            config.nodesCount = 1;

            nodes = new Node[config.nodesCount];
            config.edgesCount = 0;
            edges = new Edge[config.edgesCount];
            nodes[0].position = new Vector4(config.fieldSize / 2, config.fieldSize / 2, config.fieldSize / 2, 1.0f);

            Create2DGrid((uint)particlesCount, (uint)particlesCount, true, false);
            //Create3DGrid(2, 2, 2);

            //for (int i = 0; i < 5; i++)
            //  AddRandomNodes(particlesCount/5);
        }

        private void Create2DGrid(uint rowSizeX, uint rowSizeY, bool wrapHoriz, bool wrapVert)
        {
            nodes = new Node[rowSizeX * rowSizeY];
            var radius = 0.1f * config.fieldSize;
            var center = new Vector4(config.fieldSize / 2, config.fieldSize / 2, config.fieldSize / 2, 0);
            for (int i=0; i<nodes.Length; i++)
            {
                nodes[i].position = center + radius * new Vector4((float)globalRandom.NextDouble()-0.5f, (float)globalRandom.NextDouble() - 0.5f, (float)globalRandom.NextDouble() - 0.5f, 0);
            }

            List<Edge> list = new List<Edge>();
            for(uint x=0; x< rowSizeX; x++)
                for(uint y=0; y< rowSizeY; y++)
                {
                    if (x > 0)
                        list.Add(new Edge() { a = y* rowSizeX + (x-1), b = y* rowSizeX + x, player = 1+(x+y)%2 });
                    if (y > 0)
                        list.Add(new Edge() { a = (y-1) * rowSizeX + x, b = y * rowSizeX + x, player = 1+(x + y) % 2 });

                    if (y == 0 && wrapHoriz)
                        list.Add(new Edge() { a = (0) * rowSizeX + x, b = (rowSizeY - 1) * rowSizeX + x, player = 1+(x + y) % 2 });

                    if (x == 0 && wrapVert)
                        list.Add(new Edge() { a = y * rowSizeX + 0, b = y * rowSizeX + rowSizeX - 1, player = 1+(x + y) % 2 });
                }

            edges = list.ToArray();
            config.nodesCount = nodes.Length;
            config.edgesCount = edges.Length;
        }

        private void Create3DGrid(uint rowSizeX, uint rowSizeY, uint rowSizeZ)
        {
            nodes = new Node[rowSizeX * rowSizeY * rowSizeZ];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].position = new Vector4((float)globalRandom.NextDouble() * config.fieldSize, (float)globalRandom.NextDouble() * config.fieldSize, (float)globalRandom.NextDouble() * config.fieldSize, 1.0f);
            }

            List<Edge> list = new List<Edge>();
            for (uint x = 0; x < rowSizeX; x++)
                for (uint y = 0; y < rowSizeY; y++)
                    for (uint z = 0; z < rowSizeZ; z++)
                    {
                        if (x > 0)
                            list.Add(new Edge() { a = (z * rowSizeX * rowSizeY)     + y * rowSizeX + (x - 1), 
                                                  b = (z * rowSizeX * rowSizeY)     + y * rowSizeX + x });
                        if (y > 0)
                            list.Add(new Edge() { a = (z * rowSizeX * rowSizeY)     + (y - 1) * rowSizeX + x, 
                                                  b = (z * rowSizeX * rowSizeY)     + y * rowSizeX + x });

                        if (z > 0)
                            list.Add(new Edge()
                            {
                                a = ((z-1) * rowSizeX * rowSizeY) + (y) * rowSizeX + x,
                                b = (z * rowSizeX * rowSizeY) + y * rowSizeX + x
                            });

                    }

            edges = list.ToArray();
            config.nodesCount = nodes.Length;
            config.edgesCount = edges.Length;
        }
    }
}
