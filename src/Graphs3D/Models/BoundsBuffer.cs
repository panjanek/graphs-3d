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
    public struct BoundsBuffer
    {
        public UVec4 MinBound;
        public UVec4 MaxBound;

        public static uint FloatToOrderedUint(float f)
        {
            uint u = BitConverter.SingleToUInt32Bits(f);
            return (u & 0x80000000u) != 0 ? ~u : (u | 0x80000000u);
        }

        public static float OrderedUintToFloat(uint u)
        {
            u = (u & 0x80000000u) != 0 ? (u & 0x7FFFFFFFu) : ~u;
            return BitConverter.UInt32BitsToSingle(u);
        }

        public static BoundsBuffer GetInitialValues()
        {
            var maxed = new BoundsBuffer
            {
                MinBound = new UVec4(
                    FloatToOrderedUint(float.PositiveInfinity),
                    FloatToOrderedUint(float.PositiveInfinity),
                    FloatToOrderedUint(float.PositiveInfinity),
                    0
                ),
                            MaxBound = new UVec4(
                    FloatToOrderedUint(float.NegativeInfinity),
                    FloatToOrderedUint(float.NegativeInfinity),
                    FloatToOrderedUint(float.NegativeInfinity),
                    0
                )
            };

            return maxed;
        }

        public Vector4 GetMin()
        {
            return new Vector4(
                OrderedUintToFloat(MinBound.X),
                OrderedUintToFloat(MinBound.Y),
                OrderedUintToFloat(MinBound.Z),
                0
            );
        }

        public Vector4 GetMax()
        {
            return new Vector4(
                OrderedUintToFloat(MaxBound.X),
                OrderedUintToFloat(MaxBound.Y),
                OrderedUintToFloat(MaxBound.Z),
                0
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UVec4
    {
        public uint X;
        public uint Y;
        public uint Z;
        public uint W;

        public UVec4(uint x, uint y, uint z, uint w = 0)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
