using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace EGL {
    public static class GLState {
        public static void EnableTextures() {
            GL.Enable(EnableCap.Texture1D);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Texture3DExt);
            GL.Enable(EnableCap.TextureRectangle);
        }
        public static void EnableBlending() {
            GL.Enable(EnableCap.Blend);
        }
        public static void EnableAll() {
            EnableTextures();
            EnableBlending();
            DepthState.Default.Set();
            RasterizerState.CullCounterClockwise.Set();
            BlendState.Additive.Set();
        }
    }

    public class SamplerState {
        #region No Mip-Mapping
        public static readonly SamplerState LinearClamp = new SamplerState() {
            MinFilter = TextureMinFilter.Linear,
            MagFilter = TextureMagFilter.Linear,
            WrapS = TextureWrapMode.ClampToEdge,
            WrapT = TextureWrapMode.ClampToEdge,
            WrapR = TextureWrapMode.ClampToEdge
        };
        public static readonly SamplerState LinearWrap = new SamplerState() {
            MinFilter = TextureMinFilter.Linear,
            MagFilter = TextureMagFilter.Linear,
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.Repeat,
            WrapR = TextureWrapMode.Repeat
        };
        public static readonly SamplerState PointClamp = new SamplerState() {
            MinFilter = TextureMinFilter.Nearest,
            MagFilter = TextureMagFilter.Nearest,
            WrapS = TextureWrapMode.ClampToEdge,
            WrapT = TextureWrapMode.ClampToEdge,
            WrapR = TextureWrapMode.ClampToEdge
        };
        public static readonly SamplerState PointWrap = new SamplerState() {
            MinFilter = TextureMinFilter.Nearest,
            MagFilter = TextureMagFilter.Nearest,
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.Repeat,
            WrapR = TextureWrapMode.Repeat
        };
        #endregion
        #region Mip-Mapping
        public static readonly SamplerState LinearClampMM = new SamplerState() {
            MinFilter = TextureMinFilter.LinearMipmapLinear,
            MagFilter = TextureMagFilter.Linear,
            WrapS = TextureWrapMode.ClampToEdge,
            WrapT = TextureWrapMode.ClampToEdge,
            WrapR = TextureWrapMode.ClampToEdge
        };
        public static readonly SamplerState LinearWrapMM = new SamplerState() {
            MinFilter = TextureMinFilter.LinearMipmapLinear,
            MagFilter = TextureMagFilter.Linear,
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.Repeat,
            WrapR = TextureWrapMode.Repeat
        };
        public static readonly SamplerState PointClampMM = new SamplerState() {
            MinFilter = TextureMinFilter.NearestMipmapLinear,
            MagFilter = TextureMagFilter.Nearest,
            WrapS = TextureWrapMode.ClampToEdge,
            WrapT = TextureWrapMode.ClampToEdge,
            WrapR = TextureWrapMode.ClampToEdge
        };
        public static readonly SamplerState PointWrapMM = new SamplerState() {
            MinFilter = TextureMinFilter.NearestMipmapLinear,
            MagFilter = TextureMagFilter.Nearest,
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.Repeat,
            WrapR = TextureWrapMode.Repeat
        };
        #endregion

        public TextureMinFilter MinFilter;
        public TextureMagFilter MagFilter;
        public TextureWrapMode WrapS;
        public TextureWrapMode WrapT;
        public TextureWrapMode WrapR;

        public void Set(TextureTarget target) {
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)WrapT);
            GL.TexParameter(target, TextureParameterName.TextureWrapR, (int)WrapR);
        }
    }

    public class DepthState {
        public static readonly DepthState None = new DepthState() {
            Read = false,
            DepthFunc = DepthFunction.Always,
            Write = false
        };
        public static readonly DepthState DepthRead = new DepthState() {
            Read = true,
            DepthFunc = DepthFunction.Lequal,
            Write = false
        };
        public static readonly DepthState DepthWrite = new DepthState() {
            Read = false,
            DepthFunc = DepthFunction.Always,
            Write = true
        };
        public static readonly DepthState Default = new DepthState() {
            Read = true,
            DepthFunc = DepthFunction.Lequal,
            Write = true
        };

        public bool Read;
        public DepthFunction DepthFunc;
        public bool Write;

        public void Set() {
            if(Read || Write) {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(Write);
                GL.DepthFunc(DepthFunc);
            }
            else {
                GL.Disable(EnableCap.DepthTest);
            }
        }
    }

    public class RasterizerState {
        public static readonly RasterizerState CullNone = new RasterizerState() {
            UseCulling = false,
            CullMode = CullFaceMode.Back,
            FaceOrientation = FrontFaceDirection.Ccw
        };
        public static readonly RasterizerState CullClockwise = new RasterizerState() {
            UseCulling = true,
            CullMode = CullFaceMode.Back,
            FaceOrientation = FrontFaceDirection.Ccw
        };
        public static readonly RasterizerState CullCounterClockwise = new RasterizerState() {
            UseCulling = true,
            CullMode = CullFaceMode.Back,
            FaceOrientation = FrontFaceDirection.Cw
        };

        public bool UseCulling;
        public CullFaceMode CullMode;
        public FrontFaceDirection FaceOrientation;

        public void Set() {
            if(UseCulling) {
                GL.Enable(EnableCap.CullFace);
                GL.FrontFace(FaceOrientation);
                GL.CullFace(CullMode);
            }
            else {
                GL.Disable(EnableCap.CullFace);
            }
        }
    }

    public class BlendState {
        public static readonly BlendState Opaque = new BlendState() {
            BlendMode = BlendEquationMode.FuncAdd,
            BlendSrc = BlendingFactorSrc.One,
            BlendDest = BlendingFactorDest.Zero,
            BlendModeAlpha = BlendEquationMode.FuncAdd,
            BlendSrcAlpha = BlendingFactorSrc.One,
            BlendDestAlpha = BlendingFactorDest.Zero
        };
        public static readonly BlendState AlphaBlend = new BlendState() {
            BlendMode = BlendEquationMode.FuncAdd,
            BlendSrc = BlendingFactorSrc.SrcAlpha,
            BlendDest = BlendingFactorDest.OneMinusSrcAlpha,
            BlendModeAlpha = BlendEquationMode.FuncAdd,
            BlendSrcAlpha = BlendingFactorSrc.One,
            BlendDestAlpha = BlendingFactorDest.Zero
        };
        public static readonly BlendState PremultipliedAlphaBlend = new BlendState() {
            BlendMode = BlendEquationMode.FuncAdd,
            BlendSrc = BlendingFactorSrc.One,
            BlendDest = BlendingFactorDest.OneMinusSrcAlpha,
            BlendModeAlpha = BlendEquationMode.FuncAdd,
            BlendSrcAlpha = BlendingFactorSrc.One,
            BlendDestAlpha = BlendingFactorDest.Zero
        };
        public static readonly BlendState Additive = new BlendState() {
            BlendMode = BlendEquationMode.FuncAdd,
            BlendSrc = BlendingFactorSrc.SrcAlpha,
            BlendDest = BlendingFactorDest.One,
            BlendModeAlpha = BlendEquationMode.FuncAdd,
            BlendSrcAlpha = BlendingFactorSrc.One,
            BlendDestAlpha = BlendingFactorDest.Zero
        };
        public static readonly BlendState PremultipliedAdditive = new BlendState() {
            BlendMode = BlendEquationMode.FuncAdd,
            BlendSrc = BlendingFactorSrc.One,
            BlendDest = BlendingFactorDest.One,
            BlendModeAlpha = BlendEquationMode.FuncAdd,
            BlendSrcAlpha = BlendingFactorSrc.One,
            BlendDestAlpha = BlendingFactorDest.Zero
        };

        public BlendEquationMode BlendMode;
        public BlendingFactorSrc BlendSrc;
        public BlendingFactorDest BlendDest;
        public BlendEquationMode BlendModeAlpha;
        public BlendingFactorSrc BlendSrcAlpha;
        public BlendingFactorDest BlendDestAlpha;

        public void Set() {
            //GL.BlendEquation(BlendMode);
            //GL.BlendFunc(BlendSrc, BlendDest);
            GL.BlendEquationSeparate(BlendMode, BlendModeAlpha);
            GL.BlendFuncSeparate(BlendSrc, BlendDest, BlendSrcAlpha, BlendDestAlpha);
        }
    }
}