using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public static class GLError {
        public static StreamWriter ErrorLog {
            get;
            set;
        }

        [Conditional("DEBUG")]
        public static void Get(string desc) {
            ErrorCode error;
            while((error = GL.GetError()) != ErrorCode.NoError) {
                Write("ERROR - " + desc + " - " + error);
            }
        }
        [Conditional("DEBUG")]
        public static void Write(string err) {
            if(ErrorLog != null)
                ErrorLog.WriteLine(err);
        }

        public static void Close() {
            if(ErrorLog == null) return;
            ErrorLog.Flush();
            ErrorLog.Dispose();
            ErrorLog = null;
            return;
        }
    }
}