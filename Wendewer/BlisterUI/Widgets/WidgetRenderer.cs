using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using EGL;

namespace BlisterUI.Widgets {
    public class WidgetRenderer : IDisposable {
        private GLTexture tPixel;
        public GLTexture DefaultTexture {
            get { return tPixel; }
        }
        private SpriteFont fDefault;
        public SpriteFont DefaultFont {
            get { return fDefault; }
        }

        private readonly List<DrawableRect> rects;
        private readonly List<DrawableText> texts;

        public WidgetRenderer(SpriteFont f) {
            tPixel = new GLTexture().Init();
            tPixel.InternalFormat = OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgba;
            tPixel.SetImage(
                new int[] { 1, 1, 0 },
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte,
                System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(new uint[] { 0xffffffff }, 0)
                );
            fDefault = f;

            rects = new List<DrawableRect>();
            texts = new List<DrawableText>();
        }
        public void Dispose() {
            tPixel.Dispose();
        }

        public void Add(DrawableRect r) {
            rects.Add(r);
        }
        public void Add(DrawableText t) {
            texts.Add(t);
        }
        public void Remove(DrawableRect r) {
            rects.Remove(r);
        }
        public void Remove(DrawableText t) {
            texts.Remove(t);
        }

        public void Draw(SpriteBatch batch, int w, int h) {
            batch.Begin();
            for(int i = 0; i < rects.Count; i++) {
                // Draw Rectangle
                batch.Draw(
                    rects[i].texture == null ? tPixel : rects[i].texture,
                    rects[i].location,
                    rects[i].size,
                    rects[i].color,
                    rects[i].layerDepth
                    );
            }
            for(int i = 0; i < texts.Count; i++) {
                // Draw Text
                if(texts[i].Font == null)
                    texts[i].Font = fDefault;
                batch.DrawString(
                    texts[i].Font,
                    texts[i].Text,
                    texts[i].location,
                    new Vector2(texts[i].TextScale),
                    texts[i].color,
                    texts[i].layerDepth
                    );
            }
            batch.End(SpriteSortMode.BackToFront);
            batch.RenderBatch(
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthState.None,
                RasterizerState.CullNone,
                Matrix4.Identity,
                SpriteBatch.CreateCameraFromWindow(w, h)
                );
        }
    }
}