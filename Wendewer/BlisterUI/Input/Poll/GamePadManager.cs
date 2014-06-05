using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace BlisterUI.Input {
    public sealed class GamePadManager {
        public int Index {
            get;
            private set;
        }

        private GamePadState cGS, pGS;
        public GamePadState Current {
            get { return cGS; }
        }
        public GamePadState Previous {
            get { return pGS; }
        }

        public GamePadManager(int i) {
            Index = i;
            Refresh();
            pGS = cGS;
        }

        public bool IsButtonJustPressed(Buttons button) {
            // TODO
            switch(button) {
                case Buttons.A:
                    return cGS.Buttons.A == ButtonState.Pressed && pGS.Buttons.A == ButtonState.Released;
                default:
                    return false;
            }
        }
        public bool IsButtonJustReleased(Buttons button) {
            switch(button) {
                case Buttons.A:
                    return cGS.Buttons.A == ButtonState.Released && pGS.Buttons.A == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public void Refresh() {
            pGS = cGS;
            cGS = GamePad.GetState(Index);
        }
    }
}
