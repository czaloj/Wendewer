using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class Scene {
        /** The camera for this scene. */
        public RTCamera camera;
        public void setCamera(RTCamera camera) {
            this.camera = camera;
        }
        public RTCamera getCamera() {
            return this.camera;
        }

        public Vector3 backColor = Vector3.Zero;
        public void setBackColor(Vector3 color) {
            this.backColor = color;
        }
        public Vector3 getBackColor() {
            return this.backColor;
        }

        /** The list of lights for the scene. */
        public List<Light> lights = new List<Light>();
        public void addLight(Light toAdd) {
            lights.Add(toAdd);
        }
        public List<Light> getLights() {
            return this.lights;
        }

        /** The list of surfaces for the scene. */
        public List<Surface> surfaces = new List<Surface>();
        public void addSurface(Surface toAdd) {
            surfaces.Add(toAdd);
        }
        public List<Surface> getSurfaces() {
            return this.surfaces;
        }

        /** The list of shaders in the scene. */
        public List<Shader> shaders = new List<Shader>();
        public void addShader(Shader toAdd) {
            shaders.Add(toAdd);
        }

        /** RTImage to be produced by the renderer **/
        public RTImage outputImage;
        public RTImage getImage() { return this.outputImage; }
        public void setImage(RTImage outputImage) { this.outputImage = outputImage; }

        /** samples^2 is the number of samples per pixel **/
        public int samples;
        public int getSamples() { return this.samples == 0 ? 1 : this.samples; }
        public void setSamples(int n) { samples = (int)Math.Round(Math.Sqrt(n)); }

        /** The absorption coefficient of the scene */
        public Vector3 absorption = Vector3.Zero;
        public void setAbsorption(Vector3 value) { absorption = value; }
        public Vector3 getAbsorption() { return absorption; }

        /** The acceleration structure **/
        public IAccelStruct accelStruct = new NaiveAccelStruct();
        public void setAccelStruct(IAccelStruct accelStruct) { this.accelStruct = accelStruct; }
        public IAccelStruct getAccelStruct() { return accelStruct; }

        public void setTransform() {
            Matrix4 id = Matrix4.Identity;
            foreach(Surface s in surfaces) {
                s.setTransformation(id, id, id);
            }
        }

        public bool getFirstIntersection(IntersectionRecord outRecord, RTRay ray) {
            return accelStruct.intersect(outRecord, ray, false);
        }
        public bool getAnyIntersection(RTRay ray) {
            ray.end -= RTRay.SHADOWEPSILON;
            return accelStruct.intersect(new IntersectionRecord(), ray, true);
        }
    }
}