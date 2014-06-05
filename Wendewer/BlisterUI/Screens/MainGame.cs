using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using BlisterUI.Input;
using OpenTK.Input;

namespace BlisterUI {
    public struct GameTime {
        public double Total;
        public double Elapsed;
    }

    public abstract class MainGame : GameWindow {
        // List Of Screens And The Current Screen
        protected ScreenList screenList;
        protected IGameScreen screen;
        private GameTime lastTime, curTime;

        public event Action<int, int> OnWindowResize;

        public MainGame()
            : base() {
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            FullInitialize();
            FullLoad();
            BuildScreenList();
            screen = screenList.Current;
        }
        protected override void OnUnload(EventArgs e) {
            base.OnUnload(e);

        }

        public override void Exit() {
            FullQuit(lastTime);
            base.Exit();
        }
        protected virtual void FullQuit(GameTime gameTime) {
            if(screen != null) {
                screen.OnExit(gameTime);
            }
            screenList.Destroy(gameTime);
        }

        protected abstract void BuildScreenList();
        protected abstract void FullInitialize();
        protected abstract void FullLoad();

        protected override void OnUpdateFrame(FrameEventArgs e) {
            // Keep Track Of Time
            curTime.Elapsed = e.Time;
            curTime.Total += e.Time;
            lastTime = curTime;

            if(screen != null) {
                switch(screen.State) {
                    case ScreenState.Running:
                        screen.Update(curTime);
                        break;
                    case ScreenState.ChangeNext:
                        screen.OnExit(curTime);
                        screen = screenList.Next;
                        if(screen != null) {
                            screen.SetRunning();
                            screen.OnEntry(curTime);
                        }
                        break;
                    case ScreenState.ChangePrevious:
                        screen.OnExit(curTime);
                        screen = screenList.Previous;
                        if(screen != null) {
                            screen.SetRunning();
                            screen.OnEntry(curTime);
                        }
                        break;
                    case ScreenState.ExitApplication:
                        Exit();
                        return;
                }
            }
            else {
                Exit();
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e) {
            if(screen != null && screen.State == ScreenState.Running) {
                screen.Draw(curTime);
            }
            Context.SwapBuffers();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            if(OnWindowResize != null)
                OnWindowResize(Width, Height);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e) {
            base.OnMouseMove(e);
            MouseEventDispatcher.EventInput_MouseMotion(this, e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            MouseEventDispatcher.EventInput_MouseButton(this, e);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            MouseEventDispatcher.EventInput_MouseButton(this, e);
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e) {
            base.OnKeyDown(e);
            KeyboardEventDispatcher.EventInput_KeyDown(this, e);
        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e) {
            base.OnKeyUp(e);
            KeyboardEventDispatcher.EventInput_KeyUp(this, e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress(e);
            KeyboardEventDispatcher.EventInput_CharEntered(this, e);
        }
    }
}