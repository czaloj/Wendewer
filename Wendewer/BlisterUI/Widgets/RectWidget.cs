using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using EGL;

namespace BlisterUI.Widgets {
    public class RectWidget : BaseWidget {
        // Where To Draw To Screen
        protected DrawableRect drawRect;

        public GLTexture Texture {
            get { return drawRect.texture; }
            set { drawRect.texture = value; }
        }
        public Vector4 Color {
            get { return drawRect.color; }
            set { drawRect.color = value; }
        }

        public RectWidget(WidgetRenderer r, GLTexture t = null)
            : base(r) {
            Texture = t == null ? r.DefaultTexture : t;
        }

        public override void PreInit() {
            drawRect = new DrawableRect();
            OnRecompute += OnSelfCompute;
        }
        protected override void DisposeOther() {
        }

        public override void AddAllDrawables(WidgetRenderer r) {
            r.Add(drawRect);
        }
        public override void RemoveAllDrawables(WidgetRenderer r) {
            r.Remove(drawRect);
        }

        private void OnSelfCompute(BaseWidget w) {
            drawRect.location.X = widgetRect.X;
            drawRect.location.Y = widgetRect.Y;
            drawRect.size.X = widgetRect.Width;
            drawRect.size.Y = widgetRect.Height;
            drawRect.layerDepth = layer;
        }
    }
}