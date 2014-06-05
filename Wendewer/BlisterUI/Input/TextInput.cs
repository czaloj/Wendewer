﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace BlisterUI.Input {
    public class TextInput : IDisposable {
        public bool IsActive {
            get;
            private set;
        }

        private StringBuilder text;
        public string Text {
            get { return text.ToString(); }
            set {
                text.Clear();
                text.Append(value);
                Caret = Math.Min(Caret, Length);
                if(OnTextChanged != null)
                    OnTextChanged(this, Text);
            }
        }
        private int caret;
        public int Caret {
            get { return caret; }
            private set {
                caret = value;
                if(OnCaretMoved != null)
                    OnCaretMoved(this, Caret);
            }
        }
        public int Length {
            get { return text.Length; }
        }

        public event Action<TextInput, string> OnTextEntered;
        public event Action<TextInput, string> OnTextChanged;
        public event Action<TextInput, int> OnCaretMoved;

        public TextInput() {
            text = new StringBuilder();
            Caret = 0;
            IsActive = false;
        }
        public void Dispose() {
            OnTextEntered = null;
            OnTextChanged = null;
            OnCaretMoved = null;
            Deactivate();
            text.Clear();
            text = null;
        }

        public void Activate() {
            if(IsActive) return;
            IsActive = true;

            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.ReceiveChar += OnChar;
        }
        public void Deactivate() {
            if(!IsActive) return;
            IsActive = false;

            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.ReceiveChar -= OnChar;
        }

        public void Insert(char c) {
            text.Insert(Caret, c);
            Caret++;
            if(OnTextChanged != null)
                OnTextChanged(this, Text);
        }
        public void Insert(string s) {
            if(string.IsNullOrEmpty(s))
                return;
            text.Insert(Caret, s);
            Caret += s.Length;
            if(OnTextChanged != null)
                OnTextChanged(this, Text);
        }
        public void Delete() {
            if(Caret == Length)
                return;
            text.Remove(Caret, 1);
            if(OnTextChanged != null)
                OnTextChanged(this, Text);
        }
        public void BackSpace() {
            if(Caret == 0)
                return;
            Caret--;
            Delete();
        }

        public void OnKeyPress(object s, KeyboardKeyEventArgs args) {
            switch(args.Key) {
                case Key.Enter:
                    if(text.Length < 1)
                        return;
                    if(OnTextEntered != null)
                        OnTextEntered(this, Text);
                    return;
                case Key.Back:
                    BackSpace();
                    return;
                case Key.Delete:
                    Delete();
                    return;
                case Key.Left:
                    if(Caret > 0) Caret--;
                    return;
                case Key.Right:
                    if(Caret < Length) Caret++;
                    return;
                case Key.V:
                    if(args.Modifiers != KeyModifiers.Control) return;
                    string c = KeyboardEventDispatcher.GetNewClipboard();
                    Insert(c);
                    return;
                case Key.C:
                    if(args.Modifiers != KeyModifiers.Control) return;
                    if(text.Length > 0)
                        KeyboardEventDispatcher.SetToClipboard(Text);
                    return;
            }
        }
        public void OnChar(object s, KeyPressEventArgs args) {
            Insert(args.KeyChar);
        }
    }
}