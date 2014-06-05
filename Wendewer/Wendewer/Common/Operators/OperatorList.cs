using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wdw.Common.Data;

namespace Wdw.Common.Operators {
    public static class OperatorList {
        private static readonly Dictionary<string, OpBase> Ops = new Dictionary<string, OpBase>();
        private static MasterData lastData = null;

        public static IEnumerable<OpBase> Operators {
            get { return Ops.Values; }
        }

        static OperatorList() {
            Add(new OpDataMeshAdd());
            Add(new OpDataObjectAdd());
            Add(new OpDataObjectSelect());
            Add(new OpDataClear());
        }

        public static void Add(OpBase op) {
            Ops.Add(op.Name, op);
        }

        public static OpBase Get(string name) {
            OpBase op;
            if(!Ops.TryGetValue(name, out op))
                return null;
            return op;
        }
        public static T Get<T>(string name) where T : OpBase {
            var op = Get(name);
            return op == null ? null : op as T;
        }

        public static void InitializeAll(MasterData d) {
            if(lastData != null) foreach(var op in Ops.Values) op.Destroy(lastData);
            lastData = d;
            if(lastData != null) foreach(var op in Ops.Values) op.Initialize(lastData);
        }
    }
}
