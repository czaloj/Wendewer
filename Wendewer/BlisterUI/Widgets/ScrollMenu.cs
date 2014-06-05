using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using EGL;
using System.Drawing;


namespace BlisterUI.Widgets {
    public class ScrollMenu : IDisposable {
        const float TEXT_H_RATIO = 0.95f;
        const int TEXT_X_OFF = 5;

        public RectWidget Widget;
        public TextButton[] Buttons {
            get;
            private set;
        }
        public ScrollBar ScrollBar {
            get;
            private set;
        }

        string[] vText;
        int si;

        public bool HasButtons {
            get { return Buttons != null && Buttons.Length > 0; }
        }
        public int ButtonCount {
            get { return HasButtons ? Buttons.Length : 0; }
        }
        public int DataCount {
            get { return vText.Length; }
        }
        public int FullWidth {
            get { return Widget.Width + ScrollBar.Width; }
        }

        public BaseWidget Parent {
            get { return Widget.Parent; }
            set { Widget.Parent = value; }
        }
        public Vector4 BaseColor {
            get { return Widget.Color; }
            set {
                Widget.Color = value;
                if(HasButtons) {
                    foreach(var b in Buttons)
                        b.InactiveColor = value;
                }
                ScrollBar.Color = value;
            }
        }
        public Vector4 HighlightColor {
            get { return HasButtons ? Buttons[0].Color : Vector4.Zero; }
            set {
                if(HasButtons) {
                    foreach(var b in Buttons)
                        b.ActiveColor = value;
                }
                ScrollBar.ScrollButton.ActiveColor = value;
            }
        }
        public Vector4 TextColor {
            get { return HasButtons ? Buttons[0].TextColor : Vector4.Zero; }
            set {
                if(HasButtons) {
                    foreach(var t in Buttons)
                        t.TextColor = value;
                }
            }
        }
        public Vector4 ScrollBarBaseColor {
            get { return ScrollBar.ScrollButton.InactiveColor; }
            set { ScrollBar.ScrollButton.InactiveColor = value; }
        }

        public ScrollMenu(WidgetRenderer wr, int w, int h, int bCount, int sbw, int sbh) {
            Widget = new RectWidget(wr);
            Widget.Width = w;
            Widget.Height = h * bCount;

            ScrollBar = new ScrollBar(wr);
            ScrollBar.IsVertical = true;
            ScrollBar.Width = sbw;
            ScrollBar.ScrollButton.InactiveWidth = ScrollBar.Width;
            ScrollBar.ScrollButton.InactiveHeight = sbh;
            ScrollBar.ScrollButton.ActiveWidth = ScrollBar.Width;
            ScrollBar.ScrollButton.ActiveHeight = sbh;
            ScrollBar.Height = Widget.Height;
            ScrollBar.OffsetAlignX = Alignment.RIGHT;
            ScrollBar.Parent = Widget;
            ScrollBar.ScrollRatio = 0;


            Buttons = new TextButton[bCount];
            for(int i = 0; i < Buttons.Length; i++) {
                Buttons[i] = new TextButton(wr, Widget.Width, h, Vector4.UnitW, Vector4.One);
                if(i > 0) {
                    Buttons[i].Parent = Buttons[i - 1];
                    Buttons[i].OffsetAlignY = Alignment.BOTTOM;
                    Buttons[i].LayerOffset = 0f;
                }
                else {
                    Buttons[i].Parent = Widget;
                }
                Buttons[i].TextWidget.Height = (int)(TEXT_H_RATIO * Buttons[i].Height);
                Buttons[i].Text = "";
                Buttons[i].TextWidget.Offset = new Point(TEXT_X_OFF, 0);
                Buttons[i].TextWidget.AlignX = Alignment.LEFT;
                Buttons[i].TextWidget.OffsetAlignX = Alignment.LEFT;
            }

            BaseColor = Vector4.UnitW;
            HighlightColor = new Vector4(0.05f, 0.05f, 0.05f, 1);
            TextColor = Vector4.One;
            ScrollBarBaseColor = new Vector4(1, 0, 0, 1);
        }
        public void Dispose() {
            Widget.Dispose();
            if(HasButtons) foreach(var b in Buttons) b.Dispose();
            ScrollBar.Dispose();
        }

        public void Hook() {
            if(HasButtons) {
                foreach(var b in Buttons) {
                    b.Hook();
                }
            }
            ScrollBar.Hook();
            ScrollBar.OnScrollValueChanged += ScrollChange;
        }
        public void Unhook() {
            ScrollBar.OnScrollValueChanged -= ScrollChange;
            ScrollBar.Unhook();
            if(HasButtons) {
                foreach(var b in Buttons) {
                    b.Unhook();
                }
            }
        }

        public void Build(string[] options) {
            vText = new string[options.Length];
            options.CopyTo(vText, 0);
            si = -1;
            RefreshVisible();
        }

        void ScrollChange(ScrollBar sb, float r) {
            RefreshVisible();
        }
        public void RefreshVisible() {
            int lo = DataCount - ButtonCount;
            if(si < 0) {
                int mc = Math.Min(DataCount, ButtonCount);
                for(int i = 0; i < mc; i++) {
                    Buttons[i].Text = vText[i];
                }
                si = 0;
                return;
            }
            if(lo > 0) {
                int nsi = (int)((lo + 1) * ScrollBar.ScrollRatio);
                nsi = Math.Max(0, Math.Min(lo, nsi));
                if(si != nsi) {
                    si = nsi;
                    for(int i = 0; i < ButtonCount; i++) {
                        Buttons[i].Text = vText[i + si];
                    }
                }
            }
        }

        public bool Inside(int x, int y) {
            return Widget.Inside(x, y) || ScrollBar.Inside(x, y);
        }
        public string GetSelection(int x, int y) {
            if(!Widget.Inside(x, y)) return null;

            for(int i = 0; i < ButtonCount; i++) {
                if(Buttons[i].Inside(x, y))
                    return Buttons[i].Text;
            }
            return null;
        }
    }
}