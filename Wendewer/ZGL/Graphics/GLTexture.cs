using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using GLPixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace EGL {
    public class GLTexture : IDisposable {
        public int ID {
            get;
            private set;
        }

        public bool IsCreated {
            get { return ID != 0; }
        }

        private int[] dimensions;
        public int Width {
            get { return dimensions[0]; }
        }
        public int Height {
            get { return dimensions[1]; }
        }
        public int Depth {
            get { return dimensions[2]; }
        }

        public TextureTarget Target {
            get;
            set;
        }
        public bool IsBound {
            get;
            private set;
        }

        public PixelInternalFormat InternalFormat {
            get;
            set;
        }

        public GLTexture(TextureTarget target = TextureTarget.Texture2D, bool init = false) {
            ID = 0;
            dimensions = new int[] { 0, 0, 0 };

            Target = target;
            InternalFormat = PixelInternalFormat.Rgba;

            if(init) Init();
        }
        public void Dispose() {
            if(IsCreated) {
                GL.DeleteTexture(ID);
                ID = 0;
            }
        }

        public GLTexture Init() {
            if(IsCreated) return this;
            ID = GL.GenTexture();
            return this;
        }

        public void Bind() {
            if(IsBound) return;
            IsBound = true;
            GL.BindTexture(Target, ID);
        }
        public void Unbind() {
            if(!IsBound) return;
            IsBound = false;
            GL.BindTexture(Target, 0);
        }

        public void SetImage(int[] dim, GLPixelFormat pixelFormat, PixelType pixelType, IntPtr buf, bool mipMap = false) {
            if(dim == null || dim.Length < 1 || dim.Length > 3)
                throw new ArgumentException("Dimensions For The Texture Must Be Given (Must Be 1 - 3)");
            dim.CopyTo(dimensions, 0);

            int dims = 0;
            if(Width > 0) dims++;
            else goto SWITCH_BLOCK;
            if(Height > 0) dims++;
            else goto SWITCH_BLOCK;
            if(Depth > 0) dims++;
            else goto SWITCH_BLOCK;

        SWITCH_BLOCK:
            Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            switch(dims) {
                case 1:
                    GL.TexImage1D(Target, 0, InternalFormat, Width, 0, pixelFormat, pixelType, buf);
                    break;
                case 2:
                    if(mipMap)
                        GL.TexParameter(Target, TextureParameterName.GenerateMipmap, (int)All.True);
                    GL.TexImage2D(Target, 0, InternalFormat, Width, Height, 0, pixelFormat, pixelType, buf);
                    break;
                case 3:
                    GL.TexImage3D(Target, 0, InternalFormat, Width, Height, Depth, 0, pixelFormat, pixelType, buf);
                    break;
                default:
                    throw new ArgumentException("Invalid Dimensions For The Texture (Must Be > 0)");
            }
            Unbind();
        }
        public void SetImage(int[] dim, GLPixelFormat pixelFormat, PixelType pixelType, bool mipMap = false) {
            SetImage(dim, pixelFormat, pixelType, IntPtr.Zero, mipMap);
        }
        public void SetImage(int w, int h, int d, GLPixelFormat pixelFormat, PixelType pixelType, IntPtr buf, bool mipMap = false) {
            SetImage(new int[] { w, h, d }, pixelFormat, pixelType, buf, mipMap);
        }
        public void SetImage(int w, int h, GLPixelFormat pixelFormat, PixelType pixelType, IntPtr buf, bool mipMap = false) {
            SetImage(w, h, 0, pixelFormat, pixelType, buf, mipMap);
        }
        public void SetImage(int w, GLPixelFormat pixelFormat, PixelType pixelType, IntPtr buf, bool mipMap = false) {
            SetImage(w, 0, 0, pixelFormat, pixelType, buf, mipMap);
        }
        public void SetImage(int w, int h, int d, GLPixelFormat pixelFormat, PixelType pixelType, bool mipMap = false) {
            SetImage(w, h, d, pixelFormat, pixelType, IntPtr.Zero, mipMap);
        }
        public void SetImage(int w, int h, GLPixelFormat pixelFormat, PixelType pixelType, bool mipMap = false) {
            SetImage(w, h, 0, pixelFormat, pixelType, IntPtr.Zero, mipMap);
        }
        public void SetImage(int w, GLPixelFormat pixelFormat, PixelType pixelType, bool mipMap = false) {
            SetImage(w, 0, 0, pixelFormat, pixelType, IntPtr.Zero, mipMap);
        }
        public void SetImage2D(BitmapData data, bool mipMap = false) {
            SetImage(new int[] { data.Width, data.Height, 0 }, GLPixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0, mipMap);
        }
        public void SetImage2D(Stream s, bool mipMap = false) {
            var bmp = Bitmap.FromStream(s) as Bitmap;
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            SetImage2D(bmpData, mipMap);
            bmp.UnlockBits(bmpData);
            bmp.Dispose();
        }
        public void SetImage2D(string file, bool mipMap = false) {
            var bmp = Bitmap.FromFile(file) as Bitmap;
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            SetImage2D(bmpData, mipMap);
            bmp.UnlockBits(bmpData);
            bmp.Dispose();
        }

        public void BindToUnit(TextureUnit unit) {
            GL.ActiveTexture(unit);
            Bind();
        }
        public void SetUniformSampler(TextureUnit unit, int unSampler) {
            GL.Uniform1(unSampler, (int)unit - (int)TextureUnit.Texture0);
        }

        public void Use(TextureUnit unit, int unSampler) {
            BindToUnit(unit);
            SetUniformSampler(unit, unSampler);
        }
        public void Unuse() {
            Unbind();
        }
    }
}