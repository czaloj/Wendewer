using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGL.Helpers;
using OpenTK;
using Wdw.Common.Events;
using Wdw.Common.Property;

namespace Wdw.Common.Data {
    public class ObjectData : IDisposable {
        private static readonly UUIDGen IDGen = new UUIDGen();

        public const int EVENT_CREATION = 0;
        public const int EVENT_DESTRUCTION = EVENT_CREATION + 1;
        public const int EVENT_TRANSFORM = EVENT_DESTRUCTION + 1;
        public const int EVENT_PARENT = EVENT_TRANSFORM + 1;
        public const int EVENT_MESH = EVENT_PARENT + 1;
        public const int EVENT_MATERIAL = EVENT_MESH + 1;

        public uint UUID {
            get;
            private set;
        }

        public PropertyList Properties {
            get;
            private set;
        }
        public string Name {
            get { return Properties.Get<string>("Name").Data; }
            set { Properties.Get<string>("Name").SetData(value); }
        }

        private ObjectData parent;
        public ObjectData Parent {
            get { return parent; }
            set {
                if(parent == value) return;
                if(parent != null)
                    parent.Children.Remove(this);
                parent = value;
                if(parent != null)
                    parent.Children.Add(this);
                MasterData.SendEvent(new DataEvent(this, EVENT_PARENT));
            }
        }
        public List<ObjectData> Children {
            get;
            private set;
        }

        private MeshData mesh;
        public MeshData Mesh {
            get { return mesh; }
            set {
                if(mesh == value) return;
                mesh = value;
                MasterData.SendEvent(new DataEvent(this, EVENT_MESH));
            }
        }

        private MaterialData material;
        public MaterialData Material {
            get { return material; }
            set {
                if(material == value) return;
                material = value;
                MasterData.SendEvent(new DataEvent(this, EVENT_MATERIAL));
            }
        }

        private Matrix4 transform, mWorld;
        public Matrix4 Transform {
            get { return transform; }
            set {
                transform = value;
                PropagateTransform();
                MasterData.SendEvent(new DataEvent(this, EVENT_TRANSFORM));
            }
        }
        public Matrix4 WorldTransform {
            get { return mWorld; }
        }

        public ObjectData() {
            UUID = GLPicker.ObtainUUID();

            transform = Matrix4.Identity;
            Children = new List<ObjectData>();
            mesh = null;
            material = null;

            Properties = new PropertyList();
            Properties.Add(new StringProperty("Name"));
            Name = "Object." + IDGen.Obtain();
            Properties.Add(new Vec3Property("Translation"));
            Properties.Add(new Vec3Property("Rotation"));
            Properties.Add(new Vec3Property("Scale"));

            MasterData.SendEvent(new DataEvent(this, EVENT_CREATION));
        }
        public void Dispose() {
            GLPicker.RecycleUUID(UUID);
            MasterData.SendEvent(new DataEvent(this, EVENT_DESTRUCTION));
        }

        private void PropagateTransform() {
            if(parent != null)
                mWorld = transform * parent.WorldTransform;
            else
                mWorld = transform;
            foreach(ObjectData c in Children) {
                c.PropagateTransform();
            }
        }
    }
}