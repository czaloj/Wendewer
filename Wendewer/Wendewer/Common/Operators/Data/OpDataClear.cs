using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;

namespace Wdw.Common.Operators {
    public class OpDataClear : OpBase {
        public override string Name {
            get { return "Data.Clear"; }
        }

        public override void Initialize(Data.MasterData data) {
        }
        public override void Destroy(Data.MasterData data) {
        }

        public override void Operate(Data.MasterData data) {
            data.RemoveAll();
        }

        public override void Display(WidgetRenderer wr, BaseWidget panel, ref LinkedList<IDisposable> toDispose) {
            panel.Width = 0;
            panel.Height = 0;
        }
    }
}