using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.Common.Property {
    public class BoolProperty : DataProperty<bool> {
        public BoolProperty()
            : base(PropertyType.Bool) {
        }
        public BoolProperty(string name)
            : this() {
            Name = name;
        }
    }
    public class IntProperty : DataProperty<int> {
        public IntProperty()
            : base(PropertyType.Int) {
        }
        public IntProperty(string name)
            : this() {
            Name = name;
        }
    }
    public class FloatProperty : DataProperty<float> {
        public FloatProperty()
            : base(PropertyType.Float) {
        }
        public FloatProperty(string name)
            : this() {
            Name = name;
        }
    }
    public class Vec2Property : DataProperty<Vector2> {
        public Vec2Property()
            : base(PropertyType.Vec2) {
        }
        public Vec2Property(string name)
            : this() {
            Name = name;
        }
    }
    public class Vec3Property : DataProperty<Vector3> {
        public Vec3Property()
            : base(PropertyType.Vec3) {
        }
        public Vec3Property(string name)
            : this() {
            Name = name;
        }
    }
    public class Vec4Property : DataProperty<Vector4> {
        public Vec4Property()
            : base(PropertyType.Vec4) {
        }
        public Vec4Property(string name)
            : this() {
            Name = name;
        }
    }
    public class StringProperty : DataProperty<string> {
        public StringProperty()
            : base(PropertyType.String) {
        }
        public StringProperty(string name)
            : this() {
            Name = name;
        }
    }
}