using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wdw.Common.Property {
    public class PropertyList {
        private Dictionary<string, BaseProperty> props;
        public BaseProperty this[string name] {
            get {
                BaseProperty p;
                if(!props.TryGetValue(name, out p))
                    return null;
                return p;
            }
            set {
                if(name == null || value == null || props.ContainsKey(name))
                    return;
                props.Add(name, value);
            }
        }

        public PropertyList() {
            props = new Dictionary<string, BaseProperty>();
        }

        public void Add(BaseProperty p) {
            this[p.Name] = p;
        }

        public DataProperty<T> Get<T>(string name) {
            BaseProperty p;
            if(!props.TryGetValue(name, out p))
                return null;
            return p as DataProperty<T>;
        }
        public void Set<T>(string name, T value) {
            var p = Get<T>(name);
            if(p != null) p.SetData(value);
        }
    }
}