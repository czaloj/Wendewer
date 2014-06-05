using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;

namespace Wdw.RT {
    public class RTImage {
        public int width;
        public int height;
        protected Vector3[] data;

        public RTImage() {

        }
        public RTImage(int inW, int inH) {
            setSize(inW, inH);
        }
        public RTImage(RTImage oldImage) {
            setSize(oldImage.getWidth(), oldImage.getHeight());
            oldImage.data.CopyTo(data, 0);
        }

        public void clear() {
            Array.Clear(data, 0, data.Length);
        }

        public int getWidth() {

            return width;
        }
        public int getHeight() {

            return height;
        }

        public void setSize(int newWidth, int newHeight) {

            width = newWidth;
            height = newHeight;
            data = new Vector3[width * height];
        }

        public void getPixelColor(out Vector3 outPixel, int inX, int inY) {
            outPixel = data[inY * width + inX];
        }

        public void setPixelColor(Vector3 inPixel, int inX, int inY) {
            Vector3 pixelColor = inPixel.gammaCorrect(2.2).clamp(0, 1);
            setPixelRGB(pixelColor.X, pixelColor.Y, pixelColor.Z, inX, inY);
        }
        public void setPixelRGB(float inR, float inG, float inB, int inX, int inY) {
            int i = inY * width + inX;
            data[i].X = inR;
            data[i].Y = inG;
            data[i].Z = inB;
        }

        public void write(string fileName) {
            byte[] bd = new byte[width * height * 4];
            for(int i = 0, ci = 0; i < data.Length; i++) {
                Vector3 c = (data[i] * 255).clamp(0, 255);
                bd[ci++] = (byte)(c.Z);
                bd[ci++] = (byte)(c.Y);
                bd[ci++] = (byte)(c.X);
                bd[ci++] = 0xff;
            }

            Bitmap bmp = new Bitmap(width, height);
            var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(bd, 0, bmpData.Scan0, bd.Length);
            bmp.UnlockBits(bmpData);
            using(var s = File.Create(fileName)) {
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}