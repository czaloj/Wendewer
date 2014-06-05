using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public class GLBuffer : IDisposable {
        // Personal
        public int ID {
            get;
            private set;
        }
        public bool IsCreated {
            get { return ID != 0; }
        }
        public bool IsBound {
            get;
            private set;
        }

        // Buffer Type And Usage
        public BufferTarget Target {
            get;
            private set;
        }
        public BufferUsageHint UsageType {
            get;
            private set;
        }

        // Element Information
        public VertexAttribPointerType ComponentFormat {
            get;
            private set;
        }
        public int ComponentCount {
            get;
            private set;
        }
        public int ElementByteSize {
            get { return ComponentCount * GLUtil.SizeOf(ComponentFormat); }
        }

        // Byte Size Capacity
        public int BufCapacity {
            get;
            private set;
        }

        // Element Count
        public int CurElements {
            get;
            private set;
        }

        public GLBuffer(BufferTarget target, BufferUsageHint usage, bool init = false) {
            // Default Parameters
            ID = 0;

            Target = target;
            UsageType = usage;

            if(init) Init();
        }
        public void Dispose() {
            if(!IsCreated) return;
            GL.DeleteBuffer(ID);
            ID = 0;
        }

        public GLBuffer Init() {
            if(IsCreated) return this;
            ID = GL.GenBuffer();
            return this;
        }

        public GLBuffer SetElementFormat(VertexAttribPointerType format, int count) {
            CurElements *= ElementByteSize;
            ComponentFormat = format;
            ComponentCount = count;
            CurElements /= ElementByteSize;
            return this;
        }
        public GLBuffer SetAsIndexInt() {
            Target = BufferTarget.ElementArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.UnsignedInt, 1);
        }
        public GLBuffer SetAsIndexShort() {
            Target = BufferTarget.ElementArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.UnsignedShort, 1);
        }
        public GLBuffer SetAsVertexFloat() {
            Target = BufferTarget.ArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.Float, 1);
        }
        public GLBuffer SetAsVertexVec2() {
            Target = BufferTarget.ArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.Float, 2);
        }
        public GLBuffer SetAsVertexVec3() {
            Target = BufferTarget.ArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.Float, 3);
        }
        public GLBuffer SetAsVertexVec4() {
            Target = BufferTarget.ArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.Float, 4);
        }
        public GLBuffer SetAsVertex(int vSize) {
            Target = BufferTarget.ArrayBuffer;
            return SetElementFormat(VertexAttribPointerType.UnsignedByte, vSize);
        }

        public void Bind() {
            if(IsCreated && !IsBound) {
                IsBound = true;
                GL.BindBuffer(Target, ID);
                GLError.Get("Buffer Bind");
            }
        }
        public void Unbind() {
            if(IsCreated && IsBound) {
                IsBound = false;
                GL.BindBuffer(Target, 0);
            }
        }
        public void UseAsAttrib(int loc, int offset = 0, int instDiv = 0) {
            Bind();
            GL.EnableVertexAttribArray(loc);
            GLError.Get("Enable VAA");
            GL.VertexAttribPointer(loc, ComponentCount, ComponentFormat, false, ElementByteSize, offset * ElementByteSize);
            if(instDiv > 0)
                GL.VertexAttribDivisor(loc, instDiv);
            GLError.Get("VAP");
            Unbind();
        }
        public void UseAsAttrib(ShaderInterface si, int offset = 0) {
            Bind();
            // Calculate Stride And Bytes Of Offset
            int ebs = ElementByteSize;
            offset *= ebs;
            foreach(var bind in si.Binds) {
                if(bind.Location < 0) continue;
                GL.EnableVertexAttribArray(bind.Location);
                GLError.Get("Enable VAA");
                GL.VertexAttribPointer(bind.Location, bind.CompCount, bind.CompType, false, ebs, offset + bind.Offset);
                if(bind.InstanceDivisor > 0)
                    GL.VertexAttribDivisor(bind.Location, bind.InstanceDivisor);
                GLError.Get("VAP");
            }
            Unbind();
        }

        public void SetSizeInBytes(int bytes) {
            CurElements = 0;
            BufCapacity = bytes;
            Bind();
            GL.BufferData(Target, new IntPtr(BufCapacity), IntPtr.Zero, UsageType);
            Unbind();
        }
        public void SetSizeInElements(int elements) {
            SetSizeInBytes(elements * ElementByteSize);
        }

        public void CheckResizeInBytes(int bytes) {
            if(bytes <= BufCapacity / 4 || bytes > BufCapacity) {
                // Resize To Double The Desired Size
                SetSizeInBytes(bytes * 2);
            }
        }
        public void CheckResizeInElements(int elements) {
            CheckResizeInBytes(elements * ElementByteSize);
        }

        public void SetSubData(int off, IntPtr data, int len, int bytesPerDC) {
            Bind();
            GL.BufferSubData(Target, new IntPtr(off * bytesPerDC), new IntPtr(len * bytesPerDC), data);
            Unbind();
        }
        public void SetSubData(int off, byte[] data) {
            unsafe {
                fixed(byte* ptr = data)
                    SetSubData(off, new IntPtr(ptr), data.Length, sizeof(byte));
            }
        }
        public void SetSubData(int off, short[] data) {
            unsafe {
                fixed(short* ptr = data)
                    SetSubData(off, new IntPtr(ptr), data.Length, sizeof(short));
            }
        }
        public void SetSubData(int off, int[] data) {
            unsafe {
                fixed(int* ptr = data)
                    SetSubData(off, new IntPtr(ptr), data.Length, sizeof(int));
            }
        }
        public void SetSubData(int off, float[] data) {
            unsafe {
                fixed(float* ptr = data)
                    SetSubData(off, new IntPtr(ptr), data.Length, sizeof(float));
            }
        }
        public void SetSubData(int off, double[] data) {
            unsafe {
                fixed(double* ptr = data)
                    SetSubData(off, new IntPtr(ptr), data.Length, sizeof(double));
            }
        }
        public void SetSubData<T>(int off, T[] data, int tSize) where T : struct {
            SetSubData(off, Marshal.UnsafeAddrOfPinnedArrayElement(data, 0), data.Length, tSize);
        }
        public void SetSubData<T>(int off, T[] data) where T : struct {
            SetSubData(off, data, Marshal.SizeOf(typeof(T)));
        }

        public void SetData(IntPtr data, int len, int bytesPerDC) {
            Bind();
            int bytes = len * bytesPerDC;
            GL.BufferSubData(Target, IntPtr.Zero, new IntPtr(bytes), data);
            CurElements = bytes / ElementByteSize;
            Unbind();
        }
        public void SetData(byte[] data) {
            unsafe {
                fixed(byte* ptr = data)
                    SetData(new IntPtr(ptr), data.Length, sizeof(byte));
            }
        }
        public void SetData(short[] data) {
            unsafe {
                fixed(short* ptr = data)
                    SetData(new IntPtr(ptr), data.Length, sizeof(short));
            }
        }
        public void SetData(int[] data) {
            unsafe {
                fixed(int* ptr = data)
                    SetData(new IntPtr(ptr), data.Length, sizeof(int));
            }
        }
        public void SetData(float[] data) {
            unsafe {
                fixed(float* ptr = data)
                    SetData(new IntPtr(ptr), data.Length, sizeof(float));
            }
        }
        public void SetData(double[] data) {
            unsafe {
                fixed(double* ptr = data)
                    SetData(new IntPtr(ptr), data.Length, sizeof(double));
            }
        }
        public void SetData<T>(T[] data, int tSize) where T : struct {
            SetData(Marshal.UnsafeAddrOfPinnedArrayElement(data, 0), data.Length, tSize);
        }
        public void SetData<T>(T[] data) where T : struct {
            SetData(data, Marshal.SizeOf(typeof(T)));
        }

        public void SmartSetData(IntPtr data, int len, int bytesPerDC) {
            CheckResizeInBytes(len * bytesPerDC);
            SetData(data, len, bytesPerDC);
        }
        public void SmartSetData(byte[] data) {
            unsafe {
                fixed(byte* ptr = data)
                    SmartSetData(new IntPtr(ptr), data.Length, sizeof(byte));
            }
        }
        public void SmartSetData(short[] data) {
            unsafe {
                fixed(short* ptr = data)
                    SmartSetData(new IntPtr(ptr), data.Length, sizeof(short));
            }
        }
        public void SmartSetData(int[] data) {
            unsafe {
                fixed(int* ptr = data)
                    SmartSetData(new IntPtr(ptr), data.Length, sizeof(int));
            }
        }
        public void SmartSetData(float[] data) {
            unsafe {
                fixed(float* ptr = data)
                    SmartSetData(new IntPtr(ptr), data.Length, sizeof(float));
            }
        }
        public void SmartSetData(double[] data) {
            unsafe {
                fixed(double* ptr = data)
                    SmartSetData(new IntPtr(ptr), data.Length, sizeof(double));
            }
        }
        public void SmartSetData<T>(T[] data, int tSize) where T : struct {
            SmartSetData(Marshal.UnsafeAddrOfPinnedArrayElement(data, 0), data.Length, tSize);
        }
        public void SmartSetData<T>(T[] data) where T : struct {
            SmartSetData(data, Marshal.SizeOf(typeof(T)));
        }

        public GLBuffer InitAsVertex(float[] data, int vecDim) {
            Init();
            SetElementFormat(VertexAttribPointerType.Float, vecDim);
            Target = BufferTarget.ArrayBuffer;
            SmartSetData(data);
            return this;
        }
        public GLBuffer InitAsIndex(int[] data) {
            Init();
            SetAsIndexInt();
            SmartSetData(data);
            return this;
        }
        public GLBuffer InitAsIndex(short[] data) {
            Init();
            SetAsIndexShort();
            SmartSetData(data);
            return this;
        }
    }
}