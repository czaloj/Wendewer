using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTK.Input {
    public static class MouseEventDispatcher {
        public static event EventHandler<MouseButtonEventArgs> OnMouseRelease;
        public static event EventHandler<MouseButtonEventArgs> OnMousePress;
        public static event EventHandler<MouseWheelEventArgs> OnMouseScroll;
        public static event EventHandler<MouseMoveEventArgs> OnMouseMotion;

        public static void EventInput_MouseMotion(object sender, MouseMoveEventArgs e) {
            if(OnMouseMotion != null)
                OnMouseMotion(sender, e);
        }
        public static void EventInput_MouseButton(object sender, MouseButtonEventArgs e) {
            if(e.IsPressed) {
                if(OnMousePress != null)
                    OnMousePress(sender, e);
            }
            else {
                if(OnMouseRelease != null)
                    OnMouseRelease(sender, e);
            }
        }
        public static void EventInput_MouseWheel(object sender, MouseWheelEventArgs e) {
            if(OnMouseScroll != null)
                OnMouseScroll(sender, e);
        }
    }
}