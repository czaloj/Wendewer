using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using EGL;
using Wdw.Common.Events;
using OpenTK.Graphics.OpenGL4;
using Wdw.Common.Property;

namespace Wdw.Common.Data {
    public struct MeshVertex {
        public static readonly int Size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MeshVertex));
        public static readonly ArrayBind[] Binds = new ArrayBind[] {
            new ArrayBind(Semantic.Position, VertexAttribPointerType.Float, 3, sizeof(float) * 0),
            new ArrayBind(Semantic.Normal, VertexAttribPointerType.Float, 3, sizeof(float) * 3),
            new ArrayBind(Semantic.Tangent, VertexAttribPointerType.Float, 3, sizeof(float) * 6),
            new ArrayBind(Semantic.Binormal, VertexAttribPointerType.Float, 3, sizeof(float) * 9),
            new ArrayBind(Semantic.TexCoord, VertexAttribPointerType.Float, 2, sizeof(float) * 12),
            new ArrayBind(Semantic.Color, VertexAttribPointerType.Float, 3, sizeof(float) * 14)
        };

        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;
        public Vector2 TexCoords;
        public Vector3 Color;

        public MeshVertex(VertexPositionNormalTexture v) {
            Position = v.Position;
            Normal = v.Normal;
            TexCoords = v.TextureCoordinate;

            Tangent = Vector3.UnitX;
            Bitangent = Vector3.UnitY;

            Color = Vector3.One;
        }
    }

    public class MeshData : IDisposable {
        private static readonly UUIDGen IDGen = new UUIDGen();
        public const int EVENT_CREATION = 0;
        public const int EVENT_DESTRUCTION = EVENT_CREATION + 1;
        public const int EVENT_MODIFY = EVENT_DESTRUCTION + 1;

        // ID For This Mesh For Comparisons
        public readonly uint id;

        public PropertyList Properties {
            get;
            private set;
        }
        public string Name {
            get { return Properties.Get<string>("Name").Data; }
            set { Properties.Get<string>("Name").SetData(value); }
        }

        // Vertex Property Data
        public MeshVertex[] vertices;

        // Index Data
        public int[] triangles;
        public int[] edges;

        // AABB Coordinates
        private Vector3 aabbMin, aabbMax;

        public MeshData() {
            id = IDGen.Obtain();

            vertices = new MeshVertex[0];
            triangles = new int[0];
            edges = new int[0];
            aabbMin = -Vector3.One;
            aabbMax = Vector3.One;

            Properties = new PropertyList();
            Properties.Add(new StringProperty("Name"));
            Name = "Mesh." + id;

            MasterData.SendEvent(new DataEvent(this, EVENT_CREATION));
        }
        public void Dispose() {
            IDGen.Recycle(id);
            MasterData.SendEvent(new DataEvent(this, EVENT_DESTRUCTION));
        }

        public void ReadData(string file) {
            VertexPositionNormalTexture[] verts;
            int[] inds;
            using(var s = File.OpenRead(file)) {
                ObjParser.TryParse(s, out verts, out inds, ParsingFlags.ConversionOpenGL);
            }
            MeshVertex[] v = new MeshVertex[verts.Length];
            for(int i = 0; i < v.Length; i++) {
                v[i] = new MeshVertex(verts[i]);
            }

            SetData(v, inds);
        }
        public void SetData(MeshVertex[] verts, int[] tris) {
            int[] edges = new int[tris.Length * 2];
            for(int ti = 0, ei = 0; ti < tris.Length; ) {
                edges[ei + 0] = tris[ti + 0];
                edges[ei + 1] = tris[ti + 1];
                edges[ei + 2] = tris[ti + 1];
                edges[ei + 3] = tris[ti + 2];
                edges[ei + 4] = tris[ti + 2];
                edges[ei + 5] = tris[ti + 0];
                ei += 6;
                ti += 3;
            }
            SetData(verts, tris, edges);
        }
        public void SetData(MeshVertex[] verts, int[] tris, int[] e) {
            // Copy Over The Vertices
            vertices = new MeshVertex[verts.Length];
            verts.CopyTo(vertices, 0);

            // Copy Triangle Indices
            triangles = new int[tris.Length];
            tris.CopyTo(triangles, 0);

            // Copy Over Edges
            edges = new int[e.Length];
            e.CopyTo(edges, 0);
            
            UpdateAABB();

            MasterData.SendEvent(new DataEvent(this, EVENT_MODIFY));
        }

        public void UpdateAABB() {
            aabbMin = new Vector3(float.MaxValue);
            aabbMax = new Vector3(-float.MaxValue);
            for(int i = 0; i < vertices.Length; i++) {
                aabbMin = Vector3.Min(vertices[i].Position, aabbMin);
                aabbMax = Vector3.Max(vertices[i].Position, aabbMax);
            }
        }
    }
}