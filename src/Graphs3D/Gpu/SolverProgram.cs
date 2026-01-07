using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Graphs3D.Models;
using Graphs3D.Utils;
using System.Configuration;

namespace Graphs3D.Gpu
{
    public class SolverProgram
    {
        private int maxGroupsX;

        private int solvingProgram;

        private int tilingCountProgram;

        private int tilingBinProgram;

        private int tilingBoundstProgram;

        private int uboConfig;

        public int edgesBuffer;

        public int pointsBufferA;

        public int pointsBufferB;

        private int trackingBuffer;

        public int cellCountBuffer;

        public int cellOffsetBuffer;

        public int cellOffsetBuffer2;

        public int nodeIndicesBuffer;

        public int neighboursBuffer;

        public int neighboursStartBuffer;

        public int neighboursCountBuffer;

        public int restLengthsBuffer;

        private int currentNodesCount;

        private int currentEdgesCount;

        private int currentTotalCellsCount;

        private int boundsBuffer;

        private int shaderPointStrideSize;

        public int[] cellCounts;

        public int[] cellOffsets;

        public uint[] neighbours;

        public uint[] neighboursStart;

        public uint[] neighboursCount;

        public float[] restLengths;

        private Node trackedParticle;

        private BoundsBuffer bounds;

        private int stepCounter = 0;

        public SolverProgram()
        {
            uboConfig = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
            int configSizeInBytes = Marshal.SizeOf<ShaderConfig>();
            GL.BufferData(BufferTarget.UniformBuffer, configSizeInBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, uboConfig);

            //constant-length buffers
            CreateBuffer(ref trackingBuffer, 1, Marshal.SizeOf<Node>());
            CreateBuffer(ref boundsBuffer, 1, Marshal.SizeOf<BoundsBuffer>());

            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);
            shaderPointStrideSize = Marshal.SizeOf<Node>();
            solvingProgram = ShaderUtil.CompileAndLinkComputeShader("solver.comp");
            tilingBoundstProgram = ShaderUtil.CompileAndLinkComputeShader("tiling_bounds.comp");
            tilingCountProgram = ShaderUtil.CompileAndLinkComputeShader("tiling_count.comp");
            tilingBinProgram = ShaderUtil.CompileAndLinkComputeShader("tiling_bin.comp");
        }

        public void Run(ref ShaderConfig config, bool recomputeBounds = false)
        {
            PrepareBuffers(config.nodesCount, config.totalCellCount, config.edgesCount);
            int dispatchGroupsX = (currentNodesCount + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX;
            if (dispatchGroupsX > maxGroupsX)
                dispatchGroupsX = maxGroupsX;           

            //upload config
            GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
            GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf<ShaderConfig>(), ref config, BufferUsageHint.StaticDraw);

            RunTiling(ref config);

            //bind ubo and buffers
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, uboConfig);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBufferA);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, pointsBufferB);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, edgesBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, trackingBuffer);
            //bins buffer
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, cellCountBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 7, cellOffsetBuffer2);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 8, nodeIndicesBuffer);
            //neighbours buffers
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 10, neighboursBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 11, neighboursStartBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 12, neighboursCountBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 13, restLengthsBuffer);

            GL.UseProgram(solvingProgram);
            GL.DispatchCompute(dispatchGroupsX, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.VertexAttribArrayBarrierBit | MemoryBarrierFlags.BufferUpdateBarrierBit);

            (pointsBufferA, pointsBufferB) = (pointsBufferB, pointsBufferA);
            stepCounter++;
        }

        private void RunTiling(ref ShaderConfig config)
        {
            int dispatchGroupsX = (currentNodesCount + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX;
            if (dispatchGroupsX > maxGroupsX)
                dispatchGroupsX = maxGroupsX;

            if (config.useCells == 1 || stepCounter % 10 == 0)
            {
                //upload config and initial bounds
                GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
                GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf<ShaderConfig>(), ref config, BufferUsageHint.StaticDraw);
                bounds = BoundsBuffer.GetInitialValues();
                GL.BindBuffer(BufferTarget.UniformBuffer, boundsBuffer);
                GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf<BoundsBuffer>(), ref bounds, BufferUsageHint.StaticDraw);

                //------------------- compute bounding cube --------------------------------
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, uboConfig);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBufferA);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, boundsBuffer);

                GL.UseProgram(tilingBoundstProgram);
                GL.DispatchCompute(dispatchGroupsX, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.VertexAttribArrayBarrierBit | MemoryBarrierFlags.BufferUpdateBarrierBit);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, boundsBuffer);
                GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, Marshal.SizeOf<BoundsBuffer>(), ref bounds);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

                config.minBound = bounds.GetMin() - new Vector4(-0.1f, -0.1f, -0.1f, 0);
                config.maxBound = bounds.GetMax();
                var d = config.maxBound - config.minBound;
                config.gridSize = Math.Max(d.X, Math.Max(d.Y, d.Z)) + 0.2f;
                config.cellCount = (int)Math.Floor(config.gridSize / config.maxDist);
                if (config.cellCount == 0)
                    config.cellCount = 1;
                config.cellSize = config.gridSize / config.cellCount;
                config.totalCellCount = config.cellCount * config.cellCount * config.cellCount;

                PrepareBuffers(config.nodesCount, config.totalCellCount, config.edgesCount);

                //----------------------- count node per cell -----------------------------
                GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
                GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf<ShaderConfig>(), ref config, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, cellCountBuffer);
                GL.ClearBufferData(BufferTarget.ShaderStorageBuffer, PixelInternalFormat.R32ui, PixelFormat.RedInteger, PixelType.UnsignedInt, IntPtr.Zero);
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, uboConfig);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBufferA);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, cellCountBuffer);
                GL.UseProgram(tilingCountProgram);
                GL.DispatchCompute(dispatchGroupsX, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                // ---------------------- prefix sum ----------------------------------------
                DownloadIntBuffer(cellCounts, cellCountBuffer, currentTotalCellsCount);
                if (cellCounts.Sum() == 0)
                    throw new Exception("counted 0");

                if (config.useCells == 0)
                {
                    long ops = MathUtil.PredictNumberOfOperations(config, cellCounts);
                    if (ops / config.nodesCount < (config.nodesCount / 10) || ops < 5000000)
                        config.useCells = 1;
                }

                //config.useCells = (config.totalCellCount >= 64) ? 1 : 0;
                if (config.useCells == 1)
                {
                    int sum = 0;
                    for (int c = 0; c < currentTotalCellsCount; c++)
                    {
                        cellOffsets[c] = sum;
                        sum += cellCounts[c];
                    }
                   
                    GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
                    GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf<ShaderConfig>(), ref config, BufferUsageHint.StaticDraw);

                    //----------------------------- fill -----------------------------------
                    UploadIntBuffer(cellOffsets, cellOffsetBuffer, currentTotalCellsCount);
                    GL.CopyNamedBufferSubData(cellOffsetBuffer, cellOffsetBuffer2, IntPtr.Zero, IntPtr.Zero, currentTotalCellsCount * sizeof(uint));
                    GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, uboConfig);
                    GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBufferA);
                    GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 7, cellOffsetBuffer);
                    GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 8, nodeIndicesBuffer);
                    GL.UseProgram(tilingBinProgram);
                    GL.DispatchCompute(dispatchGroupsX, 1, 1);
                    GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.ShaderImageAccessBarrierBit);

                    //DebugUtil.DebugSolver(false, config, this);
                }
            }
        }

        public void UploadGraph(ref ShaderConfig config, Node[] nodes, Edge[] edges)
        {
            lock (this)
            {
                PrepareBuffers(nodes.Length, currentTotalCellsCount, edges.Length);
                ComputeNeighbours(nodes, edges);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBufferA);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, nodes.Length * shaderPointStrideSize, nodes);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBufferB);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, nodes.Length * shaderPointStrideSize, nodes);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, edgesBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, edges.Length * Marshal.SizeOf<Edge>(), edges);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, neighboursBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, neighbours.Length * Marshal.SizeOf<uint>(), neighbours);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, neighboursStartBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, neighboursStart.Length * Marshal.SizeOf<uint>(), neighboursStart);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, neighboursCountBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, neighboursCount.Length * Marshal.SizeOf<uint>(), neighboursCount);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, restLengthsBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, restLengths.Length * Marshal.SizeOf<float>(), restLengths);
                config.useCells = 0;
                RunTiling(ref config);
            }
        }

        public void DownloadNodes(Node[] nodes, bool bufferB = false)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferB ? pointsBufferB : pointsBufferA);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, nodes.Length * Marshal.SizeOf<Node>(), nodes);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public Node GetTrackedParticle()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, trackingBuffer);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, Marshal.SizeOf<Node>(), ref trackedParticle);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            return trackedParticle;
        }

        public void DownloadIntBuffer(int[] buffer, int bufferId, int size)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferId);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, size * Marshal.SizeOf<int>(), buffer);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public void UploadIntBuffer(int[] buffer, int bufferId, int size)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferId);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, size * Marshal.SizeOf<int>(), buffer);
        }

        public void ComputeNeighbours(Node[] nodes, Edge[] edges)
        {
            Array.Clear(neighboursCount);
            for(int e=0; e<edges.Length; e++)
            {
                var edge = edges[e];
                neighboursCount[edge.a]++;
                neighboursCount[edge.b]++;
            }

            uint sum = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                neighboursStart[i] = sum;
                sum += neighboursCount[i];
            }

            var cursor = neighboursStart.ToArray();
            for (int i = 0; i < edges.Length; i++)
            {
                uint a = edges[i].a;
                uint b = edges[i].b;
                float restLen = edges[i].restLength;

                neighbours[cursor[a]] = b;
                restLengths[cursor[a]] = restLen;
                cursor[a]++;

                neighbours[cursor[b]] = a;
                restLengths[cursor[b]] = restLen;
                cursor[b]++;
            }
        }

        private void PrepareBuffers(int nodesCount, int totalCellsCount, int edgesCount)
        {
            if (currentNodesCount != nodesCount)
            {
                currentNodesCount = nodesCount;
                CreateBuffer(ref pointsBufferA, currentNodesCount, shaderPointStrideSize);
                CreateBuffer(ref pointsBufferB, currentNodesCount, shaderPointStrideSize);
                CreateBuffer(ref nodeIndicesBuffer, currentNodesCount, Marshal.SizeOf<int>());
                CreateBuffer(ref neighboursStartBuffer, currentNodesCount, Marshal.SizeOf<int>());
                CreateBuffer(ref neighboursCountBuffer, currentNodesCount, Marshal.SizeOf<int>());
                neighboursStart = new uint[nodesCount];
                neighboursCount = new uint[nodesCount];
            }

            if (currentTotalCellsCount != totalCellsCount)
            {
                currentTotalCellsCount = totalCellsCount;
                CreateBuffer(ref cellCountBuffer, currentTotalCellsCount, Marshal.SizeOf<int>());
                CreateBuffer(ref cellOffsetBuffer, currentTotalCellsCount, Marshal.SizeOf<int>());
                CreateBuffer(ref cellOffsetBuffer2, currentTotalCellsCount, Marshal.SizeOf<int>());
                cellCounts = new int[totalCellsCount];
                cellOffsets = new int[totalCellsCount];
            }

            if (currentEdgesCount != edgesCount)
            {
                currentEdgesCount = edgesCount;
                CreateBuffer(ref edgesBuffer, (int)edgesCount, Marshal.SizeOf<Edge>());
                CreateBuffer(ref neighboursBuffer, (int)edgesCount * 2, Marshal.SizeOf<uint>());
                CreateBuffer(ref restLengthsBuffer, (int)edgesCount * 2, Marshal.SizeOf<float>());
                neighbours = new uint[edgesCount * 2];
                restLengths = new float[edgesCount * 2];
            }
        }

        private void CreateBuffer(ref int bufferId, int elementCount, int elementSize, BufferTarget target = BufferTarget.ShaderStorageBuffer, BufferUsageHint hint = BufferUsageHint.DynamicDraw)
        {
            if (bufferId > 0)
            {
                GL.DeleteBuffer(bufferId);
                bufferId = 0;
            }
            GL.GenBuffers(1, out bufferId);
            GL.BindBuffer(target, bufferId);
            GL.BufferData(target, elementCount * elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
    }
}
