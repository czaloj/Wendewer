using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace EGL {

    // Parsing Options
    public enum ParsingFlags : byte {
        None = 0x00,
        FlipTexCoordV = 0x01,
        FlipTriangleOrder = 0x02,
        ConversionOpenGL = FlipTexCoordV | FlipTriangleOrder,

        LoadEffectByteCode = 0x04,
        LoadTextureStream = 0x08,

        WriteUV = 0x10,
        WriteNorms = 0x20,
        WriteColor = 0x40,
        WriteAll = WriteUV | WriteNorms | WriteColor
    }

    // This Is Drawn With Triangles Arranged In Clockwise Order
    public struct ObjTriangle {
        public VertexPositionNormalTexture V1, V2, V3;
        public Vector3 Normal;

        public ObjTriangle(VertexPositionNormalTexture v1, VertexPositionNormalTexture v2, VertexPositionNormalTexture v3) {
            // Set Vertices
            V1 = v1; V2 = v2; V3 = v3;

            // Calculate Face Normal
            Normal = Vector3.Cross(
                V3.Position - V1.Position,
                V2.Position - V1.Position
                );
            Normal.Normalize();
        }
    }

    public static class ObjParser {
        // For Reading
        #region Intermediate Structs
        struct VInds {
            public int PosInd, UVInd, NormInd;

            public bool Viable {
                get { return PosInd >= 0; }
            }

            public VInds(string s) {
                // Trim And Split
                String[] inds = s.Trim().Split(new string[] { "/", @"\" }, StringSplitOptions.None);
                if(inds.Length < 1) { PosInd = -1; UVInd = -1; NormInd = -1; return; }

                // Get Position
                if(!int.TryParse(inds[0], out PosInd)) { PosInd = -1; UVInd = -1; NormInd = -1; return; }

                // Get UV
                if(inds.Length < 2) {
                    UVInd = 0; NormInd = 0;
                }
                else {
                    if(string.IsNullOrEmpty(inds[1])) UVInd = 0;
                    else if(!int.TryParse(inds[1], out UVInd)) { PosInd = -1; UVInd = -1; NormInd = -1; return; }
                }
                // Get Normal
                if(inds.Length < 3) {
                    NormInd = 0;
                }
                else {
                    if(string.IsNullOrEmpty(inds[2])) NormInd = 0;
                    else if(!int.TryParse(inds[2], out NormInd)) { PosInd = -1; UVInd = -1; NormInd = -1; return; }
                }
                // Make Indices Zero-based
                PosInd--;
                UVInd--;
                NormInd--;
            }
        }
        struct Tri {
            public VInds V1, V2, V3;

            public Tri(string v1, string v2, string v3) {
                V1 = new VInds(v1);
                V2 = new VInds(v2);
                V3 = new VInds(v3);
            }
        }
        class VertDict : IEnumerable<VertDict.Key> {
            public struct Key {
                public int Index;
                public VInds Vertex;

                public Key(int i, VInds v) {
                    Index = i;
                    Vertex = v;
                }
            }

            List<Key>[] verts;
            public int Count { get; private set; }

            public VertDict() {
                Count = 0;
                verts = new List<Key>[256];
                for(int i = 0; i < verts.Length; i++) {
                    verts[i] = new List<Key>(8);
                }
            }
            ~VertDict() {
                foreach(var l in verts) {
                    l.Clear();
                }
                verts = null;
                Count = 0;
            }

            public int Get(VInds v) {
                int h = v.GetHashCode() & 0xff;
                for(int i = 0; i < verts[h].Count; i++) {
                    if(verts[h][i].Vertex.Equals(v)) return verts[h][i].Index;
                }
                verts[h].Add(new Key(Count, v));
                Count++;
                return Count - 1;
            }

            public IEnumerator<Key> GetEnumerator() {
                for(int h = 0; h < verts.Length; h++) {
                    for(int i = 0; i < verts[h].Count; i++) {
                        yield return verts[h][i];
                    }
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
        #endregion

        public static bool TryParse(Stream s, out VertexPositionNormalTexture[] verts, out int[] inds, ParsingFlags ps = ParsingFlags.None) {
            // Default Values
            verts = null; inds = null;

            // Encapsulate Stream To A Buffered Stream Reader
            BufferedStream bs = new BufferedStream(s);
            StreamReader f = new StreamReader(bs);

            // List Of Components
            List<Vector3> pos = new List<Vector3>(100);
            List<Vector2> uv = new List<Vector2>(100);
            List<Vector3> norms = new List<Vector3>(100);
            List<Tri> tris = new List<Tri>(200);

            // Buffer Vectors
            Vector3 v3 = Vector3.Zero; Vector2 v2 = Vector2.Zero;

            // Get All The Information From The Stream
            string line; string[] spl;
            while(!f.EndOfStream) {
                line = f.ReadLine();
                spl = Regex.Split(line, @"\s+", RegexOptions.IgnorePatternWhitespace);
                switch(spl[0].ToLower()) {
                    case "v": // Vertex Position
                        if(spl.Length != 4) return false;
                        if(!float.TryParse(spl[1], out v3.X)) return false;
                        if(!float.TryParse(spl[2], out v3.Y)) return false;
                        if(!float.TryParse(spl[3], out v3.Z)) return false;
                        pos.Add(v3);
                        break;
                    case "vt": // Vertex Texture Coordinate
                        if(spl.Length != 3) return false;
                        if(!float.TryParse(spl[1], out v2.X)) return false;
                        if(!float.TryParse(spl[2], out v2.Y)) return false;
                        // Possibly Flip Tex Coords
                        if(ps.HasFlag(ParsingFlags.FlipTexCoordV)) v2.Y = 1 - v2.Y;
                        uv.Add(v2);
                        break;
                    case "vn": // Vertex Normal
                        if(spl.Length != 4) return false;
                        if(!float.TryParse(spl[1], out v3.X)) return false;
                        if(!float.TryParse(spl[2], out v3.Y)) return false;
                        if(!float.TryParse(spl[3], out v3.Z)) return false;
                        norms.Add(v3);
                        break;
                    case "f": // Mesh Triangle
                        if(spl.Length != 4) return false;
                        try {
                            // Add In Correct Triangle Ordering
                            if(ps.HasFlag(ParsingFlags.FlipTriangleOrder))
                                tris.Add(new Tri(spl[1], spl[3], spl[2]));
                            else
                                tris.Add(new Tri(spl[1], spl[2], spl[3]));
                        }
                        catch(Exception) {
                            return false;
                        }
                        break;
                }
            }




            // Create Indices
            VertDict vd = new VertDict();
            inds = new int[tris.Count * 3];
            int ii = 0;
            foreach(Tri tri in tris) {
                inds[ii++] = vd.Get(tri.V1);
                inds[ii++] = vd.Get(tri.V2);
                inds[ii++] = vd.Get(tri.V3);
            }

            // Create Vertices
            verts = new VertexPositionNormalTexture[vd.Count];
            foreach(VertDict.Key v in vd) {
                verts[v.Index].Position = pos[v.Vertex.PosInd];

                if(v.Vertex.UVInd < 0) verts[v.Index].TextureCoordinate = Vector2.Zero;
                else verts[v.Index].TextureCoordinate = uv[v.Vertex.UVInd];

                if(v.Vertex.NormInd < 0) verts[v.Index].Normal = Vector3.Zero;
                else verts[v.Index].Normal = norms[v.Vertex.NormInd];
            }

            return true;
        }
        public static bool TryParse(Stream s, out ObjTriangle[] tris, ParsingFlags ps = ParsingFlags.None) {
            VertexPositionNormalTexture[] verts;
            int[] inds;
            if(TryParse(s, out verts, out inds, ps)) {
                tris = new ObjTriangle[inds.Length / 3];
                for(int ti = 0, i = 0; ti < tris.Length; ti++) {
                    tris[ti] = new ObjTriangle(
                        verts[inds[i]],
                        verts[inds[i + 1]],
                        verts[inds[i + 2]]
                        );
                    i += 3;
                }
                return true;
            }
            tris = null;
            return false;
        }
        public static bool TryParse(Stream s, out GLBuffer vb, out GLBuffer ib, ParsingFlags ps = ParsingFlags.None) {
            VertexPositionNormalTexture[] verts;
            int[] inds;
            if(!TryParse(s, out verts, out inds, ps)) {
                vb = null;
                ib = null;
                return false;
            }
            vb = new GLBuffer(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
            vb.SetAsVertex(VertexPositionNormalTexture.Size);
            vb.SmartSetData(verts);
            ib = new GLBuffer(BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw);
            ib.SetAsIndexInt();
            ib.SmartSetData(inds);
            return true;
        }
    }
}