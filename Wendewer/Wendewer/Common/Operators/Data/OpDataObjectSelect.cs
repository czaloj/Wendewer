using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using BlisterUI.Widgets;
using Wdw.Common.Property;
using Wdw.Common.Data;

namespace Wdw.Common.Operators {
    public class OpDataObjectSelect : OpBase {
        public override string Name {
            get { return "Data.Object.Select"; }
        }

        private StringProperty pName;
        public string DataName {
            get { return pName.Data; }
            set { pName.SetData(value); }
        }

        public override void Initialize(Data.MasterData data) {
            pName = new StringProperty("Name");
            Properties.Add(pName);
            DataName = null;
        }
        public override void Destroy(Data.MasterData data) {
        }

        public override void Operate(Data.MasterData data) {
            if(DataName == null) {
                data.ObjectsSelected.Clear();
                data.ObjectActive = null;
                return;
            }

            ObjectData d;
            if(!data.ObjectsByName.TryGetValue(DataName, out d))
                return;
            data.ObjectsSelected.Add(d);
            data.ObjectActive = d;
        }

        public override void Display(WidgetRenderer wr, BaseWidget panel, ref LinkedList<IDisposable> toDispose) {
            TextWidget tw = new TextWidget(wr);
            tw.Text = "No, This Does Nothing";
            tw.Color = new Vector4(1, 0, 0, 1);
            tw.Height = 32;
            tw.AlignX = Alignment.MID;
            tw.OffsetAlignX = Alignment.MID;
            tw.Parent = panel;
            toDispose.AddLast(tw);

            panel.Width = 300;
            panel.Height = 40;
        }
    }
}