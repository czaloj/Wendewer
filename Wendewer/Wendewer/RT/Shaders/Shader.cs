using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public abstract class Shader {
        public static readonly Shader DEFAULT_SHADER = new Lambertian();

        public abstract void shade(out Vector3 outIntensity, Scene scene, RTRay ray, IntersectionRecord record, int depth);

        protected bool isShadowed(Scene scene, Light light, IntersectionRecord record, RTRay shadowRay) {
            // Setup the shadow ray to start at surface and end at light
            shadowRay.origin = record.location;
            shadowRay.direction = light.position - record.location;

            float end = shadowRay.direction.Length;
            shadowRay.direction /= end;

            // Set the ray to end at the light
            shadowRay.makeOffsetSegment(end);

            return scene.getAnyIntersection(shadowRay);
        }
    }
}
