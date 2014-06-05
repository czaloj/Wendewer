using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System {
    public class UUIDGen {
        private uint uuid = 0xffffffff;
        private readonly Queue<uint> recycled = new Queue<uint>();

        public uint Obtain() {
            // Use Recycled UUID's First
            if(recycled.Count > 0) return recycled.Dequeue();

            // Increment And Return The New UUID
            unchecked { uuid++; }
            return uuid;
        }
        public void Recycle(uint uuid) {
            recycled.Enqueue(uuid);
        }
    }
}
