using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Wdw.Common.Data;
using Wdw.Common.Events;
using Wdw.Common.Property;
using Wdw.RT;
using EGL;
using EGL.Helpers;
using System.Drawing;

namespace Wdw.GLView {
    public class Renderer : IDisposable {
        private Dictionary<MeshData, MeshView> dMesh;
        private Dictionary<ObjectData, ObjectView> dObj;
        private Dictionary<MaterialData, MaterialView> dMat;
        private Dictionary<TextureData, TextureView> dTex;
        
        private PickingMaterial picker;
        public GLRenderTarget PickTexture {
            get;
            private set;
        }

        public Renderer() {
            // Make Views For All The Data
            dMesh = new Dictionary<MeshData, MeshView>();
            dObj = new Dictionary<ObjectData, ObjectView>();
            dMat = new Dictionary<MaterialData, MaterialView>();
            dTex = new Dictionary<TextureData, TextureView>();
            GLState.EnableAll();

            // The Renderer Will Render Everything To A Separate Render Target For Use With Multiple Views

            // This View Will Handle The Picking
            picker = new PickingMaterial();
            picker.Build();
            PickTexture = new GLRenderTarget(true);
            PickTexture.InternalFormat = PixelInternalFormat.Rgba;
            PickTexture.SetImage(new int[] { 800, 600, 0 }, PixelFormat.Rgba, PixelType.Float, false);
            PickTexture.BuildRenderTarget();


        }
        public void Dispose() {
            foreach(var v in dMesh.Values) {
                v.Dispose();
            }
            foreach(var v in dMat.Values) {
                v.Dispose();
            }
            foreach(var v in dTex.Values) {
                v.Dispose();
            }
        }

        public void ApplyEvent(DataEvent e) {
            switch(e.SourceType) {
                case DataSource.Object:
                    ApplyEvent(e.Source as ObjectData, e.MetaData);
                    break;
                case DataSource.Mesh:
                    ApplyEvent(e.Source as MeshData, e.MetaData);
                    break;
                case DataSource.Material:
                    ApplyEvent(e.Source as MaterialData, e.MetaData);
                    break;
                case DataSource.Texture:
                    ApplyEvent(e.Source as TextureData, e.MetaData);
                    break;
            }
        }
        public void ApplyEvent(ObjectData o, int e) {
            switch(e) {
                case ObjectData.EVENT_CREATION:
                    dObj.Add(o, new ObjectView(o));
                    break;
                case ObjectData.EVENT_DESTRUCTION:
                    dObj.Remove(o);
                    break;
                case ObjectData.EVENT_MATERIAL:
                    dObj[o].MaterialView = dMat[o.Material];
                    break;
                case ObjectData.EVENT_MESH:
                    dObj[o].MeshView = dMesh[o.Mesh];
                    break;
                case ObjectData.EVENT_TRANSFORM:
                    dObj[o].Transform = o.WorldTransform;
                    break;
                case ObjectData.EVENT_PARENT:
                    dObj[o].Transform = o.WorldTransform;
                    // TODO: Update Widgets
                    break;
            }
        }
        public void ApplyEvent(MeshData m, int e) {
            switch(e) {
                case MeshData.EVENT_CREATION:
                    dMesh.Add(m, new MeshView());
                    break;
                case MeshData.EVENT_DESTRUCTION:
                    dMesh[m].Dispose();
                    dMesh.Remove(m);
                    break;
                case MeshData.EVENT_MODIFY:
                    MeshView v = dMesh[m];
                    if(m.vertices == null || m.vertices.Length < 1) {
                        v.Dispose();
                    }
                    else {
                        v.Build(m.vertices, m.triangles, m.edges);
                    }
                    break;
            }
        }
        public void ApplyEvent(MaterialData m, int e) {
            MaterialView v;
            switch(e) {
                case MaterialData.EVENT_CREATION:
                    dMat.Add(m, new MaterialView());
                    break;
                case MeshData.EVENT_DESTRUCTION:
                    dMat[m].Dispose();
                    dMat.Remove(m);
                    break;
                case MaterialData.EVENT_SHADER_LOCATION:
                    if(m.VShaderFile != null && m.FShaderFile != null)
                        dMat[m].Build(m.VShaderFile, m.FShaderFile);
                    break;
                case MaterialData.EVENT_PROPERTY_CHANGE:
                    v = dMat[m];
                    v.BeginUniformChange();
                    foreach(var p in m.Uniforms) {
                        int fxp;
                        if(!v.Uniforms.TryGetValue(p.Name, out fxp)) continue;
                        switch(p.Type) {
                            case PropertyType.Int:
                                GL.Uniform1(fxp, ((IntProperty)p).Data);
                                break;
                            case PropertyType.Float:
                                GL.Uniform1(fxp, ((FloatProperty)p).Data);
                                break;
                            case PropertyType.Vec2:
                                GL.Uniform2(fxp, ((Vec2Property)p).Data);
                                break;
                            case PropertyType.Vec3:
                                GL.Uniform3(fxp, ((Vec3Property)p).Data);
                                break;
                            case PropertyType.Vec4:
                                GL.Uniform4(fxp, ((Vec4Property)p).Data);
                                break;
                        }
                    }
                    v.EndUniformChange();
                    break;
                case MaterialData.EVENT_TEXTURE:
                    v = dMat[m];
                    v.ClearTextures();
                    foreach(var tb in m.Textures) {
                        v.AddTexture(tb.Index, tb.Uniform, dTex[tb.Texture]);
                    }
                    break;
            }
        }
        public void ApplyEvent(TextureData t, int e) {
            switch(e) {
                case TextureData.EVENT_CREATION:
                    dTex.Add(t, new TextureView());
                    break;
                case MeshData.EVENT_DESTRUCTION:
                    dTex[t].Dispose();
                    dTex.Remove(t);
                    break;
                case TextureData.EVENT_FILE:
                    dTex[t].Build(t.FileLocation);
                    break;
            }
        }

        public uint Pick(Matrix4 mView, Matrix4 mProj, float px, float py, out Vector3 wPos, bool edges = false) {
            // Picking Will Ignore Transparency
            BlendState.Opaque.Set();
            RasterizerState.CullCounterClockwise.Set();
            DepthState.Default.Set();

            PickTexture.UseTarget();

            GL.ClearColor(1, 1, 1, 1);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            picker.SetCamera(mView, mProj);
            picker.Bind();
            foreach(var o in dObj.Values)
                o.DrawPicking(picker, picker.AttrUUIDBindLocation, edges);
            picker.Unbind();
            uint uuid = GLPicker.GetUUID((int)(px * PickTexture.Width), (int)(py * PickTexture.Height));
            Matrix4 mVI = mView.Inverted();
            Matrix4 mPI = mProj.Inverted();
            wPos = GLPicker.UnprojectLocation((int)(px * PickTexture.Width), (int)(py * PickTexture.Height), new Vector2(PickTexture.Width, PickTexture.Height), ref mVI, ref mPI);

            PickTexture.UnuseTarget();

            return uuid;
        }
        public void Draw(Matrix4 mView, Matrix4 mProj, bool edges = false) {
            BlendState.Opaque.Set();
            RasterizerState.CullCounterClockwise.Set();
            DepthState.Default.Set();

            foreach(var m in dMat.Values)
                m.SetCamera(mView, mProj);
            foreach(var o in dObj.Values)
                o.Draw(edges);
        }

        public void RayTraceScene(string outFile, Matrix4 mView, Matrix4 mProj) {
            Scene scene = new Scene();
            RTGLCamera rtglCam = new RTGLCamera();
            rtglCam.setMatrices(mView, mProj);
            scene.camera = rtglCam;
            scene.backColor = Vector3.Zero;
            PointLight pl = new PointLight();
            pl.position = Matrix4.Invert(mView).ExtractTranslation();
            pl.intensity = Vector3.One;
            scene.addLight(pl);
            foreach(var o in dObj.Keys) {
                if(o.Mesh == MasterData.Instance.Box) {
                    Box s = new Box();
                    s.setTransformation(o.WorldTransform);
                    var sh = new Lambertian();
                    sh.setDiffuseColor(new Vector3(1, 0.1f, 0.2f));
                    s.setShader(sh);
                    scene.addSurface(s);
                }
            }
            scene.setImage(new RTImage(100, 100));
            scene.setSamples(2);

            RayTracer rt = new RayTracer();
            rt.renderImage(scene);
            scene.getImage().write(outFile);
        }
    }
}