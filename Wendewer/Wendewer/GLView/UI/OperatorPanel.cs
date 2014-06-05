using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using OpenTK;
using Wdw.Common.Data;
using Wdw.Common.Operators;

namespace Wdw.GLView.UI {
    public class OperatorPanel : RectWidget {
        TextButton btnExecute;
        MasterData data;
        OpBase curOp;
        LinkedList<IDisposable> toDispose;

        public event EventHandler OnOperationFinished;

        public OperatorPanel(WidgetRenderer wr, int w, int h, OpBase op, MasterData d)
            : base(wr) {
            if(op == null || d == null) return;

            Color = new Vector4(0.08f, 0.08f, 0.08f, 1f);

            data = d;
            curOp = op;

            btnExecute = new TextButton(wr, w, h,
                new Vector4(0.01f, 0.01f, 0.01f, 1f),
                new Vector4(0.31f, 0.31f, 0.31f, 1f)
                );
            btnExecute.TextColor = new Vector4(1, 1, 1, 1);
            btnExecute.Text = "Execute";


            toDispose = new LinkedList<IDisposable>();
            curOp.Display(wr, this, ref toDispose);

            if(Width < w) Width = w;
            Height += btnExecute.Height;

            btnExecute.ActiveWidth = Width;
            btnExecute.InactiveWidth = Width;
            btnExecute.AlignX = Alignment.MID;
            btnExecute.AlignY = Alignment.BOTTOM;
            btnExecute.OffsetAlignX = Alignment.MID;
            btnExecute.OffsetAlignY = Alignment.BOTTOM;
            btnExecute.Parent = this;
            btnExecute.OnButtonPress += btnExecute_OnButtonPress;
            btnExecute.Hook();
        }
        protected override void DisposeOther() {
            base.DisposeOther();
            if(btnExecute != null)
                btnExecute.Dispose();
            if(toDispose != null)
                foreach(var o in toDispose) o.Dispose();
            OnOperationFinished = null;
        }

        void btnExecute_OnButtonPress(RectButton arg1, Vector2 arg2) {
            curOp.Operate(data);
            if(OnOperationFinished != null)
                OnOperationFinished(this, null);
        }
    }
}