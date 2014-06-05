using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class RTRay {
        public const float EPSILON = 1e-3f;
        public const float SHADOWEPSILON = 1e-3f;

        public Vector3 origin;
        public Vector3 direction;
        public Vector3 absorption;

        public float start;
        public float end;

        public RTRay() {
        }
        public RTRay(Vector3 newOrigin, Vector3 newDirection) {
            origin = newOrigin;
            direction = newDirection;
        }
        public RTRay(Vector3 newOrigin, Vector3 newDirection, Vector3 newAbsorption) :
            this(newOrigin, newDirection) {
            absorption = newAbsorption;
        }

        public void set(Vector3 newOrigin, Vector3 newDirection) {
            origin = newOrigin;
            direction = newDirection;
        }

        public void setAbsorption(Vector3 newAbsorption) {
            absorption = newAbsorption;
        }

        public void evaluate(out Vector3 outPoint, float t) {
            outPoint = origin + t * direction;
        }

        public void attenuate(ref Vector3 inColor, Vector3 endPoint) {
            inColor = inColor * absorption.exponentiate(-(endPoint - origin).Length).clamp(0, 1);
        }

        public void makeOffsetRay() {
            start = EPSILON;
            end = float.PositiveInfinity;
        }
        public void makeOffsetSegment(float newEnd) {
            start = EPSILON;
            end = newEnd;
        }

        public static void reflect(Vector3 p, Vector3 n, RTRay incoming, RTRay ray) {
            ray.origin = p;
            float dotN = Vector3.Dot(n, incoming.direction);
            ray.direction = n * -2 * dotN + incoming.direction;
            ray.makeOffsetRay();
        }
        public static RTRay reflect(Vector3 p, Vector3 n, RTRay incoming) {
            RTRay ray = new RTRay();
            reflect(p, n, incoming, ray);
            return ray;
        }

        public static bool refract(float n1, float n2, Vector3 p, Vector3 n, RTRay incoming, RTRay ray) {
            ray.origin = p;
            float dotdn = Vector3.Dot(incoming.direction, n);
            if(dotdn > 0) {
                Vector3 negn = -n;
                return refract(n2, n1, p, negn, incoming, ray);
            }
            double det = 1.0 - (n1 * n1 * (1.0 - dotdn * dotdn)) / (n2 * n2);
            if(det < 0) { return false; }

            ray.direction = incoming.direction + n * -dotdn;
            ray.direction *= n1 / n2;
            ray.direction += n * -(float)Math.Sqrt(det);
            ray.makeOffsetRay();
            return true;
        }
        public static RTRay refract(float n1, float n2, Vector3 p, Vector3 n, RTRay incoming) {
            RTRay ray = new RTRay();
            refract(n1, n2, p, n, incoming, ray);
            return ray;
        }
    }
}