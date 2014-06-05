using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using OpenTK;
using Wdw.Common.Property;

namespace Wdw.GLView.UI {
    public class BoolWidget : TextButton {
        public BoolProperty Prop {
            get;
            private set;
        }

        public BoolWidget(BoolProperty p, WidgetRenderer wr, int w, int h) :
            base(wr, w, h, new Vector4(0.08f, 0.08f, 0.08f, 1f), new Vector4(0.18f, 0.18f, 0.18f, 1f)) {
            Prop = p;
            Text = Prop.Data ? "True" : "False";
            TextColor = new Vector4(1, 1, 1, 1);
            OnButtonPress += OnBP;
            Prop.OnTypedDataChange += OnProp;
        }
        protected override void DisposeOther() {
            base.DisposeOther();
            Prop.OnTypedDataChange -= OnProp;
        }

        void OnProp(BaseProperty arg1, bool arg2) {
            Text = Prop.Data ? "True" : "False";
        }
        void OnBP(RectButton arg1, Vector2 arg2) {
            Prop.SetData(!Prop.Data);
        }
    }
}