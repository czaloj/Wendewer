using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using EGL;

namespace Wdw.GLView {
    public class TextureView : IDisposable {
        private GLTexture texture;

        public TextureView() {
            texture = null;
        }
        public void Dispose() {
            if(texture != null) {
                texture.Dispose();
                texture = null;
            }
        }

        public void Build(string file) {
            Dispose();

            texture = new GLTexture().Init();
            texture.InternalFormat = PixelInternalFormat.Rgba;
            texture.SetImage2D(file, true);
        }

        public void Bind(TextureUnit i, int unSampler) {
            texture.Use(i, unSampler);
            SamplerState.PointWrap.Set(texture.Target);
        }
        public void Unbind() {
            texture.Unuse();
        }
    }
}