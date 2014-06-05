using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public struct CharGlyph {
        public char Character;
        public Vector2 Repetition;
        public Vector4 UVRect;
        public Vector2 Size;
    }

    public class SpriteFont : IDisposable {
        Rectangle[] glyphs;
        public CharGlyph[] Glyphs {
            get;
            private set;
        }
        public GLTexture Texture {
            get;
            private set;
        }
        public int FontHeight {
            get;
            private set;
        }
        private int regStart, regLength;

        public SpriteFont(string font, int size, char cs, char ce) {
            Font f = new Font(font, size, FontStyle.Regular, GraphicsUnit.Pixel);
            FontHeight = f.Height;
            regStart = cs;
            regLength = ce - cs + 1;
            int padding = size / 8;

            // First Measure All The Regions
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            glyphs = new Rectangle[ce - cs + 1];
            int i = 0;
            for(var c = cs; c <= ce; c++) {
                SizeF s;
                if(char.IsWhiteSpace(c)) {
                    s = g.MeasureString("_" + c + "_", f, new PointF(0, 0), StringFormat.GenericTypographic);
                    var s2 = g.MeasureString("__", f, new PointF(0, 0), StringFormat.GenericTypographic);
                    s.Width -= s2.Width;
                }
                else s = g.MeasureString(new string(c, 1), f, new PointF(0, 0), StringFormat.GenericTypographic);
                glyphs[i] = new Rectangle(0, 0, (int)(Math.Ceiling(s.Width) + 0.5), (int)(Math.Ceiling(s.Height) + 0.5));
                i++;
            }
            g.Dispose();
            bmp.Dispose();

            // Find Best Partitioning Of Glyphs
            int rows = 1, w, h, bestWidth = 0, bestHeight = 0, area = 4096 * 4096;
            List<int>[] bestPartition = null;
            while(rows <= glyphs.Length) {
                h = rows * (padding + FontHeight) + padding;
                var gr = CreateRows(glyphs, rows, padding, out w);

                // Desire A Power Of 2 Texture
                w = ClosestPow2(w);
                h = ClosestPow2(h);

                // A Texture Must Be Feasible
                if(w > 4096 || h > 4096) {
                    rows++;
                    continue;
                }

                // Check For Minimal Area
                if(area >= w * h) {
                    bestPartition = gr;
                    bestWidth = w;
                    bestHeight = h;
                    area = bestWidth * bestHeight;
                    rows++;
                }
                else {
                    break;
                }
            }

            // Can A Bitmap Font Be Made?
            if(bestPartition == null)
                return;

            // Now Draw All The Glyphs
            bmp = new Bitmap(bestWidth, bestHeight);
            g = Graphics.FromImage(bmp);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent);
            int ly = padding;
            Brush b = new SolidBrush(Color.White);
            for(int ri = 0; ri < bestPartition.Length; ri++) {
                int lx = padding;
                for(int ci = 0; ci < bestPartition[ri].Count; ci++) {
                    int gi = bestPartition[ri][ci];
                    g.DrawString(new string((char)(cs + gi), 1), f, b, lx, ly, StringFormat.GenericTypographic);
                    glyphs[gi].X = lx;
                    glyphs[gi].Y = ly;
                    lx += glyphs[gi].Width + padding;
                }
                ly += FontHeight + padding;
            }
            // Draw The Unsupported Glyph
            var pen = new Pen(Color.White, 1);
            int rs = padding - 1;
            g.DrawRectangle(pen, 0, 0, rs, rs);
            g.DrawLine(pen, 0, 0, rs, rs);
            g.Dispose();

            // Get The Color Information
            Texture = new GLTexture(TextureTarget.Texture2D, true);
            Texture.InternalFormat = PixelInternalFormat.Rgba;
            byte[] fontData = new byte[bmp.Width * bmp.Height * 4];
            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Texture.SetImage2D(bmpData, false);
            bmp.UnlockBits(bmpData);
            bmp.Dispose();

            // Create SpriteBatch Glyphs
            Glyphs = new CharGlyph[glyphs.Length + 1];
            for(i = 0; i < glyphs.Length; i++) {
                Glyphs[i].Character = (char)(cs + i);
                Glyphs[i].Size = new Vector2(glyphs[i].Width, glyphs[i].Height);
                Glyphs[i].Repetition = Vector2.One;
                Glyphs[i].UVRect = new Vector4(
                    (float)glyphs[i].X / (float)bestWidth,
                    (float)glyphs[i].Y / (float)bestHeight,
                    (float)glyphs[i].Width / (float)bestWidth,
                    (float)glyphs[i].Height / (float)bestHeight
                    );
            }
            Glyphs[glyphs.Length].Character = ' ';
            Glyphs[glyphs.Length].Size = Glyphs[0].Size;
            Glyphs[glyphs.Length].Repetition = new Vector2(2, 2);
            Glyphs[glyphs.Length].UVRect = new Vector4(
                0,
                0,
                (float)rs / (float)bestWidth,
                (float)rs / (float)bestHeight
                );
        }
        public void Dispose() {
            if(Texture != null) {
                Texture.Dispose();
                Texture = null;
            }
        }

        private static List<int>[] CreateRows(Rectangle[] rects, int r, int padding, out int w) {
            // Blank Initialize
            var l = new List<int>[r];
            int[] cw = new int[r];
            for(int i = 0; i < r; i++) {
                l[i] = new List<int>();
                cw[i] = padding;
            }

            // Loop Through All Glyphs
            for(int i = 0; i < rects.Length; i++) {
                // Find Row For Placement
                int ri = 0;
                for(int rii = 1; rii < cw.Length; rii++)
                    if(cw[rii] < cw[ri]) ri = rii;

                // Add Width To That Row
                cw[ri] += rects[i].Width + padding;

                // Add Glyph To The Row List
                l[ri].Add(i);
            }

            // Find The Max Width
            w = cw.Max();

            return l;
        }
        private static int ClosestPow2(int i) {
            i--;
            int pi = 1;
            while(i > 0) {
                i >>= 1;
                pi <<= 1;
            }
            return pi;
        }

        public Vector2 MeasureString(string s) {
            Vector2 size = new Vector2(0, FontHeight);
            float cw = 0;
            for(int si = 0; si < s.Length; si++) {
                char c = s[si];
                if(s[si] == '\n') {
                    size.Y += FontHeight;
                    if(size.X < cw)
                        size.X = cw;
                    cw = 0;
                }
                else {
                    // Check For Correct Glyph
                    int gi = c - regStart;
                    if(gi < 0 || gi >= regLength)
                        gi = regLength;
                    cw += Glyphs[gi].Size.X;
                }
            }
            if(size.X < cw)
                size.X = cw;
            return size;
        }

        public void Draw(SpriteBatch batch, string s, Vector2 position, Vector2 scaling, Vector4 tint, float depth) {
            Vector2 tp = position;
            for(int si = 0; si < s.Length; si++) {
                char c = s[si];
                if(s[si] == '\n') {
                    tp.Y += FontHeight * scaling.Y;
                    tp.X = position.X;
                }
                else {
                    // Check For Correct Glyph
                    int gi = c - regStart;
                    if(gi < 0 || gi >= regLength)
                        gi = regLength;
                    batch.Draw(Texture, Glyphs[gi].UVRect, Glyphs[gi].Repetition, tp, Glyphs[gi].Size * scaling, tint, depth);
                    tp.X += Glyphs[gi].Size.X * scaling.X;
                }
            }
        }
    }
}