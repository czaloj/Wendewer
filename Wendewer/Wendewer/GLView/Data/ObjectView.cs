using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGL.Helpers;
using OpenTK;
using OpenTK.Graphics;
using Wdw.Common.Data;

namespace Wdw.GLView {
    public class ObjectView {
        public ObjectData ObjectData {
            get;
            private set;
        }

        private MeshView mesh;
        public MeshView MeshView {
            get { return mesh; }
            set {
                mesh = value;
            }
        }
        public MaterialView MaterialView;
        public Matrix4 Transform;
        private Vector4 vUUID;

        public ObjectView(ObjectData od) {
            ObjectData = od;
            vUUID = GLPicker.Convert(ObjectData.UUID);

            Transform = Matrix4.Identity;
            MeshView = null;
            MaterialView = null;
        }

        public void Draw(bool edges = false) {
            if(MeshView == null || MaterialView == null)
                return;

            MaterialView.Bind(Transform);
            if(edges) MeshView.DrawEdges(MaterialView.AttributeBinds);
            else MeshView.DrawTris(MaterialView.AttributeBinds);
            MaterialView.Unbind();
        }
        public void DrawPicking(PickingMaterial picker, int pickBufInd, bool edges = false) {
            if(MeshView == null) return;

            picker.SetWorldTransform(ref Transform);
            picker.UUID = vUUID;
            if(edges) MeshView.DrawEdges(picker.AttributeBinds);
            else MeshView.DrawTris(picker.AttributeBinds);
        }
    }
}