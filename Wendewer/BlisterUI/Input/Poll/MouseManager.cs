using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace BlisterUI.Input {

    public sealed class MouseManager {
        private static readonly MouseButton[] allButtons;
        static MouseManager() {
            allButtons = new MouseButton[]
            {
                MouseButton.Left,
                MouseButton.Right,
                MouseButton.Middle,
                MouseButton.Button1,
                MouseButton.Button2
            };
        }

        public bool IsBound {
            get;
            private set;
        }
        private Point mouseBinding;

        private MouseState cMS, pMS;
        public MouseState Current {
            get { return cMS; }
        }
        public MouseState Previous {
            get { return pMS; }
        }

        // Mouse Displacements
        public int XDisplacement {
            get { return cMS.X - pMS.X; }
        }
        public int YDisplacement {
            get { return cMS.Y - pMS.Y; }
        }
        public Point Displacement {
            get { return new Point(XDisplacement, YDisplacement); }
        }
        public int ScrollDisplacement {
            get { return cMS.ScrollWheelValue - pMS.ScrollWheelValue; }
        }

        public IEnumerable<MouseButton> AllButtonsJustPressed {
            get {
                foreach(MouseButton b in allButtons) {
                    if(IsButtonJustPressed(b))
                        yield return b;

                }
            }
        }
        public IEnumerable<MouseButton> AllButtonsJustReleased {
            get {
                foreach(MouseButton b in allButtons) {
                    if(IsButtonJustReleased(b))
                        yield return b;

                }
            }
        }

        public MouseManager() {
            Refresh();
            pMS = cMS;
            IsBound = false;
            mouseBinding.X = 400;
            mouseBinding.Y = 300;
        }

        public bool IsButtonJustPressed(MouseButton button) {
            switch(button) {
                case MouseButton.Left:
                    return cMS.LeftButton == ButtonState.Pressed && pMS.LeftButton == ButtonState.Released;
                case MouseButton.Middle:
                    return cMS.MiddleButton == ButtonState.Pressed && pMS.MiddleButton == ButtonState.Released;
                case MouseButton.Right:
                    return cMS.RightButton == ButtonState.Pressed && pMS.RightButton == ButtonState.Released;
                case MouseButton.Button1:
                    return cMS.XButton1 == ButtonState.Pressed && pMS.XButton1 == ButtonState.Released;
                case MouseButton.Button2:
                    return cMS.XButton2 == ButtonState.Pressed && pMS.XButton2 == ButtonState.Released;
                default:
                    return false;
            }
        }
        public bool IsButtonJustReleased(MouseButton button) {
            switch(button) {
                case MouseButton.Left:
                    return cMS.LeftButton == ButtonState.Released && pMS.LeftButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return cMS.MiddleButton == ButtonState.Released && pMS.MiddleButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return cMS.RightButton == ButtonState.Released && pMS.RightButton == ButtonState.Pressed;
                case MouseButton.Button1:
                    return cMS.XButton1 == ButtonState.Released && pMS.XButton1 == ButtonState.Pressed;
                case MouseButton.Button2:
                    return cMS.XButton2 == ButtonState.Released && pMS.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public void Bind(Point binding) {
            Bind(binding.X, binding.Y);
        }
        public void Bind(int x, int y) {
            mouseBinding.X = x;
            mouseBinding.Y = y;
            IsBound = true;
            Mouse.SetPosition(mouseBinding.X, mouseBinding.Y);
            pMS = Mouse.GetState();
        }
        public void Unbind() {
            IsBound = false;
        }

        public void Refresh() {
            if(IsBound) {
                cMS = Mouse.GetState();
                Mouse.SetPosition(mouseBinding.X, mouseBinding.Y);
            }
            else {
                pMS = cMS;
                cMS = Mouse.GetState();
            }
        }
    }
}