using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class BVHNode {
        public BoundingBox bound;
        public Vector3 MinBound {
            get { return bound.Min; }
        }
        public Vector3 MaxBound {
            get { return bound.Max; }
        }

        public BVHNode lChild, rChild;
        public bool IsLeaf {
            get {
                return lChild == null && rChild == null;
            }
        }

        public int surfaceIndexStart, surfaceIndexEnd;

        public float Volume {
            get {
                return
                    (bound.Max.X - bound.Min.X) *
                    (bound.Max.Y - bound.Min.Y) *
                    (bound.Max.Z - bound.Min.Z)
                    ;
            }
        }
        public IEnumerable<BVHNode> Children {
            get {
                yield return lChild;
                yield return rChild;
            }
        }

        public BVHNode() {
            bound = new BoundingBox();
            lChild = null;
            rChild = null;
            surfaceIndexStart = -1;
            surfaceIndexEnd = -1;
        }
        public BVHNode(Vector3 minBound, Vector3 maxBound, BVHNode leftChild, BVHNode rightChild, int start, int end) {
            bound = new BoundingBox(minBound, maxBound);
            lChild = leftChild;
            rChild = rightChild;
            surfaceIndexStart = start;
            surfaceIndexEnd = end;
        }

        public void GetAVD(ref Vector2 f) {
            float v = Volume;
            foreach(BVHNode c in Children) {
                if(c == null) continue;
                float cv = c.Volume;
                f.X += cv / v;
                f.Y++;
                c.GetAVD(ref f);
            }
        }

        public bool Intersects(RTRay ray) {
            float tMin, tMax;
            bool hit = false;
            float tEntry = float.MaxValue, tExit = -float.MaxValue;

            // solve parametric equations to find each intersection point
            if(ray.direction.X > 0) {
                tMin = (MinBound.X - ray.origin.X) / ray.direction.X;
                tMax = (MaxBound.X - ray.origin.X) / ray.direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            else if(ray.direction.X < 0) {
                tMax = (MinBound.X - ray.origin.X) / ray.direction.X;
                tMin = (MaxBound.X - ray.origin.X) / ray.direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            if(ray.direction.Y > 0) {
                tMin = (MinBound.Y - ray.origin.Y) / ray.direction.Y;
                tMax = (MaxBound.Y - ray.origin.Y) / ray.direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.direction.Y < 0) {
                tMax = (MinBound.Y - ray.origin.Y) / ray.direction.Y;
                tMin = (MaxBound.Y - ray.origin.Y) / ray.direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            if(ray.direction.Z > 0) {
                tMin = (MinBound.Z - ray.origin.Z) / ray.direction.Z;
                tMax = (MaxBound.Z - ray.origin.Z) / ray.direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.direction.Z < 0) {
                tMax = (MinBound.Z - ray.origin.Z) / ray.direction.Z;
                tMin = (MaxBound.Z - ray.origin.Z) / ray.direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            return hit && (tEntry <= tExit);
        }
        public float IntersectTime(RTRay ray) {
            float tMin, tMax;
            bool hit = false;
            float tEntry = float.MaxValue, tExit = -float.MaxValue;

            // Find Collision Point
            if(ray.direction.X > 0) {
                tMin = (MinBound.X - ray.origin.X) / ray.direction.X;
                tMax = (MaxBound.X - ray.origin.X) / ray.direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            else if(ray.direction.X < 0) {
                tMax = (MinBound.X - ray.origin.X) / ray.direction.X;
                tMin = (MaxBound.X - ray.origin.X) / ray.direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            if(ray.direction.Y > 0) {
                tMin = (MinBound.Y - ray.origin.Y) / ray.direction.Y;
                tMax = (MaxBound.Y - ray.origin.Y) / ray.direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.direction.Y < 0) {
                tMax = (MinBound.Y - ray.origin.Y) / ray.direction.Y;
                tMin = (MaxBound.Y - ray.origin.Y) / ray.direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            if(ray.direction.Z > 0) {
                tMin = (MinBound.Z - ray.origin.Z) / ray.direction.Z;
                tMax = (MaxBound.Z - ray.origin.Z) / ray.direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.direction.Z < 0) {
                tMax = (MinBound.Z - ray.origin.Z) / ray.direction.Z;
                tMin = (MaxBound.Z - ray.origin.Z) / ray.direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            if(hit && (tEntry <= tExit)) return tEntry;
            return float.NaN;
        }
    }

    public class BVH : IAccelStruct {
        private static readonly TriAxisSorterX F_SORT_X = new TriAxisSorterX();
        private static readonly TriAxisSorterY F_SORT_Y = new TriAxisSorterY();
        private static readonly TriAxisSorterZ F_SORT_Z = new TriAxisSorterZ();

        private Surface[] surfaces;
        private BVHNode root;

        public BVH() {

        }
        
        public bool intersect(IntersectionRecord outRecord, RTRay rayIn, bool anyIntersect) {
            if(!root.Intersects(rayIn)) return false;
            return IntersectHelper(root, outRecord, rayIn, anyIntersect);
        }
        private bool IntersectHelper(BVHNode node, IntersectionRecord outRecord, RTRay rayIn, bool anyIntersect) {
            if(node.IsLeaf) {
                outRecord.t = float.MaxValue;
                IntersectionRecord tempRec = new IntersectionRecord();
                for(int i = node.surfaceIndexStart; i < node.surfaceIndexEnd; i++) {
                    if(surfaces[i].intersect(tempRec, rayIn)) {
                        // check if current t value is smaller
                        if(tempRec.t < outRecord.t) {
                            outRecord.set(tempRec);
                        }
                    }
                }
                return outRecord.t != float.MaxValue;
            }
            else {
                if(anyIntersect) {
                    float t1 = node.lChild.IntersectTime(rayIn);
                    float t2 = node.rChild.IntersectTime(rayIn);
                    if(float.IsNaN(t1)) {
                        if(float.IsNaN(t2)) return false;
                        return IntersectHelper(node.rChild, outRecord, rayIn, true);
                    }
                    if(float.IsNaN(t2)) return IntersectHelper(node.lChild, outRecord, rayIn, true);

                    // Do In Speedy Fashion
                    if(t2 < t1)
                        return IntersectHelper(node.rChild, outRecord, rayIn, true) || IntersectHelper(node.lChild, outRecord, rayIn, true);
                    else
                        return IntersectHelper(node.lChild, outRecord, rayIn, true) || IntersectHelper(node.rChild, outRecord, rayIn, true);
                }
                else {
                    float t1 = node.lChild.IntersectTime(rayIn);
                    float t2 = node.rChild.IntersectTime(rayIn);
                    if(float.IsNaN(t1)) {
                        if(float.IsNaN(t2)) return false;
                        return IntersectHelper(node.rChild, outRecord, rayIn, true);
                    }
                    if(float.IsNaN(t2)) return IntersectHelper(node.lChild, outRecord, rayIn, true);

                    // Need To Check Both
                    IntersectionRecord lRec = new IntersectionRecord();
                    IntersectionRecord rRec = new IntersectionRecord();
                    bool lHit, rHit;
                    lHit = IntersectHelper(node.lChild, lRec, rayIn, false);
                    rHit = IntersectHelper(node.rChild, rRec, rayIn, false);
                    if(lHit && rHit)
                        outRecord.set(lRec.t <= rRec.t ? lRec : rRec);
                    else if(lHit)
                        outRecord.set(lRec);
                    else if(rHit)
                        outRecord.set(rRec);
                    else
                        return false;
                    return true;
                }
            }
        }
        public void build(Surface[] surfaces) {
            this.surfaces = surfaces;
            root = CreateTree(0, surfaces.Length);
        }
        private BVHNode CreateTree(int start, int end) {
            Vector3 minB = new Vector3(float.MaxValue);
            Vector3 maxB = new Vector3(-float.MaxValue);
            for(int i = start; i < end; i++) {
                minB = Vector3.Min(surfaces[i].getMinBound(), minB);
                maxB = Vector3.Max(surfaces[i].getMaxBound(), maxB);
            }

            // Check For Leaf Node Condition
            if(end - start <= 1) {
                return new BVHNode(minB, maxB, null, null, start, end);
            }

            // Sort On Widest Dimension
            Vector3 dim = maxB - minB;
            if(dim.X >= dim.Y && dim.X >= dim.Z) Array.Sort(surfaces, start, end - start, F_SORT_X);
            else if(dim.Y >= dim.Z) Array.Sort(surfaces, start, end - start, F_SORT_Y);
            else Array.Sort(surfaces, start, end - start, F_SORT_Z);

            // Create Children
            int e = (start + end) / 2;
            BVHNode leftChild = CreateTree(start, e);
            BVHNode rightChild = CreateTree(e, end);

            return new BVHNode(minB, maxB, leftChild, rightChild, start, end);
        }

        class TriAxisSorterX : IComparer<Surface> {
            public int Compare(Surface x, Surface y) {
                float v1 = x.getAveragePosition().X;
                float v2 = y.getAveragePosition().X;
                return v1.CompareTo(v2);
            }
        }
        class TriAxisSorterY : IComparer<Surface> {
            public int Compare(Surface x, Surface y) {
                float v1 = x.getAveragePosition().Y;
                float v2 = y.getAveragePosition().Y;
                return v1.CompareTo(v2);
            }
        }
        class TriAxisSorterZ : IComparer<Surface> {
            public int Compare(Surface x, Surface y) {
                float v1 = x.getAveragePosition().Z;
                float v2 = y.getAveragePosition().Z;
                return v1.CompareTo(v2);
            }
        }
    }
}