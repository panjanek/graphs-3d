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
        public Particle[] particles;

        [JsonIgnore]
        public Vector4[] forces;

        public int seed = 11;

        public float followDistance = 150; 

        //this is for json serialization
        public float[][] F
        {
            get
            {
                var res = new float[forces.Length][];
                for (int i = 0; i < forces.Length; i++)
                    res[i] = [forces[i].X, forces[i].Y];
                return res;
            }
            set
            {
                for (int i = 0; i < forces.Length; i++)
                    forces[i] = new Vector4(value[i][0], value[i][1], 0, 0);
            }
        }

        public Simulation()
        {
            config = new ShaderConfig();
            forces = new Vector4[MaxSpeciesCount * MaxSpeciesCount * KeypointsCount];
        }

        public void StartSimulation(int particlesCount, float size)
        {
            config.speciesCount = 1;
            config.fieldSize = size;
            config.particleCount = particlesCount;
            InitializeParticles(particlesCount);
            var rnd = new Random(seed);
        }

        public void InitializeParticles(int count)
        {
            if (particles == null || particles.Length != count)
                particles = new Particle[count];

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
