using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public struct VertexPositionNormalTexture {
        public static readonly int Size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexPositionNormalTexture));
        public static readonly ArrayBind[] Bindings = new ArrayBind[] {
            new ArrayBind(Semantic.Position, VertexAttribPointerType.Float, 3, sizeof(float) * 0),
            new ArrayBind(Semantic.Normal, VertexAttribPointerType.Float, 3, sizeof(float) * 3),
            new ArrayBind(Semantic.TexCoord, VertexAttribPointerType.Float, 2, sizeof(float) * 6)
        };

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        public VertexPositionNormalTexture(Vector3 p, Vector3 n, Vector2 t) {
            Position = p;
            Normal = n;
            TextureCoordinate = t;
        }
    }
}
