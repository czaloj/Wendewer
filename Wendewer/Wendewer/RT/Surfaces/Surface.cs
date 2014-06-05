using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public abstract class Surface {
        protected Matrix4 tMat;
        protected Matrix4 tMatInv;
        protected Matrix4 tMatTInv;

        protected Vector3 averagePosition;
        protected Vector3 minBound;
        protected Vector3 maxBound;

        /** The absorption coefficient inside the surface */
        protected Vector3 insideAbsorption;
        public void setInsideAbsorption(Vector3 value) { insideAbsorption = value; }
        public Vector3 getInsideAbsorption() { return insideAbsorption; }

        /** The absorption coefficient outside the surface */
        protected Vector3 outsideAbsorption;
        public void setOutsideAbsorption(Vector3 value) { outsideAbsorption = value; }
        public Vector3 getOutsideAbsorption() { return outsideAbsorption; }

        /** Shader to be used to shade this surface. */
        protected Shader shader = Shader.DEFAULT_SHADER;
        public void setShader(Shader shader) { this.shader = shader; }
        public Shader getShader() { return shader; }

        public Vector3 getAveragePosition() { return averagePosition; }
        public Vector3 getMinBound() { return minBound; }
        public Vector3 getMaxBound() { return maxBound; }

        public RTRay untransformRay(RTRay rayIn) {
            RTRay ray = new RTRay(rayIn.origin, rayIn.direction);
            ray.start = rayIn.start;
            ray.end = rayIn.end;
            ray.direction = Vector3.TransformNormal(ray.direction, tMatInv);
            ray.origin = Vector3.Transform(ray.origin, tMatInv);
            return ray;
        }

        public void setTransformation(Matrix4 a) {
            Matrix4 aInv = Matrix4.Invert(a);
            Matrix4 aTInv = Matrix4.Transpose(aInv);
            setTransformation(a, aInv, aTInv);
        }
        public void setTransformation(Matrix4 a, Matrix4 aInv, Matrix4 aTInv) {
            tMat = a;
            tMatInv = aInv;
            tMatTInv = aTInv;
            computeBoundingBox();
        }

        public abstract bool intersect(IntersectionRecord outRecord, RTRay ray);

        public abstract void computeBoundingBox();

        public void appendRenderableSurfaces(List<Surface> l) {
            l.Add(this);
        }
    }
}