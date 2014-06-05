using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using OpenTK;

namespace Wdw.RT {
    class Pixel {
        public static int Aggregator(int i, Pixel p) {
            return i + (p.done ? 0 : 1);
        }

        public int x, y;
        public bool done;
        public Pixel(int x, int y) {
            this.x = x;
            this.y = y;
            done = false;
        }
    }
    class SuperSampling {
        public readonly int sampleWidth, totalSamples;
        public readonly float sInv, sInvD2, sInv2;

        public SuperSampling(int s) {
            sampleWidth = s;
            totalSamples = sampleWidth * sampleWidth;
            sInv = 1.0f / sampleWidth;
            sInvD2 = 1.0f / (2f * sampleWidth);
            sInv2 = 1.0f / (sampleWidth * sampleWidth);
        }
    }
    class RayTask {
        public Pixel[] pixels;
        public RayTask(int c) {
            pixels = new Pixel[c];
        }
    }
    class RayWorker : ACThreadPool<RayTask> {
        private readonly RTCamera cam;
        private readonly RTImage image;
        private readonly Scene scene;
        private readonly SuperSampling sampling;

        public RayWorker(RTCamera c, RTImage i, Scene s, SuperSampling ss) {
            cam = c;
            image = i;
            scene = s;
            sampling = ss;
        }

        protected override void DoWork(RayTask t) {
            RTRay ray = new RTRay();
            Vector3 rayColor = new Vector3();
            Vector3 totalColor = Vector3.Zero;
            float s = 0;
            foreach(Pixel pixel in t.pixels) {
                int sx, sy;
                float tsx, tsy;
                totalColor = Vector3.Zero;
                s = 0;
                for(sx = 0, tsx = sampling.sInvD2; sx < sampling.sampleWidth; sx++, tsx += sampling.sInv) {
                    for(sy = 0, tsy = sampling.sInvD2; sy < sampling.sampleWidth; sy++, tsy += sampling.sInv) {
                        cam.getRay(ray, (pixel.x + tsx) / image.width, (pixel.y + tsy) / image.height);
                        ray.setAbsorption(scene.getAbsorption());
                        RayTracer.shadeRay(out rayColor, scene, ray, 0);
                        totalColor += rayColor;
                        s++;
                    }
                }
                totalColor /= s;
                image.setPixelColor(totalColor, pixel.x, pixel.y);
                pixel.done = true;
            }
        }
    }

    public static class TestFolderPath {
        public static string Directory;
    }

    public class RayTracer {
        public const bool IsMultithreaded = false;
        public const int Workers = 8;
        public const int ImageDivisions = Workers * 12;
        public const int MAX_DEPTH = 12;

        public List<String> getFileLists(String fn) {
            var files = new List<string>();
            if(File.Exists(fn)) {
                files.Add(fn);
                TestFolderPath.Directory = new FileInfo(fn).Directory.FullName;
            }
            else if(Directory.Exists(fn)) {
                files.AddRange(Directory.GetFiles(fn));
                TestFolderPath.Directory = fn;
            }
            return files;
        }
        public void run(String directory, String[] args) {
            for(int ctr = 0; ctr < args.Length; ctr++) {
                List<String> fileLists = getFileLists(directory + "/" + args[ctr]);
                foreach(string inputFilename in fileLists) {
                    String outputFilename = inputFilename + ".png";

                    // Parse the input file
                    Scene scene = (Scene)ZXParser.ParseFile(inputFilename, typeof(Scene));

                    // Render the scene
                    renderImage(scene);

                    // Write the image out
                    scene.getImage().write(outputFilename);
                }
            }
        }

        public void renderImage(Scene scene) {
            // Propagate transformation matrix through the tree hierarchy
            scene.setTransform();

            // Create the acceleration structure.
            List<Surface> renderableSurfaces = new List<Surface>();
            List<Surface> surfaces = scene.getSurfaces();
            foreach(Surface s in surfaces) {
                s.appendRenderableSurfaces(renderableSurfaces);
            }
            Surface[] surfaceArray = renderableSurfaces.ToArray();
            scene.getAccelStruct().build(surfaceArray);

            // Do some basic setup
            RTImage image = scene.getImage();
            RTCamera cam = scene.getCamera();
            cam.initView();

            // Timing counters
            long startTime = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            SuperSampling sampling = new SuperSampling(scene.getSamples());

            // Do Certain Render Type
            if(IsMultithreaded)
                rayTraceImageMT(scene, cam, image, sampling);
            else
                rayTraceImageST(scene, cam, image, sampling);
            scene.outputImage = image;
            // Output time
            long totalTime = ((long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds - startTime);
            Console.WriteLine("Done.  Total rendering time: " + (totalTime / 1000.0) + " seconds");
        }
        private void rayTraceImageST(Scene scene, RTCamera cam, RTImage image, SuperSampling sampling) {
            int total = image.height * image.width;
            int counter = 0;
            int lastShownPercent = 0;
            RTRay ray = new RTRay();
            Vector3 rayColor = Vector3.Zero;
            Vector3 totalColor;
            for(int y = 0; y < image.height; y++) {
                for(int x = 0; x < image.width; x++) {

                    int sx, sy;
                    float tsx, tsy;
                    float s = 0;
                    totalColor = Vector3.Zero;
                    for(sx = 0, tsx = sampling.sInvD2; sx < sampling.sampleWidth; sx++, tsx += sampling.sInv) {
                        for(sy = 0, tsy = sampling.sInvD2; sy < sampling.sampleWidth; sy++, tsy += sampling.sInv) {
                            cam.getRay(ray, (x + tsx) / image.width, (y + tsy) / image.height);
                            ray.setAbsorption(scene.getAbsorption());
                            shadeRay(out rayColor, scene, ray, 0);
                            totalColor += rayColor;
                            s++;
                        }
                    }
                    totalColor /= s;
                    image.setPixelColor(totalColor, x, y);

                    counter++;
                    if((int)(100.0 * counter / total) != lastShownPercent) {
                        lastShownPercent = (int)(100.0 * counter / total);
                        Console.WriteLine(lastShownPercent + "%");
                    }
                }
            }
        }
        private void rayTraceImageMT(Scene scene, RTCamera cam, RTImage image, SuperSampling sampling) {
            // Add All The RTRay Tasks
            int total = image.height * image.width;
            int pixelPerTask = total / ImageDivisions;
            int uppt = pixelPerTask * (ImageDivisions - 1);
            int lppt = total - uppt;
            int pti = 0;
            RayWorker rtPool = new RayWorker(cam, image, scene, sampling);
            RayTask rt;
            while(pti < uppt) {
                rt = new RayTask(pixelPerTask);
                for(int i = 0; i < pixelPerTask; i++, pti++) {
                    rt.pixels[i] = new Pixel(pti % image.width, pti / image.width);
                }
                rtPool.AddWork(rt);
            }
            rt = new RayTask(lppt);
            for(int i = 0; i < lppt; i++, pti++) {
                rt.pixels[i] = new Pixel(pti % image.width, pti / image.width);
            }
            rtPool.AddWork(rt);

            // Create And Start The Workers
            rtPool.Start(Workers, ThreadPriority.Highest);
            int counter = rt.pixels.Aggregate(0, Pixel.Aggregator);
            while(counter < rt.pixels.Length) {
                Thread.Sleep(50);
                int percent = (int)((100.0 * counter) / rt.pixels.Length);
                Console.WriteLine(percent + "%");
                counter = rt.pixels.Aggregate(0, Pixel.Aggregator);
            }
            rtPool.Dispose();
        }

        public static void shadeRay(out Vector3 outColor, Scene scene, RTRay ray, int depth) {
            // Reset the output color
            outColor = Vector3.Zero;

            // Return On Reaching Max Recursion Depth
            if(depth > MAX_DEPTH) return;

            IntersectionRecord intersectionRecord = new IntersectionRecord();

            if(!scene.getFirstIntersection(intersectionRecord, ray)) {
                // TODO Render The Background From Cube Here
                outColor = scene.backColor;
                return;
            }
            Shader shader = intersectionRecord.surface.getShader();
            shader.shade(out outColor, scene, ray, intersectionRecord, depth);
        }
    }
}