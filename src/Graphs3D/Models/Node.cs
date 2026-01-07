using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Graphs3D.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Node
    {
        public Vector4 position; // xyz = position
        public Vector4 velocity; // xyz = velocity
        public Vector4 prevForce;
        public int species;
        public int flags;
        public int cellIndex;
        private int _pad1;
    }
}
