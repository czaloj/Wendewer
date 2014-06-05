using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class PointLight : Light {
        public PointLight()
            : base() {
        }

        public override Vector3 getIntensityAt(Scene scene, IntersectionRecord record) {
            RTRay shadowRay = new RTRay(position, Vector3.Zero);
            shadowRay.direction = position - record.location;
            shadowRay.end = shadowRay.direction.Length - RTRay.SHADOWEPSILON;
            shadowRay.direction.Normalize();
            shadowRay.start = RTRay.SHADOWEPSILON;
            shadowRay.setAbsorption(scene.getAbsorption());
            Vector3 c = Vector3.Zero;
            if(scene.getAnyIntersection(shadowRay)) {
                return c;
            }
            else {
                c = intensity;
                //Vector3 a = scene.getAbsorption();
                //a.exponentiate((shadowRay.end - shadowRay.start) * -attenuationFactor);
                //a.clamp(0, 1);
                //c.scale(a);
                return c;
            }
        }
    }
}