using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public abstract class RTCamera {
        protected Vector3 viewPoint = Vector3.Zero;
        public void setViewPoint(Vector3 viewPoint) {
            this.viewPoint = viewPoint;
        }

        protected Vector3 viewDir = -Vector3.UnitZ;
        public void setViewDir(Vector3 viewDir) {
            this.viewDir = viewDir;
        }

        protected Vector3 viewUp = Vector3.UnitY;
        public void setViewUp(Vector3 viewUp) {
            this.viewUp = viewUp;
        }

        protected Vector3 projNormal = Vector3.Zero;
        public void setProjNormal(Vector3 projNormal) {
            this.projNormal = projNormal;
        }

        protected float viewWidth = 1.0f;
        public void setViewWidth(float viewWidth) {
            this.viewWidth = viewWidth;
        }

        protected float viewHeight = 1.0f;
        public void setViewHeight(float viewHeight) {
            this.viewHeight = viewHeight;
        }

        public abstract void getRay(RTRay outRay, float u, float v);
        public abstract void initView();
    }

    public class PerspectiveCamera : RTCamera {
        protected float projDistance = 1.0f;
        public void setprojDistance(float projDistance) {
            this.projDistance = projDistance;
        }

        protected Vector3 basisU = new Vector3();
        protected Vector3 basisV = new Vector3();
        protected Vector3 basisW = new Vector3();
        protected Vector3 centerDir = new Vector3();

        // Has the view been initialized?
        protected bool initialized = false;

        public override void initView() {
            if(projNormal.LengthSquared == 0) {
                projNormal = viewDir;
            }
            basisW = projNormal;
            if(Vector3.Dot(basisW, viewDir) > 0)
                basisW *= -1;
            basisW.Normalize();

            basisU = Vector3.Cross(viewUp, basisW);
            basisU.Normalize();
            basisV = Vector3.Cross(basisW, basisU);
            basisV.Normalize();

            centerDir = Vector3.Normalize(viewDir) * projDistance;

            initialized = true;
        }
        public override void getRay(RTRay outRay, float inU, float inV) {
            if(!initialized) initView();

            float u = inU * 2 - 1;
            float v = 1 - inV * 2;

            // Set the output ray
            outRay.origin = viewPoint;
            outRay.direction = centerDir;
            outRay.direction += basisU * u * viewWidth / 2;
            outRay.direction += basisV * v * viewHeight / 2;
            outRay.direction.Normalize();

            outRay.makeOffsetRay();
        }
    }

    public class ParallelCamera : RTCamera {
        protected Vector3 basisU = new Vector3();
        protected Vector3 basisV = new Vector3();
        protected Vector3 basisW = new Vector3();
        protected Vector3 centerDir = new Vector3();

        // Has the view been initialized?
        protected bool initialized = false;

        public override void initView() {
            if(projNormal.LengthSquared == 0) {
                projNormal = viewDir;
            }
            basisW = projNormal;
            if(Vector3.Dot(basisW, viewDir) > 0)
                basisW *= -1;
            basisW.Normalize();

            basisU = Vector3.Cross(viewUp, basisW);
            basisU.Normalize();
            basisV = Vector3.Cross(basisW, basisU);
            basisV.Normalize();

            centerDir = Vector3.Normalize(viewDir);

            initialized = true;
        }
        public override void getRay(RTRay outRay, float inU, float inV) {
            float u = inU * 2 - 1;
            float v = 1 - inV * 2;

            // Set the output ray
            outRay.direction = centerDir;
            outRay.origin = viewPoint;
            outRay.origin += basisU * u * viewWidth / 2;
            outRay.origin += basisV * v * viewHeight / 2;

            outRay.makeOffsetRay();
        }
    }

    public class RTGLCamera : RTCamera {
        protected Matrix4 mViewInv, mProjInv;

        public void setMatrices(Matrix4 v, Matrix4 p) {
            mViewInv = Matrix4.Invert(v);
            mProjInv = Matrix4.Invert(p);
        }

        public override void initView() {

        }

        public override void getRay(RTRay outRay, float u, float v) {
            Vector4 vNear = new Vector4(u * 2 - 1, 1 - v * 2, -1, 1);
            Vector4 vFar = new Vector4(vNear.X, vNear.Y, 1, 1);

            vNear = Vector4.Transform(vNear, mProjInv); vNear /= vNear.W;
            vNear = Vector4.Transform(vNear, mViewInv);
            vFar = Vector4.Transform(vFar, mProjInv); vFar /= vFar.W;
            vFar = Vector4.Transform(vFar, mViewInv);

            outRay.origin = new Vector3(vNear.X, vNear.Y, vNear.Z);
            outRay.direction = new Vector3(vFar.X, vFar.Y, vFar.Z);
            outRay.direction -= outRay.origin;
            outRay.direction.Normalize();
            outRay.makeOffsetRay();
        }
    }
}