using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wdw.RT {
    public interface IAccelStruct {
        void build(Surface[] surfaces);
        bool intersect(IntersectionRecord outRecord, RTRay rayIn, bool anyIntersection);
    }
}
