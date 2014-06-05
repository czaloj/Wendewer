using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Wdw.Common.Data;
using Wdw.Common.Property;

namespace Wdw.Common.Operators {
    public abstract class OpBase {
        public abstract string Name {
            get;
        }

        public PropertyList Properties {
            get;
            private set;
        }

        public OpBase() {
            Properties = new PropertyList();
        }

        public abstract void Initialize(MasterData data);
        public abstract void Destroy(MasterData data);

        public abstract void Operate(MasterData data);

        public abstract void Display(WidgetRenderer wr, BaseWidget panel, ref LinkedList<IDisposable> toDispose);
    }
}