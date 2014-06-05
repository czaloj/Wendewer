using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public static class GLUtil {
        public static int SizeOf(VertexAttribPointerType a) {
            switch(a) {
                case VertexAttribPointerType.UnsignedByte:
                case VertexAttribPointerType.Byte:
                    return 1;
                case VertexAttribPointerType.UnsignedShort:
                case VertexAttribPointerType.Short:
                case VertexAttribPointerType.HalfFloat:
                    return 2;
                case VertexAttribPointerType.UnsignedInt:
                case VertexAttribPointerType.Int:
                case VertexAttribPointerType.Float:
                    return 4;
                case VertexAttribPointerType.Double:
                    return 4;
            }
            return 0;
        }
    }
}
