﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using EGL;
using OpenTK.Input;
using System.Drawing;

namespace BlisterUI.Widgets {
    public class ScrollBar : RectWidget {
        public RectButton ScrollButton {
            get;
            private set;
        }

        private float scrollRatio;
        public float ScrollRatio {
            get { return scrollRatio; }
            set {
                value = MathHelper.Clamp(value, 0f, 1f);
                if(scrollRatio != value) {
                    scrollRatio = value;
                    RefreshScroll();
                    if(OnScrollValueChanged != null)
                        OnScrollValueChanged(this, scrollRatio);
                }
            }
        }
        public event Action<ScrollBar, float> OnScrollValueChanged;

        public int ScrollAmount {
            get { return IsVertical ? Height - ScrollButton.Height : Width - ScrollButton.Width; }
        }

        private bool isVertical;
        public bool IsVertical {
            get { return isVertical; }
            set {
                if(isVertical != value) {
                    isVertical = value;
                    if(IsVertical) {
                        ScrollButton.AlignX = Alignment.MID;
                        ScrollButton.AlignY = Alignment.TOP;
                        ScrollButton.OffsetAlignX = Alignment.MID;
                        ScrollButton.OffsetAlignY = Alignment.TOP;
                    }
                    else {
                        ScrollButton.AlignX = Alignment.LEFT;
                        ScrollButton.AlignY = Alignment.MID;
                        ScrollButton.OffsetAlignX = Alignment.LEFT;
                        ScrollButton.OffsetAlignY = Alignment.MID;
                    }
                    RefreshScroll();
                }
            }
        }

        public bool IsHooked {
            get { return ScrollButton.IsHooked; }
        }

        private bool isMoving;

        public ScrollBar(WidgetRenderer wr, GLTexture tBase = null, GLTexture tButton = null)
            : base(wr, tBase) {
            // Defaults As A Vertical Scrollbar
            isVertical = true;
            scrollRatio = 0;
            isMoving = false;

            // Create The Scroll Button
            ScrollButton = new RectButton(wr, Width, Height, Color, Color, tButton);
            ScrollButton.Parent = this;
            ScrollButton.AlignX = Alignment.MID;
            ScrollButton.AlignY = Alignment.TOP;
            ScrollButton.OffsetAlignX = Alignment.MID;
            ScrollButton.OffsetAlignY = Alignment.TOP;
            RefreshScroll();
        }
        protected override void DisposeOther() {
            OnScrollValueChanged = null;
            base.DisposeOther();
            ScrollButton.Dispose();
        }

        public void Hook() {
            if(IsHooked) return;
            isMoving = false;
            ScrollButton.Hook();
            ScrollButton.OnButtonPress += ScrollButton_OnButtonPress;
            MouseEventDispatcher.OnMouseMotion += MouseEventDispatcher_OnMouseMotion;
            MouseEventDispatcher.OnMouseRelease += MouseEventDispatcher_OnMouseRelease;
        }
        public void Unhook() {
            if(!IsHooked) return;
            isMoving = false;
            ScrollButton.Unhook();
            ScrollButton.OnButtonPress -= ScrollButton_OnButtonPress;
            MouseEventDispatcher.OnMouseMotion -= MouseEventDispatcher_OnMouseMotion;
            MouseEventDispatcher.OnMouseRelease -= MouseEventDispatcher_OnMouseRelease;
        }

        public void RefreshScroll() {
            // Find Offset
            int off = (int)(scrollRatio * ScrollAmount);

            // Set Offset To ScrollButton
            if(IsVertical)
                ScrollButton.Offset = new Point(0, off);
            else
                ScrollButton.Offset = new Point(off, 0);
        }

        public void MoveScrollBar(float dx, float dy) {
            ScrollRatio += IsVertical ? dy / ScrollAmount : dx / ScrollAmount;
        }

        void ScrollButton_OnButtonPress(RectButton arg1, Vector2 arg2) {
            isMoving = true;
        }
        void MouseEventDispatcher_OnMouseRelease(object sender, MouseButtonEventArgs e) {
            isMoving = false;
        }
        void MouseEventDispatcher_OnMouseMotion(object sender, MouseMoveEventArgs e) {
            if(isMoving) MoveScrollBar(e.XDelta, e.YDelta);
        }
    }
}