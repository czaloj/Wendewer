using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace BlisterUI {
    public class FalseFirstScreen : GameScreen {
        protected int nS;
        protected bool doNext;
        public override int Next {
            get {
                if(doNext) { return Index; }
                doNext = true;
                return nS;
            }
            protected set { nS = value; }
        }
        public override int Previous { get; protected set; }

        public FalseFirstScreen(int nextScreen) {
            doNext = false;
            Next = nextScreen;
            Previous = ScreenList.NO_SCREEN;
        }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
        }
        public override void OnExit(GameTime gameTime) {
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color4.Black);
            State = ScreenState.ChangeNext;
        }
    }
}
