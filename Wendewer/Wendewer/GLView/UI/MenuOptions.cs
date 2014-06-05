using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using EGL;
using OpenTK;
using OpenTK.Input;
using Wdw.Common.Data;

namespace Wdw.GLView.UI {
    public class StringTree {
        public readonly int Level;
        public readonly string Value;
        public List<StringTree> Children;
        public List<string> Ops;

        public int Count {
            get { return Children.Count + Ops.Count; }
        }

        public StringTree(string value, IEnumerable<string> ch)
            : this(value, ch, 0) {
        }
        private StringTree(string value, IEnumerable<string> ch, int level) {
            Level = level;
            Value = value;
            Children = new List<StringTree>();
            Ops = new List<string>();
            var grouped = ch.GroupBy(Grouper);
            foreach(var g in grouped.Where(SelectorNN)) {
                Children.Add(new StringTree(g.Key, g.Select(Selector).Where(SelectorNN), level + 1));
            }
            foreach(var g in grouped.Where(SelectorN)) {
                Ops.AddRange(g);
            }
        }

        public string Grouper(string value) {
            if(value.Contains('.')) {
                int i = value.IndexOf('.');
                if(i == 0) return "";
                return value.Substring(0, i);
            }
            return null;
        }
        public string Selector(string value) {
            if(value != null || value.Contains('.')) {
                int i = value.IndexOf('.');
                if(i == value.Length - 1) return null;
                return value.Substring(i + 1, value.Length - i - 1);
            }
            return null;
        }
        public bool SelectorNN(string value) {
            return !string.IsNullOrWhiteSpace(value);
        }
        public bool SelectorNN(IGrouping<string, string> value) {
            return !string.IsNullOrWhiteSpace(value.Key);
        }
        public bool SelectorN(IGrouping<string, string> value) {
            return value.Key == null;
        }

        public StringTree GetTree(string s) {
            string[] sa = s.Split('.');
            if(sa.Length < 1)
                return null;
            string sLeaf = sa.Length < 2 ? null : string.Join(".", sa, 1, sa.Length - 1);
            foreach(var c in Children) {
                if(sa[0].Equals(c.Value))
                    return sLeaf == null ? c : c.GetTree(sLeaf);
            }
            return null;
        }
        public bool HasOp(string s) {
            if(!s.Contains('.')) {
                return Ops.Contains(s);
            }

            string[] sa = s.Split('.');
            string sLeaf = string.Join(".", sa, 1, sa.Length - 1);
            foreach(var c in Children) {
                if(sa[0].Equals(c.Value))
                    return c.HasOp(sLeaf);
            }
            return false;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(Value);
            sb.Append('\n');
            if(Level == 0) {
                foreach(var c in Children)
                    sb.AppendFormat("|- {0}", c.ToString());
                foreach(var c in Ops)
                    sb.AppendFormat("|- {0}\n", c.ToString());
            }
            else {
                foreach(var c in Children) {
                    for(int i = 0; i < Level; i++)
                        sb.Append("|  ");
                    sb.AppendFormat("|- {0}", c.ToString());
                }
                foreach(var c in Ops) {
                    for(int i = 0; i < Level; i++)
                        sb.Append("|  ");
                    sb.AppendFormat("|- {0}\n", c.ToString());
                }
            }
            return sb.ToString();
        }
    }

    public class MenuPickArgs : EventArgs {
        public string Operation;

        public MenuPickArgs(string op) {
            Operation = op;
        }
    }

    public class MenuOptions : RectWidget {
        #region Stack Struct
        public struct MenuStack {
            public static readonly MenuStack Empty = new MenuStack(null, null);
            public ScrollMenu Menu;
            public StringTree Tree;
            public MenuStack(ScrollMenu m, StringTree t) {
                Menu = m;
                Tree = t;
            }
            public override string ToString() {
                return Tree == null ? "" : Tree.Value;
            }
        } 
        #endregion

        private StringTree sTree;
        private Stack<MenuStack> menus;
        private MenuStack CurStack {
            get { return menus.Count > 0 ? menus.Peek() : MenuStack.Empty; }
        }
        private ScrollMenu CurMenu {
            get { return CurStack.Menu; }
        }
        private string CurrentDirectory {
            get {
                return menus.Count < 1 ? "" : string.Join(".", menus.Reverse()) + ".";
            }
        }

        private WidgetRenderer wr;
        private TextButton[] buttons;

        public event EventHandler<MenuPickArgs> OnMenuPick;

        public MenuOptions(WidgetRenderer _wr, int w, int h, SpriteFont font, int fontHeight, IEnumerable<string> ops)
            : base(_wr) {
            wr = _wr;
            X = 0;
            Y = 0;
            Width = w;
            Height = h;

            sTree = new StringTree("ops", ops);

            // Find Out The Size Of The Buttons
            StringBuilder sb = new StringBuilder();
            string[] texts = new string[sTree.Count];
            int bi = 0;
            foreach(var s in sTree.Children) {
                sb.Append(s.Value);
                texts[bi++] = s.Value;
            }
            foreach(var s in sTree.Ops) {
                sb.Append(s);
                texts[bi++] = s;
            }
            Vector2 size = font.MeasureString(sb.ToString()) * ((float)fontHeight / (float)font.FontHeight);
            int sx = (int)(size.X + 0.5f);
            int padding = (Width - sx) / sTree.Count;

            buttons = new TextButton[texts.Length];
            for(int i = 0; i < texts.Length; i++) {
                int bw = (int)(font.MeasureString(texts[i]).X * ((float)fontHeight / (float)font.FontHeight) + 0.5f);
                bw += padding;
                buttons[i] = new TextButton(wr, bw, h, Vector4.One * 0.5f, Vector4.One, font);
                buttons[i].TextColor = new Vector4(0, 0, 0, 1);
                buttons[i].Text = texts[i];
                buttons[i].TextWidget.Height = fontHeight;
                if(i > 0) {
                    buttons[i].Parent = buttons[i - 1];
                    buttons[i].LayerOffset = 0;
                    buttons[i].OffsetAlignX = Alignment.RIGHT;
                }
                else buttons[i].Parent = this;
            }

            menus = new Stack<MenuStack>();
        }

        public ScrollMenu AddMenu(StringTree st, int w, int maxCount) {
            if(st == null)
                return null;

            int count = Math.Min(maxCount, st.Count);
            ScrollMenu sm = new ScrollMenu(wr, w, Height, count, 5, (count * Height) / st.Count);
            string[] texts = new string[st.Count];
            int bi = 0;
            foreach(var s in st.Children) {
                texts[bi++] = s.Value;
            }
            foreach(var s in st.Ops) {
                texts[bi++] = s;
            }


            if(menus.Count < 1) {
                var btn = buttons.FirstOrDefault((b) => { return b.Text.Equals(st.Value); });
                if(btn == null) {
                    sm.Dispose();
                    return null;
                }
                sm.Widget.OffsetAlignY = Alignment.BOTTOM;
                sm.Parent = btn;
            }
            else {
                var btn = CurMenu.Buttons.FirstOrDefault((b) => { return b.Text.Equals(st.Value); });
                if(btn == null) {
                    sm.Dispose();
                    return null;
                }
                sm.Widget.OffsetAlignX = Alignment.RIGHT;
                sm.Parent = btn;
            }

            sm.Build(texts);
            sm.Hook();
            menus.Push(new MenuStack(sm, st));

            return sm;
        }

        protected override void DisposeOther() {
            foreach(var menu in menus)
                menu.Menu.Dispose();
            foreach(var button in buttons)
                button.Dispose();
            base.DisposeOther();
        }

        public void Hook() {
            foreach(var button in buttons)
                button.Hook();

            foreach(var menu in menus)
                menu.Menu.Dispose();
            menus.Clear();

            MouseEventDispatcher.OnMousePress += OnMP;
            MouseEventDispatcher.OnMouseMotion += OnMM;
        }
        public void Unhook() {
            foreach(var button in buttons)
                button.Hook();

            foreach(var menu in menus)
                menu.Menu.Dispose();
            menus.Clear();

            MouseEventDispatcher.OnMouseMotion -= OnMM;
        }

        private void OnMP(object sender, MouseButtonEventArgs e) {
            if(OnMenuPick == null) return;

            if(e.Button != MouseButton.Left) return;
            if(CurMenu == null) {
                if(Inside(e.X, e.Y)) {
                    foreach(var btn in buttons) {
                        if(btn.Inside(e.X, e.Y)) {
                            if(sTree.HasOp(btn.Text))
                                OnMenuPick(this, new MenuPickArgs(btn.Text));
                        }
                    }
                }
            }
            else {
                string sel = CurMenu.GetSelection(e.X, e.Y);
                if(sel != null) {
                    var op = CurrentDirectory + sel;
                    if(sTree.HasOp(op))
                        OnMenuPick(this, new MenuPickArgs(op));
                }
            }
        }
        private void OnMM(object sender, MouseMoveEventArgs e) {
            if(CurMenu == null) {
                if(Inside(e.X, e.Y)) {
                    foreach(var btn in buttons) {
                        if(btn.Inside(e.X, e.Y)) {
                            AddMenu(sTree.GetTree(btn.Text), 140, 6);
                            return;
                        }
                    }
                }
            }
            else {
                while(CurMenu != null && !CurMenu.Inside(e.X, e.Y)) {
                    var menu = menus.Pop();
                    menu.Menu.Dispose();
                }
                if(CurMenu == null) {
                    OnMM(sender, e);
                    return;
                }

                string newMenu = CurMenu.GetSelection(e.X, e.Y);
                AddMenu(sTree.GetTree(CurrentDirectory + newMenu), 140, 6);
            }
        }
    }
}