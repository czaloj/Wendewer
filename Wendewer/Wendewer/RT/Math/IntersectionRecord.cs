using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class IntersectionRecord {
        public Vector3 location;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 bitangent;
        public Vector2 texCoords;
        public Surface surface;
        public float t = 0;

        public void set(IntersectionRecord rec) {
            location = rec.location;
            normal = rec.normal;
            tangent = rec.tangent;
            bitangent = rec.bitangent;
            texCoords = rec.texCoords;
            surface = rec.surface;
            t = rec.t;
        }
    }
}