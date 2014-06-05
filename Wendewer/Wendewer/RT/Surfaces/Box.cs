using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class Box : Surface {
        public Vector3 minPt = -Vector3.One;
        public Vector3 maxPt = Vector3.One;

        public override bool intersect(IntersectionRecord outRecord, RTRay rayIn) {
            RTRay ray = untransformRay(rayIn);

            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            float ox = o.X;
            float oy = o.Y;
            float oz = o.Z;
            float dx = d.X;
            float dy = d.Y;
            float dz = d.Z;

            // a three-slab intersection test. We'll get in and out t values for
            // all three axes. For instance on the x axis:
            // o.x + t d.x = 1 => t = (1 - o.x) / d.x
            // o.x + t d.x = -1 => t = (-1 - o.x) / d.x
            // This code is straight from Shirley's section 10.9.1

            float tMin = ray.start, tMax = ray.end;

            float txMin, txMax;
            if(dx >= 0) {
                txMin = (minPt.X - ox) / dx;
                txMax = (maxPt.X - ox) / dx;
            }
            else {
                txMin = (maxPt.X - ox) / dx;
                txMax = (minPt.X - ox) / dx;
            }
            if(tMin > txMax || txMin > tMax)
                return false;
            if(txMin > tMin)
                tMin = txMin;
            if(txMax < tMax)
                tMax = txMax;

            float tyMin, tyMax;
            if(dy >= 0) {
                tyMin = (minPt.Y - oy) / dy;
                tyMax = (maxPt.Y - oy) / dy;
            }
            else {
                tyMin = (maxPt.Y - oy) / dy;
                tyMax = (minPt.Y - oy) / dy;
            }
            if(tMin > tyMax || tyMin > tMax)
                return false;
            if(tyMin > tMin)
                tMin = tyMin;
            if(tyMax < tMax)
                tMax = tyMax;

            float tzMin, tzMax;
            if(dz >= 0) {
                tzMin = (minPt.Z - oz) / dz;
                tzMax = (maxPt.Z - oz) / dz;
            }
            else {
                tzMin = (maxPt.Z - oz) / dz;
                tzMax = (minPt.Z - oz) / dz;
            }
            if(tMin > tzMax || tzMin > tMax)
                return false;
            if(tzMin > tMin)
                tMin = tzMin;
            if(tzMax < tMax)
                tMax = tzMax;

            if(outRecord != null) {
                Vector3 sp = ray.origin;
                sp += (float)RTRay.EPSILON * ray.direction;
                if(sp.X >= minPt.X && sp.X <= maxPt.X && sp.Y >= minPt.Y && sp.Y <= maxPt.Y && sp.Z >= minPt.Z && sp.Z <= maxPt.Z) {
                    outRecord.t = tMax;
                    ray.evaluate(out outRecord.location, tMax);
                    if(tMax == txMax)
                        outRecord.normal = Vector3.UnitX;
                    else if(tMax == tyMax)
                        outRecord.normal = Vector3.UnitY;
                    else
                        outRecord.normal = Vector3.UnitZ;

                    if(Vector3.Dot(outRecord.normal, ray.direction) < 0)
                        outRecord.normal *= -1;
                }
                else {
                    outRecord.t = tMin;
                    ray.evaluate(out outRecord.location, tMin);
                    if(tMin == txMin)
                        outRecord.normal = Vector3.UnitX;
                    else if(tMin == tyMin)
                        outRecord.normal = Vector3.UnitY;
                    else
                        outRecord.normal = Vector3.UnitZ;

                    if(Vector3.Dot(outRecord.normal, ray.direction) > 0)
                        outRecord.normal *= -1;

                }
                outRecord.surface = this;

                // TODO 2
                outRecord.location = Vector3.Transform(outRecord.location, tMat);
                outRecord.normal = Vector3.TransformNormal(outRecord.normal, tMatTInv);
                outRecord.normal.Normalize();
            }

            return true;
        }
        public override void computeBoundingBox() {
            // the corners of a box
            Vector3[] corners = new Vector3[] {
			    new Vector3(minPt.X, minPt.Y, minPt.Z),
			    new Vector3(minPt.X, minPt.Y, maxPt.Z),
			    new Vector3(maxPt.X, minPt.Y, maxPt.Z),
			    new Vector3(maxPt.X, minPt.Y, minPt.Z),
			    new Vector3(minPt.X, maxPt.Y, maxPt.Z),
			    new Vector3(minPt.X, maxPt.Y, minPt.Z),
			    new Vector3(maxPt.X, maxPt.Y, minPt.Z),
			    new Vector3(maxPt.X, maxPt.Y, maxPt.Z)
	        };

            Vector3 minBound = new Vector3(float.MaxValue);
            Vector3 maxBound = new Vector3(-float.MaxValue);

            // transform and check corners for min/max
            foreach(Vector3 tp in corners) {
                Vector3 p = Vector3.Transform(tp, tMat);
                minBound = Vector3.Min(minBound, p);
                maxBound = Vector3.Max(maxBound, p);
            }
            averagePosition = (minBound + maxBound) * 0.5f;
        }
    }
}
