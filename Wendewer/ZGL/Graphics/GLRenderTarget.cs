using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public class GLRenderTarget : GLTexture {
        private int fb, rb;

        public GLRenderTarget(bool init = false)
            : base(TextureTarget.Texture2D, init) {
        }

        public void BuildRenderTarget() {
            fb = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);

            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ID, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            rb = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rb);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rb);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            FramebufferErrorCode err = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if(err != FramebufferErrorCode.FramebufferComplete)
                return;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void UseTarget() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rb);
            GL.Viewport(0, 0, Width, Height);
        }
        public void UnuseTarget() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }
    }
}