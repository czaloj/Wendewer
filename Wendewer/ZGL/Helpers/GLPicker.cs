using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace EGL.Helpers {
    /// <summary>
    /// This Class Manages The Vertex ID Colors
    /// For Hardware-Accelerated Pixel Picking.
    /// 
    /// Hopefully, No more Than 4 Billion Objects
    /// Are Ever Present For Picking.
    /// </summary>
    public static class GLPicker {
        private static uint uuid = 0xffffffff;
        private static readonly Queue<uint> recycled = new Queue<uint>();

        public static uint ObtainUUID() {
            // Use Recycled UUID's First
            if(recycled.Count > 0) return recycled.Dequeue();

            // Increment And Return The New UUID
            unchecked { uuid++; }
            return uuid;
        }
        public static void RecycleUUID(uint uuid) {
            recycled.Enqueue(uuid);
        }

        public static uint GetUUID(int px, int py) {
            byte[] buf = new byte[sizeof(float) * 4];
            GL.ReadPixels(px, py, 1, 1, PixelFormat.Rgba, PixelType.Float, buf);
            return Convert(new Vector4(
                BitConverter.ToSingle(buf, sizeof(float) * 0),
                BitConverter.ToSingle(buf, sizeof(float) * 1),
                BitConverter.ToSingle(buf, sizeof(float) * 2),
                BitConverter.ToSingle(buf, sizeof(float) * 3)
                ));
        }
        public static Vector3 UnprojectLocation(int px, int py, Vector2 viewSize, ref Matrix4 mViewInv, ref Matrix4 mProjInv) {
            Vector4 sPos = new Vector4(
                px * 2 / viewSize.X - 1,
                py * 2 / viewSize.Y - 1,
                0,
                1
                );

            GL.ReadPixels(px, py, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref sPos.Z);
            Vector4 vPos = Vector4.Transform(sPos, mProjInv);
            vPos /= vPos.W;
            Vector4 wPos = Vector4.Transform(vPos, mViewInv);
            return new Vector3(wPos.X, wPos.Y, wPos.Z);
        }

        public static Vector4 Convert(uint id) {
            return new Vector4(
                (id & 0x000000ff) / 255f,
                ((id >> 8) & 0x000000ff) / 255f,
                ((id >> 16) & 0x000000ff) / 255f,
                ((id >> 24) & 0x000000ff) / 255f
                );
        }
        public static uint Convert(Vector4 v) {
            uint l0 = (uint)(v.X * 255f + 0.5f);
            uint l1 = (uint)(v.Y * 255f + 0.5f);
            uint l2 = (uint)(v.Z * 255f + 0.5f);
            uint l3 = (uint)(v.W * 255f + 0.5f);
            return l0 | (l1 << 8) | (l2 << 16) | (l3 << 24);
        }
    }
}