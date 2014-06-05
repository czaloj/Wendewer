using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTK {
    public static class Ext {
        public static Vector3 exponentiate(this Vector3 v, double f) {
            v.X = (float)Math.Exp(f * v.X);
            v.Y = (float)Math.Exp(f * v.Y);
            v.Z = (float)Math.Exp(f * v.Z);
            return v;
        }
        public static Vector3 clamp(this Vector3 v, float min, float max) {
            return new Vector3(
                MathHelper.Clamp(v.X, min, max),
                MathHelper.Clamp(v.Y, min, max),
                MathHelper.Clamp(v.Z, min, max)
                );
        }
        public static Vector3 gammaCorrect(this Vector3 color, double gamma) {
            double inverseGamma = 1.0f / gamma;
            return new Vector3(
                (float)Math.Pow(color.X, inverseGamma),
                (float)Math.Pow(color.Y, inverseGamma),
                (float)Math.Pow(color.Z, inverseGamma)
            );
        }
        public static float getGrey(this Vector3 color) {
            return 0.2f * color.X + 0.7f * color.Y + 0.1f * color.Z;
        }
    }
}