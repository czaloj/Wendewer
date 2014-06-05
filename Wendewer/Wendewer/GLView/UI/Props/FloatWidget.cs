using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using OpenTK;
using OpenTK.Input;
using Wdw.Common.Property;

namespace Wdw.GLView.UI {
    public class FloatWidget : TextButton {
        public FloatProperty Prop {
            get;
            private set;
        }

        float orig;
        Vector2 mp;

        public FloatWidget(FloatProperty p, WidgetRenderer wr, int w, int h) :
            base(wr, w, h, Vector4.One * 0.7f, Vector4.One) {
            Prop = p;
            Text = Prop.Data.ToString();
            OnButtonPress += OnBP;
            Prop.OnTypedDataChange += OnProp;
        }

        void OnProp(BaseProperty arg1, float arg2) {
            Text = Prop.Data.ToString();
        }
        void OnBP(RectButton arg1, Vector2 arg2) {
            mp = arg2;
            orig = Prop.Data;
            MouseEventDispatcher.OnMouseMotion += OnMM;
            MouseEventDispatcher.OnMouseRelease += OnMR;
        }

        void OnMM(object sender, MouseMoveEventArgs e) {
            Vector2 pos = new Vector2(e.X, e.Y);
            pos -= mp;
            float exp = (float)Math.Pow(2, pos.X);
            Prop.SetData(orig + pos.Y * 10 * exp);
        }
        void OnMR(object sender, MouseButtonEventArgs e) {
            MouseEventDispatcher.OnMouseMotion -= OnMM;
            MouseEventDispatcher.OnMouseRelease -= OnMR;
        }
    }
}