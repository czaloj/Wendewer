using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using OpenTK;
using Wdw.Common.Data;
using Wdw.Common.Property;
using Wdw.GLView.UI;

namespace Wdw.Common.Operators {
    public class OpDataObjectAdd : OpBase {
        public override string Name {
            get { return "Data.Object.Add"; }
        }

        StringProperty pName;

        public string DataName {
            get { return pName.Data; }
            set { pName.SetData(value); }
        }

        public override void Initialize(Data.MasterData data) {
            pName = new StringProperty("Name");
            Properties.Add(pName);
        }
        public override void Destroy(Data.MasterData data) {
        }

        public override void Operate(Data.MasterData data) {
            var d = data.CreateObject();
            if(DataName != null) d.Name = DataName;
            if(data.Materials.Count > 0) d.Material = data.Materials[0];
            if(data.Meshes.Count > 0) d.Mesh = data.Meshes[0];
            d.Transform = Matrix4.Identity;
        }

        public override void Display(WidgetRenderer wr, BaseWidget panel, ref LinkedList<IDisposable> toDispose) {
            var wName = new TextInputWidget(wr);
            wName.Text = "Enter Name";
            wName.Color = new Vector4(1, 1, 0, 1);
            wName.Height = 24;
            wName.Parent = panel;
            wName.ActivateInput();
            wName.Caret.Width = 1;
            wName.Caret.Color = new Vector4(1, 0, 1, 1);
            toDispose.AddLast(wName);

            panel.Width = 300;
            panel.Height = wName.Width;
        }
    }
}