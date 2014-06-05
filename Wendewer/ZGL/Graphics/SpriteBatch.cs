using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public struct VertexSpriteBatch {
        public static readonly int Size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexSpriteBatch));
        public static readonly ArrayBind[] Binds = new ArrayBind[]{
          new ArrayBind(Semantic.Position, VertexAttribPointerType.Float, 3, sizeof(float) * 0),
          new ArrayBind(Semantic.TexCoord, VertexAttribPointerType.Float, 2, sizeof(float) * 3),
          new ArrayBind(Semantic.TexCoord | Semantic.Index1, VertexAttribPointerType.Float, 4, sizeof(float) * 5),
          new ArrayBind(Semantic.Color, VertexAttribPointerType.Float, 4, sizeof(float) * 9)
        };

        public Vector3 Position;
        public Vector2 UV;
        public Vector4 UVRect;
        public Vector4 Color;

        public VertexSpriteBatch(Vector3 p, Vector2 uv, Vector4 uvr, Vector4 c) {
            Position = p;
            UV = uv;
            UVRect = uvr;
            Color = c;
        }
    }

    public enum SpriteSortMode {
        None,
        FrontToBack,
        BackToFront,
        Texture
    }

    public class SpriteGlyph {
        public GLTexture Texture;
        public float Depth;

        public VertexSpriteBatch VTL;
        public VertexSpriteBatch VTR;
        public VertexSpriteBatch VBL;
        public VertexSpriteBatch VBR;

        public SpriteGlyph(GLTexture t, float d) {
            Texture = t;
            Depth = d;
        }
    }
    public class SpriteBatchCall {
        public GLTexture Texture;
        public int Indices;
        public int IndexOffset;

        public SpriteBatchCall(int iOff, GLTexture t, List<SpriteBatchCall> calls) {
            Texture = t;
            IndexOffset = iOff;
            Indices = 4;
            calls.Add(this);
        }

        public SpriteBatchCall Append(SpriteGlyph g, List<SpriteBatchCall> calls) {
            if(g.Texture != Texture) return new SpriteBatchCall(IndexOffset + Indices, g.Texture, calls);
            else Indices += 4;
            return this;
        }
    }

    public class SpriteBatch {
        #region Sorters
        public static int SSMTexture(SpriteGlyph g1, SpriteGlyph g2) {
            return g1.Texture.ID.CompareTo(g2.Texture.ID);
        }
        public static int SSMFrontToBack(SpriteGlyph g1, SpriteGlyph g2) {
            return g1.Depth.CompareTo(g2.Depth);
        }
        public static int SSMBackToFront(SpriteGlyph g1, SpriteGlyph g2) {
            return g2.Depth.CompareTo(g1.Depth);
        }
        #endregion

        #region Vertex Shader
        private const string VS_SRC =
@"uniform mat4 World;
uniform mat4 VP;
in vec4 vPosition;  // Sem  (Position    0)
in vec2 vUV;        // Sem  (Texcoord    0)
in vec4 vUVRect;    // Sem  (Texcoord    1)
in vec4 vTint;      // Sem  (Color       0)
out vec2 fUV;
out vec4 fUVRect;
out vec4 fTint;
void main() {
    fTint = vTint;
    fUV = vUV;
    fUVRect = vUVRect;
    vec4 worldPos = vPosition * World;
    gl_Position = vPosition * VP;
}
";
        #endregion
        #region Fragment Shader
        private const string FS_SRC =
@"uniform sampler2D SBTex;
in vec2 fUV;
in vec4 fUVRect;
in vec4 fTint;
void main() {
    gl_FragColor = texture(SBTex, (vec2(mod(fUV.x, 1.0), mod(fUV.y, 1.0)) * fUVRect.zw) + fUVRect.xy) * fTint;
}
";
        #endregion

        public static readonly Vector4 FULL_UV_RECT = new Vector4(0, 0, 1, 1);
        public static readonly Vector2 UV_NO_TILE = new Vector2(1, 1);

        public static Matrix4 CreateCameraFromWindow(float w, float h) {
            w *= 0.5f;
            h *= 0.5f;
            Matrix4 mm = new Matrix4(
                w, 0, 0, 0,
                0, -h, 0, 0,
                0, 0, -1, 0,
                w, h, 0, 1
                ).Inverted();
            Matrix4 mo =
                Matrix4.CreateScale(1 / w, -1 / h, 1) *
                Matrix4.CreateTranslation(-1, 1, -1)
                ;
            return mo;
        }

        // Glyph Information
        private List<SpriteGlyph> glyphs;
        private Queue<SpriteGlyph> emptyGlyphs;

        // Render Batches
        private GLBuffer bufVerts;
        private List<SpriteBatchCall> batches;

        // Custom Shader
        private GLProgram program;
        private int unWorld, unVP, unTexture;
        private ShaderInterface si;

        public SpriteBatch(bool isDynamic = true) {
            BufferUsageHint bufUsage = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

            // Create Vertex Buffer
            bufVerts = new GLBuffer(BufferTarget.ArrayBuffer, bufUsage, true);
            bufVerts.SetAsVertex(VertexSpriteBatch.Size);

            emptyGlyphs = new Queue<SpriteGlyph>();

            program = new GLProgram(true);
            program.AddShader(ShaderType.VertexShader, VS_SRC);
            program.AddShader(ShaderType.FragmentShader, FS_SRC);
            program.Link();
            program.InitAttributes();
            si = new ShaderInterface(VertexSpriteBatch.Binds);
            si.Build(program.SemanticLinks);
            program.InitUniforms();
            unWorld = program.Uniforms["World"];
            unVP = program.Uniforms["VP"];
            unTexture = program.Uniforms["SBTex"];
        }

        public void Begin() {
            // Only Clear The Glyphs
            glyphs = new List<SpriteGlyph>();
            batches = new List<SpriteBatchCall>();
        }

        private SpriteGlyph CreateGlyph(GLTexture t, float d) {
            if(emptyGlyphs.Count > 0) {
                var g = emptyGlyphs.Dequeue();
                g.Texture = t;
                g.Depth = d;
                return g;
            }
            else {
                return new SpriteGlyph(t, d);
            }
        }
        public void Draw(GLTexture t, Vector4? uvRect, Vector2? uvTiling, Matrix4 mTransform, Vector4 tint, float depth = 0f) {
            Vector4 uvr = uvRect.HasValue ? uvRect.Value : FULL_UV_RECT;
            Vector2 uvt = uvTiling.HasValue ? uvTiling.Value : UV_NO_TILE;
            SpriteGlyph g = CreateGlyph(t, depth);


            g.VTL = new VertexSpriteBatch(new Vector3(0, 0, depth), new Vector2(0, 0), uvr, tint);
            g.VTR = new VertexSpriteBatch(new Vector3(1, 0, depth), new Vector2(uvt.X, 0), uvr, tint);
            g.VBL = new VertexSpriteBatch(new Vector3(0, 1, depth), new Vector2(0, uvt.Y), uvr, tint);
            g.VBR = new VertexSpriteBatch(new Vector3(1, 1, depth), uvt, uvr, tint);

            // Transform The Vertex Positions
            g.VTL.Position = Vector3.Transform(g.VTL.Position, mTransform);
            g.VTR.Position = Vector3.Transform(g.VTR.Position, mTransform);
            g.VBL.Position = Vector3.Transform(g.VBL.Position, mTransform);
            g.VBR.Position = Vector3.Transform(g.VBR.Position, mTransform);

            glyphs.Add(g);
        }
        public void Draw(GLTexture t, Vector4? uvRect, Vector2? uvTiling, Vector2 position, Vector2 offset, Vector2 size, float rotation, Vector4 tint, float depth = 0f) {
            Vector4 uvr = uvRect.HasValue ? uvRect.Value : FULL_UV_RECT;
            Vector2 uvt = uvTiling.HasValue ? uvTiling.Value : UV_NO_TILE;
            SpriteGlyph g = CreateGlyph(t, depth);

            float rxx = (float)Math.Cos(-rotation);
            float rxy = (float)Math.Sin(-rotation);
            float cl = size.X * (-offset.X);
            float cr = size.X * (1 - offset.X);
            float ct = size.Y * (-offset.Y);
            float cb = size.Y * (1 - offset.Y);

            g.VTL.Position.X = (cl * rxx) + (ct * rxy) + position.X;
            g.VTL.Position.Y = (cl * -rxy) + (ct * rxx) + position.Y;
            g.VTL.Position.Z = depth;
            g.VTL.UV.X = 0;
            g.VTL.UV.Y = 0;
            g.VTL.UVRect = uvr;
            g.VTL.Color = tint;

            g.VTR.Position.X = (cr * rxx) + (ct * rxy) + position.X;
            g.VTR.Position.Y = (cr * -rxy) + (ct * rxx) + position.Y;
            g.VTR.Position.Z = depth;
            g.VTR.UV.X = uvt.X;
            g.VTR.UV.Y = 0;
            g.VTR.UVRect = uvr;
            g.VTR.Color = tint;

            g.VBL.Position.X = (cl * rxx) + (cb * rxy) + position.X;
            g.VBL.Position.Y = (cl * -rxy) + (cb * rxx) + position.Y;
            g.VBL.Position.Z = depth;
            g.VBL.UV.X = 0;
            g.VBL.UV.Y = uvt.Y;
            g.VBL.UVRect = uvr;
            g.VBL.Color = tint;

            g.VBR.Position.X = (cr * rxx) + (cb * rxy) + position.X;
            g.VBR.Position.Y = (cr * -rxy) + (cb * rxx) + position.Y;
            g.VBR.Position.Z = depth;
            g.VBR.UV.X = uvt.X;
            g.VBR.UV.Y = uvt.Y;
            g.VBR.UVRect = uvr;
            g.VBR.Color = tint;

            glyphs.Add(g);
        }
        public void Draw(GLTexture t, Vector4? uvRect, Vector2? uvTiling, Vector2 position, Vector2 offset, Vector2 size, Vector4 tint, float depth = 0f) {
            Vector4 uvr = uvRect.HasValue ? uvRect.Value : FULL_UV_RECT;
            Vector2 uvt = uvTiling.HasValue ? uvTiling.Value : UV_NO_TILE;
            SpriteGlyph g = CreateGlyph(t, depth);

            float cl = size.X * (-offset.X);
            float cr = size.X * (1 - offset.X);
            float ct = size.Y * (-offset.Y);
            float cb = size.Y * (1 - offset.Y);

            g.VTL.Position.X = cl + position.X;
            g.VTL.Position.Y = ct + position.Y;
            g.VTL.Position.Z = depth;
            g.VTL.UV.X = 0;
            g.VTL.UV.Y = 0;
            g.VTL.UVRect = uvr;
            g.VTL.Color = tint;

            g.VTR.Position.X = cr + position.X;
            g.VTR.Position.Y = ct + position.Y;
            g.VTR.Position.Z = depth;
            g.VTR.UV.X = uvt.X;
            g.VTR.UV.Y = 0;
            g.VTR.UVRect = uvr;
            g.VTR.Color = tint;

            g.VBL.Position.X = cl + position.X;
            g.VBL.Position.Y = cb + position.Y;
            g.VBL.Position.Z = depth;
            g.VBL.UV.X = 0;
            g.VBL.UV.Y = uvt.Y;
            g.VBL.UVRect = uvr;
            g.VBL.Color = tint;

            g.VBR.Position.X = cr + position.X;
            g.VBR.Position.Y = cb + position.Y;
            g.VBR.Position.Z = depth;
            g.VBR.UV.X = uvt.X;
            g.VBR.UV.Y = uvt.Y;
            g.VBR.UVRect = uvr;
            g.VBR.Color = tint;

            glyphs.Add(g);
        }
        public void Draw(GLTexture t, Vector4? uvRect, Vector2? uvTiling, Vector2 position, Vector2 size, Vector4 tint, float depth = 0f) {
            Vector4 uvr = uvRect.HasValue ? uvRect.Value : FULL_UV_RECT;
            Vector2 uvt = uvTiling.HasValue ? uvTiling.Value : UV_NO_TILE;
            SpriteGlyph g = CreateGlyph(t, depth);

            g.VTL.Position.X = position.X;
            g.VTL.Position.Y = position.Y;
            g.VTL.Position.Z = depth;
            g.VTL.UV.X = 0;
            g.VTL.UV.Y = 0;
            g.VTL.UVRect = uvr;
            g.VTL.Color = tint;

            g.VTR.Position.X = size.X + position.X;
            g.VTR.Position.Y = position.Y;
            g.VTR.Position.Z = depth;
            g.VTR.UV.X = uvt.X;
            g.VTR.UV.Y = 0;
            g.VTR.UVRect = uvr;
            g.VTR.Color = tint;

            g.VBL.Position.X = position.X;
            g.VBL.Position.Y = size.Y + position.Y;
            g.VBL.Position.Z = depth;
            g.VBL.UV.X = 0;
            g.VBL.UV.Y = uvt.Y;
            g.VBL.UVRect = uvr;
            g.VBL.Color = tint;

            g.VBR.Position.X = size.X + position.X;
            g.VBR.Position.Y = size.Y + position.Y;
            g.VBR.Position.Z = depth;
            g.VBR.UV.X = uvt.X;
            g.VBR.UV.Y = uvt.Y;
            g.VBR.UVRect = uvr;
            g.VBR.Color = tint;

            glyphs.Add(g);
        }
        public void Draw(GLTexture t, Vector4? uvRect, Vector2 position, Vector2 size, Vector4 tint, float depth = 0f) {
            Vector4 uvr = uvRect.HasValue ? uvRect.Value : FULL_UV_RECT;
            SpriteGlyph g = CreateGlyph(t, depth);

            g.VTL.Position.X = position.X;
            g.VTL.Position.Y = position.Y;
            g.VTL.Position.Z = depth;
            g.VTL.UV.X = 0;
            g.VTL.UV.Y = 0;
            g.VTL.UVRect = uvr;
            g.VTL.Color = tint;

            g.VTR.Position.X = size.X + position.X;
            g.VTR.Position.Y = position.Y;
            g.VTR.Position.Z = depth;
            g.VTR.UV.X = 1;
            g.VTR.UV.Y = 0;
            g.VTR.UVRect = uvr;
            g.VTR.Color = tint;

            g.VBL.Position.X = position.X;
            g.VBL.Position.Y = size.Y + position.Y;
            g.VBL.Position.Z = depth;
            g.VBL.UV.X = 0;
            g.VBL.UV.Y = 1;
            g.VBL.UVRect = uvr;
            g.VBL.Color = tint;

            g.VBR.Position.X = size.X + position.X;
            g.VBR.Position.Y = size.Y + position.Y;
            g.VBR.Position.Z = depth;
            g.VBR.UV.X = 1;
            g.VBR.UV.Y = 1;
            g.VBR.UVRect = uvr;
            g.VBR.Color = tint;

            glyphs.Add(g);
        }
        public void Draw(GLTexture t, Vector2 position, Vector2 size, Vector4 tint, float depth = 0f) {
            SpriteGlyph g = CreateGlyph(t, depth);

            g.VTL.Position.X = position.X;
            g.VTL.Position.Y = position.Y;
            g.VTL.Position.Z = depth;
            g.VTL.UV.X = 0;
            g.VTL.UV.Y = 0;
            g.VTL.UVRect = FULL_UV_RECT;
            g.VTL.Color = tint;

            g.VTR.Position.X = size.X + position.X;
            g.VTR.Position.Y = position.Y;
            g.VTR.Position.Z = depth;
            g.VTR.UV.X = 1;
            g.VTR.UV.Y = 0;
            g.VTR.UVRect = FULL_UV_RECT;
            g.VTR.Color = tint;

            g.VBL.Position.X = position.X;
            g.VBL.Position.Y = size.Y + position.Y;
            g.VBL.Position.Z = depth;
            g.VBL.UV.X = 0;
            g.VBL.UV.Y = 1;
            g.VBL.UVRect = FULL_UV_RECT;
            g.VBL.Color = tint;

            g.VBR.Position.X = size.X + position.X;
            g.VBR.Position.Y = size.Y + position.Y;
            g.VBR.Position.Z = depth;
            g.VBR.UV.X = 1;
            g.VBR.UV.Y = 1;
            g.VBR.UVRect = FULL_UV_RECT;
            g.VBR.Color = tint;

            glyphs.Add(g);
        }
        public void DrawString(SpriteFont font, string s, Vector2 position, Vector2 scaling, Vector4 tint, float depth = 0f) {
            font.Draw(this, s, position, scaling, tint, depth);
        }
        public void DrawString(SpriteFont font, string s, Vector2 position, float desiredHeight, float scaleX, Vector4 tint, float depth = 0f) {
            Vector2 scaling = new Vector2(desiredHeight / font.FontHeight);
            scaling.X *= scaleX;
            font.Draw(this, s, position, scaling, tint, depth);
        }

        private void SortGlyphs(SpriteSortMode ssm) {
            if(glyphs.Count < 1) return;

            int i = 0, id, c = 1;
            switch(ssm) {
                case SpriteSortMode.Texture:
                    glyphs.Sort(SSMTexture);
                    break;
                case SpriteSortMode.FrontToBack:
                    glyphs.Sort(SSMFrontToBack);
                    break;
                case SpriteSortMode.BackToFront:
                    glyphs.Sort(SSMBackToFront);
                    break;
                default:
                    break;
            }
        }
        private void GenerateBatches() {
            if(glyphs.Count < 1) return;

            // Create Arrays
            VertexSpriteBatch[] verts = new VertexSpriteBatch[4 * glyphs.Count];
            int vi = 0;

            var call = new SpriteBatchCall(0, glyphs[0].Texture, batches);
            verts[vi++] = glyphs[0].VTL;
            verts[vi++] = glyphs[0].VTR;
            verts[vi++] = glyphs[0].VBR;
            verts[vi++] = glyphs[0].VBL;

            int gc = glyphs.Count;
            for(int i = 1; i < gc; i++) {
                var glyph = glyphs[i];
                call = call.Append(glyph, batches);
                verts[vi++] = glyph.VTL;
                verts[vi++] = glyph.VTR;
                verts[vi++] = glyph.VBR;
                verts[vi++] = glyph.VBL;
                emptyGlyphs.Enqueue(glyphs[i]);
            }
            glyphs = null;

            // Set The Buffer Data
            bufVerts.SmartSetData(verts, VertexSpriteBatch.Size);
        }
        public void End(SpriteSortMode ssm) {
            SortGlyphs(ssm);
            GenerateBatches();
        }

        public void RenderBatch(BlendState bs, SamplerState ss, DepthState ds, RasterizerState rs, Matrix4 mWorld, Matrix4 mCamera) {
            if(bs == null) bs = BlendState.PremultipliedAlphaBlend;
            if(ds == null) ds = DepthState.None;
            if(rs == null) rs = RasterizerState.CullNone;
            if(ss == null) ss = SamplerState.LinearWrap;

            // Setup The Shader
            program.Use();
            bs.Set();
            ds.Set();
            rs.Set();

            GL.UniformMatrix4(unWorld, true, ref mWorld);
            GL.UniformMatrix4(unVP, true, ref mCamera);

            bufVerts.UseAsAttrib(si);

            // Draw All The Batches
            int bc = batches.Count;
            for(int i = 0; i < bc; i++) {
                var batch = batches[i];
                batch.Texture.Use(TextureUnit.Texture0, unTexture);
                ss.Set(TextureTarget.Texture2D);
                GL.DrawArrays(PrimitiveType.Quads, batch.IndexOffset, batch.Indices);
                batch.Texture.Unuse();
            }
            program.Unuse();
        }
    }
}