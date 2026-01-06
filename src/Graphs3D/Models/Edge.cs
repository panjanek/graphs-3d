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
        public uint a;
        public uint b;
        public float restLength;
        public int player;
        public int flags;
    }
}
