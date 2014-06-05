using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGL;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Wdw.Common.Data;

namespace Wdw.GLView {
    public class PickingMaterial : MaterialView {
        private const string VS_FILE = @"data\shaders\pick.vert";
        private const string FS_FILE = @"data\shaders\pick.frag";
        public int AttrUUIDBindLocation {
            get;
            private set;
        }
        private int unUUID;
        public Vector4 UUID {
            set { GL.Uniform4(unUUID, ref value); }
        }

        public PickingMaterial()
            : base() {
        }

        public void Build() {
            Build(VS_FILE, FS_FILE);
            int aubl;
            if(Program.SemanticLinks.TryGetValue(Semantic.Color | Semantic.Index1, out aubl))
                AttrUUIDBindLocation = aubl;
            else
                AttrUUIDBindLocation = -1;
            if(!Program.Uniforms.TryGetValue("UUID", out unUUID))
                unUUID = -1;
        }

        public void Bind() {
            Program.Use();
        }
        new public void Unbind() {
            Program.Unuse();
        }
    }
}
