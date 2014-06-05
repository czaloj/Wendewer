using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Wdw.Common.Events;
using EGL;
using Wdw.Common.Property;

namespace Wdw.Common.Data {
    public class MasterData {
        public static MasterData Instance {
            get;
            private set;
        }
        static MasterData() {
            Instance = new MasterData();
        }
        public static void SendEvent(DataEvent e) {
            Instance.EventQueue.Enqueue(e);
        }
        public static DataEvent GetEvent() {
            return Instance.EventQueue.Count > 0 ? Instance.EventQueue.Dequeue() : null;
        }

        // Scene Comprising Of The Objects
        public SceneData Scene {
            get;
            private set;
        }

        // Lists Of Data
        public List<ObjectData> Objects {
            get;
            private set;
        }
        public List<ObjectData> ObjectsSelected {
            get;
            private set;
        }
        public ObjectData ObjectActive {
            get;
            set;
        }
        public Dictionary<uint, ObjectData> ObjectsByUUID {
            get;
            private set;
        }
        public Dictionary<string, ObjectData> ObjectsByName {
            get;
            private set;
        }

        public List<MeshData> Meshes {
            get;
            private set;
        }
        public List<MeshData> MeshesSelected {
            get;
            private set;
        }
        public MeshData MeshActive {
            get;
            set;
        }
        public Dictionary<string, MeshData> MeshesByName {
            get;
            private set;
        }

        public List<MaterialData> Materials {
            get;
            private set;
        }
        public List<MaterialData> MaterialsSelected {
            get;
            private set;
        }
        public MaterialData MaterialActive {
            get;
            set;
        }
        public Dictionary<string, MaterialData> MaterialsByName {
            get;
            private set;
        }

        public List<TextureData> Textures {
            get;
            private set;
        }
        public List<TextureData> TexturesSelected {
            get;
            private set;
        }
        public TextureData TextureActive {
            get;
            set;
        }
        public Dictionary<string, TextureData> TexturesByName {
            get;
            private set;
        }

        // Pre-Made Values
        public MeshData Sphere {
            get;
            private set;
        }
        public MeshData Box {
            get;
            private set;
        }
        public MeshData Cylinder {
            get;
            private set;
        }
        public IEnumerable<MeshData> DefaultMeshes {
            get {
                yield return Sphere;
                yield return Box;
                yield return Cylinder;
            }
        }
        public MaterialData SimpleShader {
            get;
            private set;
        }
        public IEnumerable<MaterialData> DefaultMaterials {
            get {
                yield return SimpleShader;
            }
        }
        public TextureData SimpleTexture {
            get;
            private set;
        }
        public IEnumerable<TextureData> DefaultTextures {
            get {
                yield return SimpleTexture;
            }
        }

        // The Event Queue For Views
        public Queue<DataEvent> EventQueue {
            get;
            private set;
        }

        public MasterData() {
            Objects = new List<ObjectData>();
            ObjectsSelected = new List<ObjectData>();
            ObjectActive = null;
            ObjectsByUUID = new Dictionary<uint, ObjectData>();
            ObjectsByName = new Dictionary<string, ObjectData>();
            
            Meshes = new List<MeshData>();
            MeshesSelected = new List<MeshData>();
            MeshActive = null;
            MeshesByName = new Dictionary<string, MeshData>();
            
            Materials = new List<MaterialData>();
            MaterialsSelected = new List<MaterialData>();
            MaterialActive = null;
            MaterialsByName = new Dictionary<string, MaterialData>();
            
            Textures = new List<TextureData>();
            TexturesSelected = new List<TextureData>();
            TextureActive = null;
            TexturesByName = new Dictionary<string, TextureData>();
            
            Scene = new SceneData();
            EventQueue = new Queue<DataEvent>();
            Instance = this;

            Box = CreateMesh();
                //BuildBox();
            Sphere = CreateMesh();
            //BuildSphere(0.5f);
            Cylinder = CreateMesh();
            //BuildCylinder(0.5f);

            SimpleTexture = CreateTexture();
            SimpleTexture.Name = "Simple";
            SimpleTexture.FileLocation = @"data\textures\Simple.png";
            SimpleShader = CreateMaterial();
            SimpleShader.VShaderFile = @"data\shaders\Simple.vert";
            SimpleShader.FShaderFile = @"data\shaders\Simple.frag";
        }

        public ObjectData CreateObject() {
            ObjectData d = new ObjectData();
            Objects.Add(d);
            ObjectsByUUID[d.UUID] = d;
            ObjectsByName[d.Name] = d;
            return d;
        }
        public MeshData CreateMesh() {
            MeshData d = new MeshData();
            Meshes.Add(d);
            MeshesByName[d.Name] = d;
            return d;
        }
        public MaterialData CreateMaterial() {
            MaterialData d = new MaterialData();
            Materials.Add(d);
            MaterialsByName[d.Name] = d;
            return d;
        }
        public TextureData CreateTexture() {
            TextureData d = new TextureData();
            Textures.Add(d);
            TexturesByName[d.Name] = d;
            return d;
        }

        public void Remove(ObjectData d) {
            d.Dispose();
            Objects.Remove(d);
            ObjectsByUUID.Remove(d.UUID);
            ObjectsByName.Remove(d.Name);
        }
        public void RemoveObject(uint uuid) {
            ObjectData d;
            if(!ObjectsByUUID.TryGetValue(uuid, out d))
                return;
            Remove(d);
        }
        public void RemoveObject(string name) {
            ObjectData d;
            if(!ObjectsByName.TryGetValue(name, out d))
                return;
            Remove(d);
        }
        public void Remove(MeshData d) {
            d.Dispose();
            Meshes.Remove(d);
            MeshesByName.Remove(d.Name);
        }
        public void RemoveMesh(string name) {
            MeshData d;
            if(!MeshesByName.TryGetValue(name, out d))
                return;
            Remove(d);
        }
        public void Remove(MaterialData d) {
            d.Dispose();
            Materials.Remove(d);
            MaterialsByName.Remove(d.Name);
        }
        public void RemoveMaterial(string name) {
            MaterialData d;
            if(!MaterialsByName.TryGetValue(name, out d))
                return;
            Remove(d);
        }
        public void Remove(TextureData d) {
            d.Dispose();
            Textures.Remove(d);
            TexturesByName.Remove(d.Name);
        }
        public void RemoveTexture(string name) {
            TextureData d;
            if(!TexturesByName.TryGetValue(name, out d))
                return;
            Remove(d);
        }

        public void RemoveAll() {
            foreach(var d in Objects.ToArray()) Remove(d);
            foreach(var d in Meshes.Except(DefaultMeshes).ToArray()) Remove(d);
            foreach(var d in Materials.Except(DefaultMaterials).ToArray()) Remove(d);
            foreach(var d in Textures.Except(DefaultTextures).ToArray()) Remove(d);
        }

        private int lineInd = 0;
        private void makeLine(int[] lineInds, int startInd, int endInd) {
            lineInds[lineInd] = startInd;
            lineInds[lineInd + 1] = endInd;
            lineInd += 2;
        }
        private int triInd = 0;
        private void makeTri(int[] triInds, int one, int two, int three) {
            triInds[triInd] = one;
            triInds[triInd + 1] = two;
            triInds[triInd + 2] = three;
            triInd += 3;
        }
        public MeshData BuildSphere(float tolerance) {
            var mesh = CreateMesh();

            int U = (int)Math.Ceiling((2.0 * Math.PI / tolerance));
            int verts = U * U + 2;
            int lines = 2 * U * U;
            int tris = 2 * U * U;

            MeshVertex[] vertices = new MeshVertex[verts];

            int[] triInds = new int[tris * 3];
            int[] lineInds = new int[lines * 2];

            lineInd = 0;
            triInd = 0;

            // top point
            vertices[0].Position = Vector3.UnitY;
            vertices[0].Normal = Vector3.UnitY;
            vertices[0].TexCoords = Vector2.Zero;
            int vi = 1;
            for(int u = 1; u < U; u++) {
                float theta = u * (float)Math.PI / U;
                float z = (float)Math.Cos(theta);
                vi = 3 * u * U;
                for(int v = 0; v < U; v++) {
                    float phi = v * (float)Math.PI * 2 / U;
                    float x = (float)Math.Cos(phi) * (float)Math.Sin(theta);
                    float y = (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    vertices[vi].Position = new Vector3(x, y, z);
                    vertices[vi].Normal = new Vector3(x, y, z);
                    vertices[vi].TexCoords = new Vector2((float)u / U, (float)v / U);

                    // lines of latitude
                    if(v > 0) {
                        makeLine(lineInds, vi, vi - 1);
                    }

                    // lines of longitude
                    if(u == 1) {
                        makeLine(lineInds, vi, 0);
                    }
                    else if(u > 1) {
                        makeLine(lineInds, vi, (u - 1) * U + v);
                    }

                    if(v > 0 && u == 1) {
                        makeTri(triInds, vi, vi - 1, 0);
                    }
                    else if(v > 0) {
                        makeTri(triInds, vi, vi - 1, (u - 1) * U + v);
                        makeTri(triInds, vi - 1, (u - 1) * U + v - 1, (u - 1) * U + v);
                    }

                    vi++;
                }
                // connect the lines of latitude
                makeLine(lineInds, u * U, vi - 1);
                // connect the triangles
                makeTri(triInds, u * U, vi - 1, (u - 1) * U);
                makeTri(triInds, vi - 1, u * U - 1, (u - 1) * U);
            }

            // connect the lines of longitude and make the endcap
            for(int v = 0; v < U; v++) {
                makeLine(lineInds, (U - 1) * U + v, verts - 1);
                if(v < U - 1)
                    makeTri(triInds, (U - 1) * U + v, (U - 1) * U + v + 1, verts - 1);
            }

            vertices[verts - 1].Position = -Vector3.UnitY;
            vertices[verts - 1].Normal = -Vector3.UnitY;
            vertices[verts - 1].TexCoords = Vector2.UnitY;

            mesh.SetData(vertices, triInds, lineInds);
            return mesh;
        }
        public MeshData BuildBox() {
            var mesh = CreateMesh();

            MeshVertex[] verts = new MeshVertex[24] {
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, 1, -1), -Vector3.UnitX, Vector2.Zero
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, 1, 1), -Vector3.UnitX, Vector2.UnitX
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, -1, -1), -Vector3.UnitX, Vector2.UnitY
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, -1, 1), -Vector3.UnitX, Vector2.One
                    )),

                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, 1, 1), Vector3.UnitX, Vector2.Zero
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, 1, -1), Vector3.UnitX, Vector2.UnitX
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, -1, 1), Vector3.UnitX, Vector2.UnitY
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, -1, -1), Vector3.UnitX, Vector2.One
                    )),

                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, -1, 1), -Vector3.UnitY, Vector2.Zero
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, -1, 1), -Vector3.UnitY, Vector2.UnitX
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, -1, -1), -Vector3.UnitY, Vector2.UnitY
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, -1, -1), -Vector3.UnitY, Vector2.One
                    )),

                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, 1, -1), Vector3.UnitY, Vector2.Zero
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, 1, -1), Vector3.UnitY, Vector2.UnitX
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, 1, 1), Vector3.UnitY, Vector2.UnitY
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, 1, 1), Vector3.UnitY, Vector2.One
                    )),

                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, 1, -1), -Vector3.UnitZ, Vector2.Zero
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, 1, -1), -Vector3.UnitZ, Vector2.UnitX
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, -1, -1), -Vector3.UnitZ, Vector2.UnitY
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, -1, -1), -Vector3.UnitZ, Vector2.One
                    )),

                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, 1, 1), Vector3.UnitZ, Vector2.Zero
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, 1, 1), Vector3.UnitZ, Vector2.UnitX
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(-1, -1, 1), Vector3.UnitZ, Vector2.UnitY
                    )),
                new MeshVertex(new VertexPositionNormalTexture(
                    new Vector3(1, -1, 1), Vector3.UnitZ, Vector2.One
                    ))
            };

            int[] inds = new int[36];
            for(int i = 0, vi = 0; i < inds.Length; ) {
                inds[i++] = vi + 0;
                inds[i++] = vi + 1;
                inds[i++] = vi + 2;
                inds[i++] = vi + 2;
                inds[i++] = vi + 1;
                inds[i++] = vi + 3;
                vi += 4;
            }

            mesh.SetData(verts, inds);
            return mesh;
        }
        public MeshData BuildCylinder(float tolerance) {
            var mesh = CreateMesh();

            return mesh;
        }
    }
}