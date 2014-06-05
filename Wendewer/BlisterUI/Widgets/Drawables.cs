using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using EGL;

namespace BlisterUI.Widgets {
    public class DrawableRect {
        // Visual Information
        public GLTexture texture;
        public Vector4 color;

        // Spatial Information
        public Vector2 location;
        public Vector2 size;
        public float layerDepth;
    }

    public class DrawableText {
        // Visual Information
        private SpriteFont font;
        public SpriteFont Font {
            get { return font; }
            set {
                font = value;
                ComputeScale();
            }
        }
        public Vector4 color;
        private string text;
        public string Text {
            get { return text; }
            set {
                text = value;
                ComputeScale();
            }
        }
        public float layerDepth;

        // Spatial Information
        public Vector2 location;
        private float height;
        public float TextHeight {
            get { return height; }
            set {
                height = value;
                ComputeScale();
            }
        }
        public float TextWidth {
            get;
            private set;
        }
        public float TextScale {
            get;
            private set;
        }

        private void ComputeScale() {
            if(font == null || string.IsNullOrWhiteSpace(text)) {
                TextScale = 1f;
                return;
            }

            // Find Sizes
            Vector2 s = font.MeasureString(text);
            TextScale = TextHeight / s.Y;
            TextWidth = s.X * TextScale;
        }
    }
}