using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using Wdw.Common.Data;
using EGL;
using OpenTK;
using EGL.Helpers;

namespace Wdw.GLView {
    public class MeshView : IDisposable {
        private GLBuffer vBuffer;
        private GLBuffer iBufferTris, iBufferEdges;
        public bool HasResources {
            get { return vBuffer != null || iBufferTris != null || iBufferEdges != null; }
        }

        public MeshView() {
        }
        public void Dispose() {
            if(vBuffer != null) {
                vBuffer.Dispose();
                vBuffer = null;
            }
            if(iBufferTris != null) {
                iBufferTris.Dispose();
                iBufferTris = null;
            }
            if(iBufferEdges != null) {
                iBufferEdges.Dispose();
                iBufferEdges = null;
            }
        }

        public void Build(MeshVertex[] verts, int[] tris, int[] edges) {
            Dispose();

            vBuffer = new GLBuffer(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw, true);
            vBuffer.SetAsVertex(MeshVertex.Size);
            vBuffer.SmartSetData(verts);
            iBufferTris = new GLBuffer(BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw, true);
            iBufferTris.SetAsIndexInt();
            iBufferTris.SmartSetData(tris);
            iBufferEdges = new GLBuffer(BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw, true);
            iBufferEdges.SetAsIndexInt();
            iBufferEdges.SmartSetData(edges);
        }

        public void DrawTris(ShaderInterface si) {
            if(vBuffer == null) return;
            vBuffer.UseAsAttrib(si);
            iBufferTris.Bind();
            GL.DrawElements(BeginMode.Triangles, iBufferTris.CurElements, DrawElementsType.UnsignedInt, 0);
            iBufferTris.Unbind();
        }
        public void DrawEdges(ShaderInterface si) {
            if(vBuffer == null) return;
            vBuffer.UseAsAttrib(si);
            iBufferEdges.Bind();
            GL.DrawElements(BeginMode.Lines, iBufferEdges.CurElements, DrawElementsType.UnsignedInt, 0);
            iBufferEdges.Unbind();
        }
    }
}