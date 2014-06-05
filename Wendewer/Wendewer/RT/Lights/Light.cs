using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class Light {
        /** Where the light is located in space. */
        public Vector3 position = new Vector3();
        public void setPosition(Vector3 position) {
            this.position = position;
        }

        /** How bright the light is. */
        public Vector3 intensity = new Vector3(1, 1, 1);
        public void setIntensity(Vector3 intensity) {
            this.intensity = intensity;
        }

        public double attenuationFactor = 1;
        public void setAttenuation(double d) {
            attenuationFactor = d;
        }

        public Light() { }

        public virtual Vector3 getIntensityAt(Scene scene, IntersectionRecord record) {
            RTRay shadowRay = new RTRay(position, Vector3.Zero);
            shadowRay.direction = record.location - position;
            shadowRay.end = shadowRay.direction.Length - RTRay.SHADOWEPSILON;
            shadowRay.direction.Normalize();
            shadowRay.start = RTRay.SHADOWEPSILON;
            shadowRay.setAbsorption(scene.getAbsorption());
            Vector3 c = Vector3.Zero;
            if(scene.getAnyIntersection(shadowRay)) { return c; }
            else {
                return intensity * scene.getAbsorption().exponentiate((shadowRay.end - shadowRay.start) * -attenuationFactor).clamp(0, 1);
            }
        }

        public override string ToString() {
            return "light: " + position + " " + intensity;
        }
    }
}