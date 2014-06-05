using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class Lambertian : Shader {
        /** The color of the surface. */
        protected Vector3 diffuseColor = Vector3.One;
        public void setDiffuseColor(Vector3 inDiffuseColor) { diffuseColor = inDiffuseColor; }

        public Lambertian() { }

        public override string ToString() {
            return "Lambertian: " + diffuseColor;
        }

        public override void shade(out Vector3 outIntensity, Scene scene, RTRay ray, IntersectionRecord record, int depth) {
            Vector3 incoming = new Vector3();
            Vector3 color = new Vector3();
            RTRay shadowRay = new RTRay();

            // Assume we are not inside a surface
            shadowRay.setAbsorption(scene.getAbsorption());

            outIntensity = Vector3.Zero;
            foreach(Light light in scene.getLights()) {
                Vector3 lColor = light.getIntensityAt(scene, record);
                double li = lColor.getGrey();
                if(li > 0) {
                    incoming = light.position - record.location;
                    incoming.Normalize();
                    float dotProd = Vector3.Dot(record.normal, incoming);
                    if(dotProd <= 0)
                        continue;
                    else {
                        color = (diffuseColor * dotProd) * lColor;
                        outIntensity += color;
                    }
                }
            }

            ray.attenuate(ref outIntensity, record.location);
        }
    }
}