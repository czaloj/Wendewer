using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wdw.Common.Property {
    public enum PropertyType {
        String,
        Float,
        Vec2,
        Vec3,
        Vec4,
        Int,
        Bool
    }
    public abstract class BaseProperty {
        public readonly PropertyType Type;

        public string Name;

        public event Action<BaseProperty> OnDataChange;

        public BaseProperty(PropertyType t) {
            Type = t;
            Name = "Unnamed";
        }

        public void BaseNotifyChange() {
            if(OnDataChange != null)
                OnDataChange(this);
        }
    }
    public abstract class DataProperty<T> : BaseProperty {
        private T data;
        public T Data {
            get { return data; }
        }

        public DataProperty(PropertyType t)
            : base(t) {
        }

        public event Action<BaseProperty, T> OnTypedDataChange;

        public void SetData(T d) {
            data = d;
            if(OnTypedDataChange != null)
                OnTypedDataChange(this, data);
            BaseNotifyChange();
        }
    }
}