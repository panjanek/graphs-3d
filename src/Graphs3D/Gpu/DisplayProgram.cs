using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Graphs3D.Utils;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Graphs3D.Gpu
{
    public class DisplayProgram
    {
        private int nodesProgram;

        private int projNodesLocation;

        private int viewportSizeLocation;

        private int particleSizeLocation;

        private int viewLocation;

        private int trackedPosLocation;

        private int quadVao;

        private int quadVbo;

        private int quadEbo;

        private int edgesVAO;

        private int edgesProgram;

        private int projEdgesLocation;


        public DisplayProgram()
        {
            nodesProgram = ShaderUtil.CompileAndLinkRenderShader("nodes.vert", "nodes.frag");

            projNodesLocation = GL.GetUniformLocation(nodesProgram, "projection");
            if (projNodesLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");
            particleSizeLocation = GL.GetUniformLocation(nodesProgram, "paricleSize");
            if (particleSizeLocation == -1) throw new Exception("Uniform 'paricleSize' not found. Shader optimized it out?");
            viewportSizeLocation = GL.GetUniformLocation(nodesProgram, "viewportSize");
            if (viewportSizeLocation == -1) throw new Exception("Uniform 'viewportSize' not found. Shader optimized it out?");
            viewLocation = GL.GetUniformLocation(nodesProgram, "view");
            if (viewLocation == -1) throw new Exception("Uniform 'view' not found. Shader optimized it out?");
            trackedPosLocation = GL.GetUniformLocation(nodesProgram, "trackedPos");
            if (trackedPosLocation == -1) throw new Exception("Uniform 'trackedPos' not found. Shader optimized it out?");

            edgesProgram = ShaderUtil.CompileAndLinkRenderShader("edges.vert", "edges.frag");
            projEdgesLocation = GL.GetUniformLocation(edgesProgram, "projection");
            if (projEdgesLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");

            float[] quad =
                {
                    -1, -1,
                     1, -1,
                     1,  1,
                    -1,  1
                };

            uint[] indices = { 0, 1, 2, 2, 3, 0 };

            quadVao = GL.GenVertexArray();
            quadVbo = GL.GenBuffer();
            quadEbo = GL.GenBuffer();
            edgesVAO = GL.GenVertexArray();

            GL.BindVertexArray(quadVao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(5, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(5);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, quadEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(
                BlendingFactor.SrcAlpha,
                BlendingFactor.OneMinusSrcAlpha
            );


        }

        public void Run(int nodesBuffer, int edgesBuffer, Matrix4 projectionMatrix, int particlesCount, float particleSize, Vector2 viewportSize, Matrix4 view, Vector4 trackedPos, int edgesCount)
        {
            GL.Clear(
                ClearBufferMask.ColorBufferBit |
                ClearBufferMask.DepthBufferBit
            );

            GL.UseProgram(nodesProgram);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, nodesBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, edgesBuffer);
            GL.BindVertexArray(quadVao);
            GL.UniformMatrix4(projNodesLocation, false, ref projectionMatrix);
            GL.Uniform1(particleSizeLocation, particleSize);
            GL.Uniform2(viewportSizeLocation, viewportSize);
            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.Uniform4(trackedPosLocation, ref trackedPos);

            GL.DrawElementsInstanced(
                PrimitiveType.Triangles,
                6,
                DrawElementsType.UnsignedInt,
                IntPtr.Zero,
                particlesCount * 1
            );


            GL.UseProgram(edgesProgram);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, nodesBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, edgesBuffer);
            GL.BindVertexArray(edgesVAO);
            var projection = view * projectionMatrix;
            GL.UniformMatrix4(projEdgesLocation, false, ref projection);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.LineWidth(3f);
            GL.DrawArrays(PrimitiveType.Lines, 0, edgesCount * 2);
        }
    }
}
