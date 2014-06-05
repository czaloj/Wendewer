using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Wdw.Common.Events;
using Wdw.Common.Property;

namespace Wdw.Common.Data {
    public struct TextureBind {
        public int Index;
        public string Uniform;
        public TextureData Texture;

        public TextureBind(int i, string un, TextureData t) {
            Index = i;
            Uniform = un;
            Texture = t;
        }
    }

    public class MaterialData : IDisposable {
        private static readonly UUIDGen IDGen = new UUIDGen();
        public const int EVENT_CREATION = 0;
        public const int EVENT_DESTRUCTION = EVENT_CREATION + 1;
        public const int EVENT_SHADER_LOCATION = EVENT_DESTRUCTION + 1;
        public const int EVENT_PROPERTY_CHANGE = EVENT_SHADER_LOCATION + 1;
        public const int EVENT_TEXTURE = EVENT_PROPERTY_CHANGE + 1;

        public PropertyList Properties {
            get;
            private set;
        }
        public string Name {
            get { return Properties.Get<string>("Name").Data; }
            set { Properties.Get<string>("Name").SetData(value); }
        }

        private string vsFile, fsFile;
        public string VShaderFile {
            get { return vsFile; }
            set {
                vsFile = value;
                MasterData.SendEvent(new DataEvent(this, EVENT_SHADER_LOCATION));
            }
        }
        public string FShaderFile {
            get { return fsFile; }
            set {
                fsFile = value;
                MasterData.SendEvent(new DataEvent(this, EVENT_SHADER_LOCATION));
            }
        }
        private Dictionary<string, BaseProperty> uniforms;
        public IEnumerable<BaseProperty> Uniforms {
            get { return uniforms.Values; }
        }

        public List<TextureBind> Textures {
            get;
            private set;
        }

        public MaterialData() {
            vsFile = null;
            uniforms = new Dictionary<string, BaseProperty>();
            Textures = new List<TextureBind>();

            Properties = new PropertyList();
            Properties.Add(new StringProperty("Name"));
            Name = "Material." + IDGen.Obtain();

            MasterData.SendEvent(new DataEvent(this, EVENT_CREATION));
        }
        public void Dispose() {
            MasterData.SendEvent(new DataEvent(this, EVENT_DESTRUCTION));
        }

        public void AddUniform(string name, PropertyType t) {
            BaseProperty p;
            switch(t) {
                case PropertyType.Int:
                    p = new IntProperty(name);
                    p.OnDataChange += OnDataChange;
                    uniforms.Add(name, p);
                    break;
                case PropertyType.Float:
                    p = new FloatProperty(name);
                    p.OnDataChange += OnDataChange;
                    uniforms.Add(name, p);
                    break;
                case PropertyType.Vec2:
                    p = new Vec2Property(name);
                    p.OnDataChange += OnDataChange;
                    uniforms.Add(name, p);
                    break;
                case PropertyType.Vec3:
                    p = new Vec3Property(name);
                    p.OnDataChange += OnDataChange;
                    uniforms.Add(name, p);
                    break;
                case PropertyType.Vec4:
                    p = new Vec4Property(name);
                    p.OnDataChange += OnDataChange;
                    uniforms.Add(name, p);
                    break;
                default:
                    return;
            }
        }
        public void ChangeUniform(string name, int f) {
            IntProperty p = uniforms[name] as IntProperty;
            if(p == null) return;
            p.SetData(f);
        }
        public void ChangeUniform(string name, float f) {
            FloatProperty p = uniforms[name] as FloatProperty;
            if(p == null) return;
            p.SetData(f);
        }
        public void ChangeUniform(string name, Vector2 f) {
            Vec2Property p = uniforms[name] as Vec2Property;
            if(p == null) return;
            p.SetData(f);
        }
        public void ChangeUniform(string name, Vector3 f) {
            Vec3Property p = uniforms[name] as Vec3Property;
            if(p == null) return;
            p.SetData(f);
        }
        public void ChangeUniform(string name, Vector4 f) {
            Vec4Property p = uniforms[name] as Vec4Property;
            if(p == null) return;
            p.SetData(f);
        }

        public void AddTexture(int i, string un, TextureData t) {
            Textures.Add(new TextureBind(i, un, t));
            MasterData.SendEvent(new DataEvent(this, EVENT_TEXTURE));
        }
        public void RemoveTexture(int i) {
            Textures.RemoveAll((t) => { return t.Index == i; });
            MasterData.SendEvent(new DataEvent(this, EVENT_TEXTURE));
        }

        public void OnDataChange(BaseProperty p) {
            MasterData.SendEvent(new DataEvent(this, EVENT_PROPERTY_CHANGE));
        }
    }
}