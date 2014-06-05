using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Wdw.Common.Data;
using EGL;

namespace Wdw.GLView {
    public class MaterialView : IDisposable {
        public const string PARAM_KEY_WORLD = "World";
        public const string PARAM_KEY_VIEW = "View";
        public const string PARAM_KEY_PROJ = "Projection";

        public GLProgram Program {
            get;
            private set;
        }
        private int fxpWorld, fxpView, fxpProj;
        public Dictionary<string, int> Uniforms {
            get { return Program.Uniforms; }
        }
        public ShaderInterface AttributeBinds {
            get;
            private set;
        }

        private List<int> texBinds;
        private List<string> texUniforms;
        private List<TextureView> tex;

        public MaterialView() {
            Program = null;
            fxpWorld = fxpView = fxpProj = -1;
            texBinds = new List<int>();
            texUniforms = new List<string>();
            tex = new List<TextureView>();
        }
        public void Dispose() {
            if(Program != null) {
                Program.Dispose();
                Program = null;
            }
            ClearTextures();
        }

        public void Build(string vsFile, string fsFile) {
            Dispose();

            Program = new GLProgram().QuickCreate(vsFile, fsFile);
            if(!Program.Uniforms.TryGetValue(PARAM_KEY_WORLD, out fxpWorld))
                fxpWorld = -1;
            if(!Program.Uniforms.TryGetValue(PARAM_KEY_VIEW, out fxpView))
                fxpView = -1;
            if(!Program.Uniforms.TryGetValue(PARAM_KEY_PROJ, out fxpProj))
                fxpProj = -1;
            AttributeBinds = new ShaderInterface(MeshVertex.Binds);
            AttributeBinds.Build(Program.SemanticLinks);
        }

        public void AddTexture(int i, string unSampler, TextureView v) {
            texBinds.Add(i);
            texUniforms.Add(unSampler);
            tex.Add(v);
        }
        public void ClearTextures() {
            texBinds = new List<int>();
            tex = new List<TextureView>();
        }

        public void SetCamera(Matrix4 mView, Matrix4 mProj) {
            Program.Use();
            if(fxpView >= 0) GL.UniformMatrix4(fxpView, true, ref mView);
            if(fxpProj >= 0) GL.UniformMatrix4(fxpProj, true, ref mProj);
            Program.Unuse();
        }

        public void SetWorldTransform(ref Matrix4 mWorld) {
            if(fxpWorld >= 0) GL.UniformMatrix4(fxpWorld, true, ref mWorld);
        }
        public void Bind(Matrix4 mWorld) {
            Program.Use();
            SetWorldTransform(ref mWorld);
            for(int i = 0; i < texBinds.Count; i++) {
                tex[i].Bind(texBinds[i] + TextureUnit.Texture0, Program.Uniforms[texUniforms[i]]);
            }
        }
        public void Unbind() {
            Program.Unuse();
            for(int i = 0; i < texBinds.Count; i++) {
                tex[i].Unbind();
            }
        }

        public void BeginUniformChange() {
            Program.Use();
        }
        public void EndUniformChange() {
            Program.Unuse();
        }
    }
}