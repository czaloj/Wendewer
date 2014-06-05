using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK;
using System.IO;
using System.Drawing;

namespace System.IO {
    public static class OTKSerializer {
        public static void Write(this BinaryWriter s, Vector2 v) {
            s.Write(v.X);
            s.Write(v.Y);
        }
        public static void Write(this BinaryWriter s, Point p) {
            s.Write(p.X);
            s.Write(p.Y);
        }
        public static void Write(this BinaryWriter s, Vector3 v) {
            s.Write(v.X);
            s.Write(v.Y);
            s.Write(v.Z);
        }
        public static void Write(this BinaryWriter s, Vector4 v) {
            s.Write(v.X);
            s.Write(v.Y);
            s.Write(v.Z);
            s.Write(v.W);
        }
        public static void Write(this BinaryWriter s, Rectangle r) {
            s.Write(r.X);
            s.Write(r.Y);
            s.Write(r.Width);
            s.Write(r.Height);
        }

        public static Vector2 ReadVector2(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            return new Vector2(x, y);
        }
        public static Point ReadPoint(this BinaryReader s) {
            int x = s.ReadInt32();
            int y = s.ReadInt32();
            return new Point(x, y);
        }
        public static Vector3 ReadVector3(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            float z = s.ReadSingle();
            return new Vector3(x, y, z);
        }
        public static Vector4 ReadVector4(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            float z = s.ReadSingle();
            float w = s.ReadSingle();
            return new Vector4(x, y, z, w);
        }
        public static Rectangle ReadRectangle(this BinaryReader s) {
            int x = s.ReadInt32();
            int y = s.ReadInt32();
            int z = s.ReadInt32();
            int w = s.ReadInt32();
            return new Rectangle(x, y, z, w);
        }
    }
}