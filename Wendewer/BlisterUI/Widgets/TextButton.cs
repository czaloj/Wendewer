using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGL;
using OpenTK;

namespace BlisterUI.Widgets {
    public class TextButton : RectButton {
        public TextWidget TextWidget {
            get;
            private set;
        }
        public string Text {
            get { return TextWidget.Text; }
            set { TextWidget.Text = value; }
        }
        public Vector4 TextColor {
            get { return TextWidget.Color; }
            set { TextWidget.Color = value; }
        }

        public TextButton(WidgetRenderer wr, int w, int h, Vector4 cInactive, Vector4 cActive, SpriteFont font = null, GLTexture t = null)
            : base(wr, w, h, cInactive, cActive, t) {
            TextWidget = new TextWidget(wr, font);
            TextWidget.AlignX = Alignment.MID;
            TextWidget.AlignY = Alignment.MID;
            TextWidget.Height = h;
            TextWidget.OffsetAlignX = Alignment.MID;
            TextWidget.OffsetAlignY = Alignment.MID;
            TextWidget.Parent = this;
        }

        protected override void DisposeOther() {
            TextWidget.Dispose();
            TextWidget = null;
            base.DisposeOther();
        }
    }
}