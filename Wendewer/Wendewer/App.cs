using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BlisterUI;
using BlisterUI.Input;

namespace Wdw {
    public class App : MainGame {

        protected override void BuildScreenList() {
            screenList = new ScreenList(this, 0,
                new FalseFirstScreen(1)
                );
        }
        protected override void FullInitialize() {
            WMHookInput.Initialize(Window);
        }
        protected override void FullLoad() {
        }

        static void Main(string[] args) {
            using(App app = new App()) {
                app.Run();
            }
        }
    }
}
