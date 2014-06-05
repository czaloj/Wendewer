using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using EGL;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Wdw.GLView.UI {
    public class PickEvent : EventArgs {
        public uint UUID;
        public Vector3 Location;

        public PickEvent(uint id, Vector3 l) {
            UUID = id;
            Location = l;
        }
    }

    public class RenderPanel : RectButton {
        private static readonly Vector4 COLOR_INACTIVE = new Vector4(1, 1, 1, 0.4f);
        private static readonly Vector4 COLOR_ACTIVE = new Vector4(1, 1, 1, 1);

        private static readonly Vector3 DEFAULT_EYE = new Vector3(0, 0, 10);
        private static readonly Vector3 DEFAULT_TARGET = new Vector3(0, 0, 0);
        private static readonly Vector3 DEFAULT_UP = new Vector3(0, 1, 0);

        private const float DEFAULT_FOV = 1.5f;
        private const float DEFAULT_ZNEAR = 0.01f;
        private const float DEFAULT_ZFAR = 800f;

        private Renderer renderer;
        private Matrix4 mView, mProj;
        private GLRenderTarget rt;

        public bool DrawWireframe {
            get;
            set;
        }

        bool shouldPick;
        Vector2 pickLoc;

        public event EventHandler<PickEvent> OnPick;

        public RenderPanel(WidgetRenderer wr, int w, int h, Renderer _renderer, int rw, int rh)
            : base(wr, w, h, COLOR_INACTIVE, COLOR_ACTIVE) {
            // This Panel's Texture Will Be The Render Target
            rt = new GLRenderTarget(true);
            rt.SetImage(rw, rh, PixelFormat.Rgba, PixelType.UnsignedByte, false);
            rt.BuildRenderTarget();
            Texture = rt;

            // The Panel Will Use A Default Rendering Configuration
            renderer = _renderer;
            DrawWireframe = false;
            mView = Matrix4.LookAt(
                DEFAULT_EYE,
                DEFAULT_TARGET,
                DEFAULT_UP
                );
            mProj = Matrix4.CreatePerspectiveFieldOfView(
                DEFAULT_FOV,
                (float)rt.Width / (float)rt.Height,
                DEFAULT_ZNEAR,
                DEFAULT_ZFAR
                );

            MouseEventDispatcher.OnMousePress += OnMP;
        }
        protected override void DisposeOther() {
            MouseEventDispatcher.OnMousePress -= OnMP;
            base.DisposeOther();
            rt.Dispose();
        }

        void OnMP(object sender, MouseButtonEventArgs e) {
            if(e.Button != MouseButton.Right)
                return;

            if(Inside(e.X, e.Y)) {
                pickLoc = new Vector2(e.X, e.Y);
                pickLoc.X -= X; pickLoc.X /= Width;
                pickLoc.Y -= Y; pickLoc.Y /= Height;
                shouldPick = true;
            }
        }

        public void SetCamera(Matrix4 mV, Matrix4 mP) {
            mView = mV;
            mProj = mP;
        }

        public void Render() {
            if(shouldPick) {
                shouldPick = false;
                Vector3 wPos;
                uint uuid = renderer.Pick(mView, mProj, pickLoc.X, pickLoc.Y, out wPos, DrawWireframe);
                if(uuid != uint.MaxValue && OnPick != null)
                    OnPick(this, new PickEvent(uuid, wPos));
            }

            rt.UseTarget();
            GL.ClearColor(0, 0, 0, 0);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderer.Draw(mView, mProj, DrawWireframe);
            rt.UnuseTarget();
        }
    }
}