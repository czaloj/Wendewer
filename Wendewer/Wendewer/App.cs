using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using BlisterUI;
using BlisterUI.Input;
using Wdw.RT;

namespace Wdw {
    public class App : MainGame {

        protected override void BuildScreenList() {
            screenList = new ScreenList(this, 0,
                new FalseFirstScreen(1),
                new AppScreen()
                );
        }
        protected override void FullInitialize() {
            Width = 1000;
            Height = 600;
        }
        protected override void FullLoad() {
        }

        static void Main(string[] args) {
            ZXPCExt.AddVecTypes();

            //RayTracer rayTracer = new RayTracer();
            //rayTracer.run("data/scenes", new string[] {
            //    "test.txt"
            //});

            using(App app = new App()) {
                app.Run();
            }
        }
    }
}