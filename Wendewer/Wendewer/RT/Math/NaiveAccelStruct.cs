using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wdw.RT {
    public class NaiveAccelStruct : IAccelStruct {
        private Surface[] surfaces;

        public NaiveAccelStruct() {
        }

        public bool intersect(IntersectionRecord outRecord, RTRay rayIn, bool anyIntersection) {
            bool ret = false;
            IntersectionRecord tmp = new IntersectionRecord();
            RTRay ray = new RTRay(rayIn.origin, rayIn.direction);
            ray.start = rayIn.start;
            ray.end = rayIn.end;
            for(int i = 0; i < surfaces.Length; i++) {
                if(surfaces[i].intersect(tmp, ray) && tmp.t < ray.end) {
                    if(anyIntersection) return true;
                    ret = true;
                    ray.end = tmp.t;
                    if(outRecord != null)
                        outRecord.set(tmp);
                }
            }
            return ret;
        }

        public void build(Surface[] surfaces) {
            this.surfaces = surfaces;
        }
    }
}