using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class Sphere : Surface {
        protected Vector3 center = Vector3.Zero;
        public void setCenter(Vector3 center) {
            this.center = center;
        }

        protected float radius = 1.0f;
        public void setRadius(float radius) {
            this.radius = radius;
        }

        public Sphere() {
        }

        public override bool intersect(IntersectionRecord outRecord, RTRay rayIn) {
            RTRay ray = untransformRay(rayIn);
            ray.origin -= center;

            // http://wiki.cgsociety.org/index.php/Ray_Sphere_Intersection
            float a = ray.direction.LengthSquared;
            float b = 2 * Vector3.Dot(ray.direction, ray.origin);
            float c = ray.origin.LengthSquared - (radius * radius);
            float disc = b * b - 4 * a * c;

            if(disc < 0) return false;

            float distSqrt = (float)Math.Sqrt(disc);
            float q;
            if(b < 0) q = (-b - distSqrt) / 2.0f;
            else q = (-b + distSqrt) / 2.0f;

            // compute t0 and t1
            float t0 = q / a;
            float t1 = c / q;

            if(t0 > t1) {
                float temp = t0;
                t0 = t1;
                t1 = temp;
            }
            if(t1 < 0) return false;

            // There was an intersection, fill out the intersection record
            if(outRecord != null) {
                outRecord.t = t0 < 0 ? t1 : t0;
                ray.evaluate(out outRecord.location, outRecord.t);
                outRecord.surface = this;
                outRecord.normal = outRecord.location;
                outRecord.location = Vector3.Transform(outRecord.location + center, tMat);
                outRecord.normal = Vector3.TransformNormal(outRecord.normal, tMatTInv);
                if(outRecord.normal.Y > 0)
                    outRecord.normal.Normalize();
                else
                    outRecord.normal.Normalize();
            }

            return true;

        }

        public override void computeBoundingBox() {
            // TODO 14
            // the corners of a bounding box for sphere
            Vector3[] corners = new Vector3[] {
			    new Vector3(center.X+radius, center.Y+radius, center.Z-radius),
			    new Vector3(center.X-radius, center.Y+radius, center.Z-radius),
			    new Vector3(center.X-radius, center.Y-radius, center.Z-radius),
			    new Vector3(center.X+radius, center.Y-radius, center.Z-radius),
			    new Vector3(center.X+radius, center.Y+radius, center.Z+radius),
			    new Vector3(center.X-radius, center.Y+radius, center.Z+radius),
			    new Vector3(center.X-radius, center.Y-radius, center.Z+radius),
			    new Vector3(center.X+radius, center.Y-radius, center.Z+radius)
	        };

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(-float.MaxValue);

            // transform and check corners for min/max
            foreach(Vector3 p in corners) {
                Vector3 pt = Vector3.Transform(p, tMat);
                min = Vector3.Min(min, pt);
                max = Vector3.Max(max, pt);
            }

            minBound = min;
            maxBound = max;

            // set averagePosition
            averagePosition = (min + max) * 0.5f;
        }

        public override string ToString() {
            return "sphere " + center + " " + radius + " " + shader + " end";
        }
    }
}