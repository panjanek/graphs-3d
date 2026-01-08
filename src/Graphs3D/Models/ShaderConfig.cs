using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using static OpenTK.Graphics.OpenGL.GL;

namespace Graphs3D.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 108)]
    public unsafe struct ShaderConfig
    {
        public ShaderConfig()
        {

        }

        [FieldOffset(0)] public int nodesCount = 0;

        [FieldOffset(4)] public float dt = 0.1f;

        [FieldOffset(8)] public float sigma2 = 0f;

        [FieldOffset(12)] public int edgesCount = 0;

        [FieldOffset(16)] public int useCells = 0;

        [FieldOffset(20)] public float fieldSize = 800;

        [FieldOffset(24)] public float cellSize = 0;

        [FieldOffset(28)] public float maxDist = 100;

        [FieldOffset(32)] public float gridSize = 0;

        [FieldOffset(36)] public float damping = 0.1f;

        [FieldOffset(40)] public int trackedIdx;

        [FieldOffset(44)] public float maxForce = 15;

        [FieldOffset(48)] public float amp = 1f;

        [FieldOffset(52)] public int cellCount = 0;

        [FieldOffset(56)] public int totalCellCount = 0;

        [FieldOffset(60)] int _pad1;

        [FieldOffset(64)] public Vector4 minBound;

        [FieldOffset(80)] public Vector4 maxBound;

        [FieldOffset(96)] public int marker1 = -1;

        [FieldOffset(100)]  public int marker2;

        [FieldOffset(104)]  public int markerT;
    }
}
