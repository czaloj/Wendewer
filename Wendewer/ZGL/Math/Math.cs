using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;

namespace OpenTK {
    public static class OTKExt {
        public static Color4 Lerp(this Color4 a, Color4 b, float r) {
            float ir = 1 - r;
            return new Color4(
                ir * a.R + r * b.R,
                ir * a.G + r * b.G,
                ir * a.B + r * b.B,
                ir * a.A + r * b.A
                );
        }
    }

    public struct BoundingBox {
        public Vector3 Min, Max;

        public BoundingBox(Vector3 min, Vector3 max) {
            Min = min;
            Max = max;
        }
    }

    public struct Ray {
        public Vector3 Position;
        public Vector3 Direction;

        public float? Intersects(BoundingBox box) {
            return null;
        }
    }
}
