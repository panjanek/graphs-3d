using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace Graphs3D.Models
{
    public class Simulation
    {
        public const int MaxSpeciesCount = 10;

        public const int KeypointsCount = 6;

        public ShaderConfig config;

        public float cameraFollowSpeed = 1f;

        public float particleSize = 0.7f;

        [JsonIgnore]
        public Node[] particles;

        public Edge[] edges;

        public int seed = 11;

        public float followDistance = 150; 

        public Simulation()
        {
            config = new ShaderConfig();
        }

        public void StartSimulation(int particlesCount, float size)
        {
            config.speciesCount = 1;
            config.fieldSize = size;
            config.particleCount = particlesCount;
            InitializeParticles(particlesCount);
            InitializeRandomEdges();
            var rnd = new Random(seed);
        }

        private void InitializeRandomEdges()
        {
            var rnd = new Random(seed);
            config.edgesCount = config.particleCount / 2;
            edges = new Edge[config.edgesCount];
            for(int i=0; i< config.edgesCount; i++)
            {
                edges[i].a = (uint)rnd.Next(0, config.particleCount);
                edges[i].b = (uint)rnd.Next(0, config.particleCount);
            }
        }

        public void InitializeParticles(int count)
        {
            if (particles == null || particles.Length != count)
                particles = new Node[count];

            var rnd = new Random(1);
            for(int i=0; i< count; i++)
            {
                particles[i].position = new Vector4((float)(config.fieldSize * rnd.NextDouble()), 
                                                    (float)(config.fieldSize * rnd.NextDouble()),
                                                    (float)(config.fieldSize * rnd.NextDouble()),
                                                    0);
                particles[i].velocity = new Vector4((float)(100 * config.dt * (rnd.NextDouble()-0.5)), 
                                                    (float)(100 * config.dt * (rnd.NextDouble()-0.5)),
                                                    (float)(100 * config.dt * (rnd.NextDouble() - 0.5)),
                                                    0);
                particles[i].species = rnd.Next(config.speciesCount);
            }
        }
    }
}
