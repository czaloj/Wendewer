using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlisterUI {
    public class ScreenList {
        public const int NO_START_SELECTED = -1;
        public const int NO_SCREEN = -2;

        protected MainGame game;

        protected IGameScreen[] screens;
        protected int current;

        public IGameScreen Current {
            get {
                try {
                    return screens[current];
                }
                catch(Exception) {
                    return null;
                }
            }
        }
        public IGameScreen Next {
            get {
                try {
                    current = Current.Next;
                    return Current;
                }
                catch(Exception) {
                    return null;
                }
            }
        }
        public IGameScreen Previous {
            get {
                try {
                    current = Current.Previous;
                    return Current;
                }
                catch(Exception) {
                    return null;
                }
            }
        }

        public ScreenList(MainGame game) {
            this.game = game;
            current = NO_START_SELECTED;
        }
        public ScreenList(MainGame game, int startScreen, params IGameScreen[] screens)
            : this(game) {
            SetStartScreen(startScreen);
            AddScreens(screens);
        }

        public void SetStartScreen(int s) {
            if(current == NO_START_SELECTED) {
                current = s;
            }
        }
        public void AddScreens(params IGameScreen[] s) {
            //Copy Over The Screens
            int l;
            if(screens == null) {
                l = 0;
                screens = s;
            }
            else {
                l = screens.Length;
                Array.Resize<IGameScreen>(ref screens, screens.Length + s.Length);
                Array.Copy(s, 0, screens, l, s.Length);
            }

            //Build The Added Screens
            for(int i = l; i < screens.Length; i++) {
                screens[i].SetParentGame(game, i);
                screens[i].Build();
            }
        }

        public void Destroy(GameTime gameTime) {
            foreach(IGameScreen screen in screens) {
                screen.Destroy(gameTime);
            }
        }
    }
}
