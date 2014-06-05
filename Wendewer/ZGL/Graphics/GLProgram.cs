using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    #region Shader Semantics
    public enum Semantic : ushort {
        Index0 = 0x0000,
        Index1 = 0x0001,
        Index2 = 0x0002,
        Index3 = 0x0003,
        Index4 = 0x0004,
        Index5 = 0x0005,
        Index6 = 0x0006,
        Index7 = 0x0007,
        Index8 = 0x0008,
        Index9 = 0x0009,
        Index10 = 0x000a,
        Index11 = 0x000b,
        Index12 = 0x000c,
        Index13 = 0x000d,
        Index14 = 0x000e,
        Index15 = 0x000f,

        Position = 0x0000,
        Normal = 0x0010,
        Tangent = 0x0020,
        Binormal = 0x0030,
        Color = 0x0040,
        TexCoord = 0x0050,
        Custom = 0x0060,
        None = 0xffff
    }
    public struct ArrayBind {
        public int Location;
        public Semantic Semantic;
        public VertexAttribPointerType CompType;
        public int CompCount;
        public int Offset;
        public int InstanceDivisor;

        public ArrayBind(Semantic sem, VertexAttribPointerType ct, int cc, int o, int instDiv = 0) {
            Location = 0;
            Semantic = sem;
            CompType = ct;
            CompCount = cc;
            Offset = o;
            InstanceDivisor = instDiv;
        }
    }
    public class ShaderInterface {
        public readonly ArrayBind[] Binds;

        public ShaderInterface(ArrayBind[] binds) {
            Binds = new ArrayBind[binds.Length];
            binds.CopyTo(Binds, 0);
        }

        public int Build(Dictionary<Semantic, int> dSemBinds) {
            int bound = 0;
            for(int i = 0; i < Binds.Length; i++) {
                if(dSemBinds.TryGetValue(Binds[i].Semantic, out Binds[i].Location))
                    bound++;
                else
                    Binds[i].Location = -1;
            }
            return bound;
        }
    }
    #endregion

    public class GLProgram {
        private const string NON_ALLOWABLE_PREFIX = "gl_";
        private static readonly Regex RGX_SEMANTIC = new Regex(@"(\w+)\s*;\s*//\s*sem\s*\x28\s*(\w+)\s*(\d+)\s*\x29\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline
            );

        public int ID {
            get;
            private set;
        }
        private int idVS, idFS;

        public bool IsCreated {
            get { return ID != 0; }
        }
        public bool IsInUse {
            get;
            private set;
        }

        public bool IsLinked {
            get;
            private set;
        }

        public Dictionary<string, int> Uniforms {
            get;
            private set;
        }
        public Dictionary<string, int> Attributes {
            get;
            private set;
        }
        public Dictionary<Semantic, int> SemanticLinks {
            get;
            private set;
        }
        private Dictionary<string, Semantic> foundSemantics;

        public GLProgram(bool init = false) {
            ID = 0;
            idFS = 0;
            idVS = 0;
            if(init) Init();
            IsInUse = false;
            IsLinked = false;

            Uniforms = new Dictionary<string, int>();
            Attributes = new Dictionary<string, int>();
            SemanticLinks = new Dictionary<Semantic, int>();
        }
        public void Dispose() {
            if(!IsCreated) return;

            GL.DeleteProgram(ID);
            ID = 0;
            if(idVS != 0) {
                GL.DeleteShader(idVS);
                idVS = 0;
            }
            if(idFS != 0) {
                GL.DeleteShader(idFS);
                idFS = 0;
            }
        }

        public void Init() {
            if(IsCreated) return;
            IsInUse = false;
            IsLinked = false;
            ID = GL.CreateProgram();
        }

        public bool AddShader(ShaderType st, string src) {
            switch(st) {
                case ShaderType.VertexShader:
                    if(idVS != 0) {
                        GLError.Write("Attempting To Add Another Vertex Shader To Program");
                        return false;
                    }
                    break;
                case ShaderType.FragmentShader:
                    if(idFS != 0) {
                        GLError.Write("Attempting To Add Another Fragment Shader To Program");
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            int idS = GL.CreateShader(st);
            GL.ShaderSource(idS, src);
            GLError.Get(st + " Source");
            GL.CompileShader(idS);
            GLError.Get(st + " Compile");

            // Check Status
            int status;
            GL.GetShader(idS, ShaderParameter.CompileStatus, out status);
            if(status != (int)All.True) {
                GL.DeleteShader(idS);
                return false;
            }

            GL.AttachShader(ID, idS);
            GLError.Get(st + " Attach");

            // If It's A Vertex Shader -> Get Semantics From Source
            if(st == ShaderType.VertexShader) {
                foundSemantics = new Dictionary<string, Semantic>();
                Match m = RGX_SEMANTIC.Match(src);
                while(m.Success) {
                    string attr = m.Groups[1].Value;
                    Semantic sem;
                    switch(m.Groups[2].Value.ToLower()) {
                        case "position": sem = Semantic.Position; break;
                        case "normal": sem = Semantic.Normal; break;
                        case "tangent": sem = Semantic.Tangent; break;
                        case "binormal": sem = Semantic.Binormal; break;
                        case "texcoord": sem = Semantic.TexCoord; break;
                        case "color": sem = Semantic.Color; break;
                        default: sem = Semantic.None; break;
                    }
                    Semantic index = (Semantic)int.Parse(m.Groups[3].Value);
                    sem |= index;
                    foundSemantics.Add(attr, sem);
                    m = m.NextMatch();
                }
            }

            switch(st) {
                case ShaderType.VertexShader: idVS = idS; break;
                case ShaderType.FragmentShader: idFS = idS; break;
            }
            return true;
        }
        public bool AddShaderFile(ShaderType st, string file) {
            if(!File.Exists(file))
                return false;

            string src = null;
            using(var fs = File.OpenRead(file)) {
                src = new StreamReader(fs).ReadToEnd();
            }

            return AddShader(st, src);
        }

        public void SetAttributes(Dictionary<string, int> attr) {
            foreach(var kvp in attr) {
                // Make Sure It Is A Good Binding
                string name = kvp.Key;
                if(name.StartsWith(NON_ALLOWABLE_PREFIX))
                    continue;

                // Check Location
                int loc = kvp.Value;
                if(loc < 0)
                    continue;

                // Place It In
                Attributes.Add(name, loc);
                GL.BindAttribLocation(ID, loc, name);
                GLError.Get("Program Attr Bind");
            }
        }
        public bool Link() {
            if(IsLinked) return false;

            GL.LinkProgram(ID);
            GLError.Get("Program Link");

            int status;
            GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out status);
            IsLinked = status == (int)All.True;
            return IsLinked;
        }
        public void InitAttributes() {
            // How Many Attributes Are In The Program
            int count = 0;

            // Necessary Info
            string name;
            int loc;
            int size;
            ActiveAttribType type;

            // Enumerate Through Uniforms
            GL.GetProgram(ID, GetProgramParameterName.ActiveAttributes, out count);
            for(int i = 0; i < count; i++) {
                // Get Uniform Info
                name = GL.GetActiveAttrib(ID, i, out size, out type);
                loc = GL.GetAttribLocation(ID, name);

                // Get Rid Of System Uniforms
                if(!name.StartsWith(NON_ALLOWABLE_PREFIX) && loc > -1)
                    Attributes.Add(name, loc);
            }

            if(foundSemantics != null) {
                GenerateSemanticBindings(foundSemantics);
                foundSemantics = null;
            }
        }
        public void InitUniforms() {
            // How Many Attributes Are In The Program
            int count = 0;

            // Necessary Info
            string name;
            int loc;
            int size;
            ActiveUniformType type;

            // Enumerate Through Uniforms
            GL.GetProgram(ID, GetProgramParameterName.ActiveUniforms, out count);
            for(int i = 0; i < count; i++) {
                // Get Uniform Info
                name = GL.GetActiveUniform(ID, i, out size, out type);
                loc = GL.GetUniformLocation(ID, name);

                // Get Rid Of System Uniforms
                if(!name.StartsWith(NON_ALLOWABLE_PREFIX) && loc > -1)
                    Uniforms.Add(name, loc);
            }
        }
        public void GenerateSemanticBindings(Dictionary<string, Semantic> dSems) {
            int i;
            foreach(var kvp in dSems) {
                if(Attributes.TryGetValue(kvp.Key, out i))
                    SemanticLinks.Add(kvp.Value, i);
            }
        }

        public void Use() {
            if(IsInUse) return;
            IsInUse = true;
            GL.UseProgram(ID);
        }
        public void Unuse() {
            if(!IsInUse) return;
            IsInUse = false;
            GL.UseProgram(0);
        }

        public GLProgram QuickCreate(string vsFile, string fsFile, Dictionary<string, int> attr = null) {
            Init();
            if(!AddShaderFile(ShaderType.VertexShader, vsFile)) return this;
            if(!AddShaderFile(ShaderType.FragmentShader, fsFile)) return this;

            if(attr != null)
                SetAttributes(attr);
            if(!Link())
                return this;

            InitAttributes();
            InitUniforms();
            return this;
        }
    }
}