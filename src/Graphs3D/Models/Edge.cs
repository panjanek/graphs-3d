using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Edge
    {
        public Edge() { }
        public uint a;
        public uint b;
        public float restLength = 10;
        public int player;
        public int flags;
    }
}
