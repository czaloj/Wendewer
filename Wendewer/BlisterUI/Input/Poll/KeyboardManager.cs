using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace BlisterUI.Input {
    public class KeyboardManager {
        private static readonly Key[] allKeys;
        static KeyboardManager() {
            allKeys = (Key[])Enum.GetValues(typeof(Key));
        }

        protected KeyboardState cKS, pKS;
        public KeyboardState Current {
            get { return cKS; }
        }
        public KeyboardState Previous {
            get { return pKS; }
        }

        public IEnumerable<Key> AllKeysJustPressed {
            get {
                foreach(Key k in allKeys) {
                    if(IsKeyJustPressed(k))
                        yield return k;
                }
            }
        }
        public IEnumerable<Key> AllKeysJustReleased {
            get {
                foreach(Key k in allKeys) {
                    if(IsKeyJustReleased(k))
                        yield return k;
                }
            }
        }

        public KeyboardManager() {
            Refresh();
            pKS = cKS;
        }

        public bool IsKeyJustPressed(Key key) {
            return cKS.IsKeyDown(key) && pKS.IsKeyUp(key);
        }
        public bool IsKeyJustReleased(Key key) {
            return cKS.IsKeyUp(key) && pKS.IsKeyDown(key);
        }

        public void Refresh() {
            pKS = cKS;
            cKS = Keyboard.GetState();
        }
    }
}
