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
    public class OpDataMeshAdd : OpBase {
        public override string Name {
            get { return "Data.Mesh.Add"; }
        }

        StringProperty pName, pFile, pType;
        BoolProperty pUseFile;
        FloatProperty pTolerance;

        public string DataName {
            get { return pName.Data; }
            set { pName.SetData(value); }
        }
        public bool UseFile {
            get { return pUseFile.Data; }
            set { pUseFile.SetData(value); }
        }
        public string DataFile {
            get { return pFile.Data; }
            set { pFile.SetData(value); }
        }
        public string PrimType {
            get { return pType.Data; }
            set { pType.SetData(value); }
        }
        public float Tolerance {
            get { return pTolerance.Data; }
            set { pTolerance.SetData(value); }
        }

        public override void Initialize(Data.MasterData data) {
            pName = new StringProperty("Name");
            Properties.Add(pName);
            pUseFile = new BoolProperty("Use File");
            Properties.Add(pUseFile);
            pFile = new StringProperty("File");
            Properties.Add(pFile);
            pType = new StringProperty("Primitive Type");
            Properties.Add(pType);
            pTolerance = new FloatProperty("Tolerance");
            Properties.Add(pTolerance);
        }
        public override void Destroy(Data.MasterData data) {
        }

        public override void Operate(Data.MasterData data) {
            MeshData mesh = null;
            if(UseFile) {
                if(string.IsNullOrWhiteSpace(DataFile) || !File.Exists(DataFile))
                    return;
                mesh = new MeshData();
                mesh.ReadData(DataFile);
            }
            else {
                if(Tolerance < 1e-5 || Tolerance > 1)
                    return;
                switch(PrimType.ToLower()) {
                    case "ball":
                    case "sphere":
                        mesh = data.BuildSphere(Tolerance);
                        break;
                    case "box":
                    case "cube":
                        mesh = data.BuildSphere(Tolerance);
                        break;
                    case "cylinder":
                    case "rod":
                        mesh = data.BuildSphere(Tolerance);
                        break;
                    default:
                        return;
                }
            }
            if(DataName != null) mesh.Name = DataName;
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

            BoolWidget wUseFile = new BoolWidget(pUseFile, wr, 200, 24);
            wUseFile.Parent = wName;
            wUseFile.Offset = new Point(0, 26);
            wUseFile.Hook();
            toDispose.AddLast(wUseFile);

            panel.Width = 300;
            panel.Height = 80;
        }
    }
}