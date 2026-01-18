using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Xps;
using Graphs3D.Gui;
using Graphs3D.Models;
using Graphs3D.Utils;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AppContext = Graphs3D.Models.AppContext;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Graphs3D.Gpu
{
    public class OpenGlRenderer
    {
        public const float ForwardSpeed = 0.1f;

        public const float DirectionChangeSpeed = 0.003f;

        public const int ClickingRadius = 7;

        public int FrameCounter => frameCounter;

        public bool Paused { get; set; }

        public int? DraggedIdx { get; set; }

        public int? SelectedIdx { get; private set; }

        public int? AnimatingIdx { get; private set; }

        private Panel placeholder;

        private System.Windows.Forms.Integration.WindowsFormsHost host;

        private GLControl glControl;

        private int frameCounter;

        private SolverProgram solverProgram;

        private DisplayProgram displayProgram;

        private float cameraDist;

        private Vector4 targetPos;

        private Vector4 cameraPos;

        private Vector4 freePos;

        private Vector4 prevCenterOfMass;

        private double xzAngle = 0;

        private double yAngle = 0;

        private AppContext app;

        public byte[] captureBuffer;

        private int? recFrameNr;

        private AnimationTimer animation;

        public OpenGlRenderer(Panel placeholder, AppContext app)
        {
            this.placeholder = placeholder;
            this.app = app;
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            host.Visibility = Visibility.Visible;
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;
            glControl = new GLControl(new GLControlSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3), // OpenGL 3.3
                Profile = ContextProfile.Compatability,
                Flags = ContextFlags.Default,
                IsEventDriven = false,
                NumberOfSamples = 8
            });
            glControl.Dock = DockStyle.Fill;
            host.Child = glControl;
            placeholder.Children.Add(host);

            //setup required features
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);

            solverProgram = new SolverProgram();
            displayProgram = new DisplayProgram();

            var dragging = new DraggingHandler(glControl, (mousePos, btn) =>
            {
                if (app.positionDrawn && mousePos.X <= AppContext.PosWidth && mousePos.Y >= glControl.Height - AppContext.PosHeight)
                    return false;

                //if (app.configWindow.NatigationMode != 2)

                var selectedIdx = GetClickedNodeIndex((int)mousePos.X, (int)mousePos.Y);
                if (selectedIdx.HasValue)
                    DraggedIdx = selectedIdx;

                return true;
            }, (prev, curr, btn) =>
            {
                var delta = (curr - prev);
                if (btn == MouseButtons.Right)
                {
                    // change camera angle
                    xzAngle -= (delta.X) * DirectionChangeSpeed;
                    yAngle -= (delta.Y) * DirectionChangeSpeed;
                    yAngle = Math.Clamp(yAngle, -Math.PI*0.48, Math.PI * 0.48);
                }
                else
                {
                    var dragged = DraggedIdx;
                    if (dragged.HasValue)
                    {
                        //translate dragged node by world coorditanes translation
                        Vector3 planeNormal = GetCameraDirection().Xyz;
                        planeNormal.Normalize();;
                        Vector3 camPos = cameraPos.Xyz;
                        Vector3 planePoint = app.simulation.nodes[dragged.Value].position.Xyz;
                        Vector3 ray0 = GpuUtil.ScreenToWorldRay(new Vector2(prev.X, prev.Y), GetViewMatrix(), GetProjectionMatrix(), glControl.Width, glControl.Height);
                        Vector3 ray1 = GpuUtil.ScreenToWorldRay(new Vector2(curr.X, curr.Y), GetViewMatrix(), GetProjectionMatrix(), glControl.Width, glControl.Height);
                        Vector3 p0 = GpuUtil.IntersectRayPlane(camPos, ray0, planePoint, planeNormal);
                        Vector3 p1 = GpuUtil.IntersectRayPlane(camPos, ray1, planePoint, planeNormal);
                        var translation = p1 - p0;
                        solverProgram.DownloadNodes(app.simulation.nodes);
                        app.simulation.nodes[dragged.Value].position += new Vector4(translation, 0);
                        solverProgram.UploadGraph(ref app.simulation.config, app.simulation.nodes, app.simulation.edges, app.simulation.nodeFlags);
                    }
                    else
                    {
                        // translating camera in a plane perpendicular to the current cammera direction
                        var forward = GetCameraDirection();
                        forward.Normalize();
                        Vector3 right = Vector3.Normalize(Vector3.Cross(forward.Xyz, Vector3.UnitY));
                        Vector3 up = Vector3.Cross(right, forward.Xyz);
                        var translation = -right * delta.X + up * delta.Y;
                        //cameraPos += new Vector4(translation.X, translation.Y, translation.Z, 0);
                        freePos += new Vector4(translation.X, translation.Y, translation.Z, 0);
                    }
                }

            }, () => { DraggedIdx = null; });

            glControl.MouseWheel += (s, e) =>
            {
                var delta = e.Delta * ForwardSpeed;
                if (app.configWindow.NavigationMode == 2)
                {
                    //free navigation: moving forward/backward current camera direction
                    //cameraPos += GetCameraDirection() * delta;
                    freePos += GetCameraDirection() * delta;
                }
                else
                {
                    //locked on some node: only change follow distance
                    app.configWindow.SetAutomaticDistance(false);
                    app.simulation.followDistance -= delta;
                    if (app.simulation.followDistance < 10)
                        app.simulation.followDistance = 10;
                }
            };

            cameraDist = app.simulation.followDistance;
            targetPos = new Vector4(app.simulation.config.fieldSize / 2f, app.simulation.config.fieldSize / 2f, app.simulation.config.fieldSize / 2f, 0);
            cameraPos = new Vector4(app.simulation.config.fieldSize/2f, app.simulation.config.fieldSize / 2f, app.simulation.config.fieldSize / 2f - cameraDist, 0);
            prevCenterOfMass = cameraPos;
            glControl.MouseDown += GlControl_Clicked;
            glControl.Paint += GlControl_Paint;
            glControl.SizeChanged += GlControl_SizeChanged;
            GlControl_SizeChanged(this, null);
        }

        private void GlControl_Clicked(object? sender, MouseEventArgs e)
        {
            if (animation != null)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (app.positionDrawn && e.X <= AppContext.PosWidth && e.Y >= glControl.Height - AppContext.PosHeight)
                    app.simulation.graph.Click(e.X, e.Y - glControl.Height + AppContext.PosHeight);

                var newIdx = GetClickedNodeIndex(e.X, e.Y);
                if (newIdx.HasValue)
                {
                    var currentIdx = SelectedIdx;
                    if (!currentIdx.HasValue)
                        Select(newIdx.Value);
                    else
                        AnimateTo(newIdx.Value);
                }
            }
        }

        public void AnimateTo(int targetIdx)
        {
            if (animation != null)
                return;

            if (targetIdx < 0 || targetIdx >= app.simulation.nodes.Length)
                return;

            var path = app.simulation.FindPath(SelectedIdx ?? 0, targetIdx);
            if (path.Count == 0)
                return;

            if (path.Count == 1)
                Select(path[0]);

            var stageLen = 1.0 / (path.Count - 1);
            int currPos = path.First();
            AnimatingIdx = currPos;
            animation = new AnimationTimer(0.04, 500 + 200 * (path.Count - 1), (progress, counter) =>
            {
                var nr = (int)Math.Floor(progress / stageLen);
                if (nr < 0 || nr > path.Count - 2)
                    return;
                var nextPos = path[nr + 1];
                if (nextPos != currPos)
                {
                    currPos = nextPos;
                    AnimatingIdx = currPos;
                    app.DrawPosition(currPos);
                    WpfUtil.DispatchRender(placeholder.Dispatcher, () => { app.SetupPathHighlight(currPos); });
                }

                app.simulation.config.marker1 = path[nr];
                app.simulation.config.marker2 = path[nr + 1];
                app.simulation.config.markerT = (float)((progress - nr * stageLen) / stageLen);
            },
            () =>
            {
                AnimatingIdx = null;
                animation = null;
                Select(path.Last());
            });
        }

        public void Select(int idx, bool expandAndHighlight = true)
        {
            if (animation != null)
                return; 

            SelectedIdx = idx;
            app.simulation.config.marker1 = SelectedIdx ?? -1;
            app.simulation.config.marker2 = SelectedIdx ?? -1;
            app.simulation.config.markerT = 1.0f;
            app.DrawPosition(idx);

            if (expandAndHighlight)
            {
                WpfUtil.DispatchRender(placeholder.Dispatcher, () =>
                {
                    lock (solverProgram)
                    {
                        app.SetupPathHighlight();
                        app.ExpandOne(idx);
                    }
                });
            }
        }

        private int? GetClickedNodeIndex(int mouseX, int mouseY)
        {
            DownloadNodes();
            int? selectedIdx = null;
            float minDepth = app.simulation.config.fieldSize * 10;
            var projectionMatrix = GetCombinedProjectionMatrix();
            for (int idx = 0; idx < app.simulation.nodes.Length; idx++)
            {
                var particlePosition = app.simulation.nodes[idx].position;
                var screenAndDepth = GpuUtil.World3DToScreenWithDepth(particlePosition.Xyz, projectionMatrix, glControl.Width, glControl.Height);
                if (screenAndDepth.HasValue)
                {
                    var screen = screenAndDepth.Value.screen;
                    var depth = screenAndDepth.Value.depth;
                    var distance = Math.Sqrt((screen.X - mouseX) * (screen.X - mouseX) +
                                             (screen.Y - mouseY) * (screen.Y - mouseY));
                    if (distance < ClickingRadius && depth < minDepth)
                    {
                        selectedIdx = idx;
                        minDepth = depth;
                    }

                }
            }

            return selectedIdx;
        }

        private Vector4 GetCameraDirection() => new Vector4((float)(Math.Cos(yAngle) * Math.Sin(xzAngle)), (float)(Math.Sin(yAngle)), (float)(Math.Cos(yAngle) * Math.Cos(xzAngle)), 0);

        private Matrix4 GetViewMatrix() => Matrix4.LookAt(cameraPos.Xyz, (cameraPos + GetCameraDirection()).Xyz, Vector3.UnitY);

        private Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), glControl.Width / (float)glControl.Height, 0.1f, 5000f);

        private Matrix4 GetCombinedProjectionMatrix() => GetViewMatrix() * GetProjectionMatrix();

        private void AutomaticCameraMovement()
        {
            if (app.configWindow.NavigationMode == 0) //center
            {
                if (frameCounter % 100 == 0)
                    AdaptCameraDistanceToGraphSize();

                var currentCenterOfMass = solverProgram.GetCenterOfMass();
                PositionCameraTo(currentCenterOfMass);
                prevCenterOfMass = currentCenterOfMass;
            }
            else if (app.configWindow.NavigationMode == 1) //selected node
            {
                PositionCameraTo(solverProgram.GetTrackedParticle().position);
            }
            else //free navigation
            {
                float step = 1f / app.simulation.cameraPeriod;
                cameraPos = cameraPos * (1-step) + freePos * step;
            }
        }

        public void StartFreeNavigation()
        {
            freePos = cameraPos;
        }

        private void PositionCameraTo(Vector4 toTarget)
        {
            if (app.configWindow.NavigationMode <= 1)
                xzAngle -= app.simulation.rotationSpeed;

            float step = 1f / app.simulation.cameraPeriod;
            targetPos = targetPos * (1-step) + toTarget * step;
            cameraDist = cameraDist * (1-step) + app.simulation.followDistance * step;

            cameraPos = targetPos - GetCameraDirection() * cameraDist;
        }

        public void AdaptCameraDistanceToGraphSize()
        {          
            if (app.configWindow.NavigationMode == 0 && app.configWindow.AutomaticDistance)
            {
                var currDist = app.simulation.followDistance;
                app.simulation.followDistance = 100;
                var massCenter = solverProgram.GetCenterOfMass();
                solverProgram.DownloadNodes(app.simulation.nodes);
                float maxD = 0;
                for (int i = 0; i < app.simulation.nodes.Length; i++)
                {
                    var d = Math.Sqrt((app.simulation.nodes[i].position.X - massCenter.X) * (app.simulation.nodes[i].position.X - massCenter.X) +
                                      (app.simulation.nodes[i].position.Y - massCenter.Y) * (app.simulation.nodes[i].position.Y - massCenter.Y) +
                                      (app.simulation.nodes[i].position.Z - massCenter.Z) * (app.simulation.nodes[i].position.Z - massCenter.Z));
                    if (d > maxD) maxD = (float)d;
                }
                if (maxD * 1.3f > app.simulation.followDistance)
                    app.simulation.followDistance = maxD * 1.3f;
            }
        }

        public void ResetOrigin()
        {
            cameraPos = new Vector4(app.simulation.config.fieldSize / 2, app.simulation.config.fieldSize / 2, -1.5f*app.simulation.config.fieldSize, 1.0f);
            xzAngle = 0;
            yAngle = 0;
            GlControl_SizeChanged(this, null);
        }

        public void UploadGraph()
        {
            solverProgram.UploadGraph(ref app.simulation.config, app.simulation.nodes, app.simulation.edges, app.simulation.nodeFlags);
            if (SelectedIdx.HasValue && SelectedIdx.Value >= app.simulation.nodes.Length)
                Select(0);
            if (SelectedIdx.HasValue)
                app.DrawPosition(SelectedIdx.Value);
        }

        public void ResetHighlighting(float alpha) => displayProgram.ResetHighlighting(alpha);

        public void UploadFlags() => solverProgram.UploadFlags(app.simulation.edges, app.simulation.nodeFlags);

        public void UploadImage(byte[] pixels) => displayProgram.UploadImage(pixels);

        public void DownloadNodes() => solverProgram.DownloadNodes(app.simulation.nodes);

        private void GlControl_SizeChanged(object? sender, EventArgs e)
        {
            if (app.simulation.nodes.Length == 0)
                return;

            if (glControl.Width <= 0 || glControl.Height <= 0)
                return;

            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            AutomaticCameraMovement();
            var trackedPos = solverProgram.GetTrackedParticle().position;
            displayProgram.Run(
                solverProgram.pointsBufferB,
                solverProgram.edgesBuffer, 
                solverProgram.nodeFlagsBuffer,
                GetProjectionMatrix(),
                new Vector2(glControl.Width, glControl.Height),
                GetViewMatrix(),
                trackedPos,
                app.positionDrawn && app.configWindow.ImageVisible,
                app.simulation);
            glControl.SwapBuffers();
            frameCounter++;
            Capture();
        }

        public void Step()
        {
            if (Application.Current.MainWindow == null || Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                return;

            //compute
            if (!Paused)
            {
                app.simulation.config.trackedIdx = AnimatingIdx ?? SelectedIdx ?? 0;
                solverProgram.Run(ref app.simulation.config, frameCounter%100 == 0);
            }

            glControl.Invalidate();
        }

        private void Capture()
        {
            //combine PNGs into video:
            //mp4: ffmpeg -f image2 -framerate 60 -i rec/frame_%05d.png -vf "scale=trunc(iw/2)*2:trunc(ih/2)*2" -r 60 -vcodec libx264 -pix_fmt yuv420p out.mp4 -y
            //gif: ffmpeg -framerate 60 -ss2 -i rec/frame_%05d.png -vf "select='not(mod(n,2))',setpts=N/FRAME_RATE/TB" -t 5 -r 20 simple2.gif
            //reduce bitrate:  ffmpeg -i in.mp4 -c:v libx264 -b:v 4236000 -pass 2 -c:a aac -b:a 128k out.mp4
            var recDir = app.configWindow.recordDir?.ToString();
            if (!recFrameNr.HasValue && !string.IsNullOrWhiteSpace(recDir))
            {
                recFrameNr = 0;
            }

            if (recFrameNr.HasValue && string.IsNullOrWhiteSpace(recDir))
                recFrameNr = null;

            if (recFrameNr.HasValue && !string.IsNullOrWhiteSpace(recDir))
            {
                string recFilename = $"{recDir}\\frame_{recFrameNr.Value.ToString("00000")}.png";
                glControl.MakeCurrent();
                int width = glControl.Width;
                int height = glControl.Height;
                int bufferSize = width * height * 4;
                if (captureBuffer == null || bufferSize != captureBuffer.Length)
                    captureBuffer = new byte[bufferSize];
                GL.ReadPixels(
                    0, 0,
                    width, height,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    captureBuffer
                );

                TextureUtil.FlipVertical(captureBuffer, width, height);
                TextureUtil.SaveBufferToFile(captureBuffer, width, height, recFilename);
                recFrameNr = recFrameNr.Value + 1;
            }
        }
    }
}
