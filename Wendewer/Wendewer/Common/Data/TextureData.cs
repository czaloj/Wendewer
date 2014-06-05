using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wdw.Common.Events;
using Wdw.Common.Property;

namespace Wdw.Common.Data {
    public class TextureData : IDisposable {
        private static readonly UUIDGen IDGen = new UUIDGen();
        public const int EVENT_CREATION = 0;
        public const int EVENT_DESTRUCTION = EVENT_CREATION + 1;
        public const int EVENT_FILE = EVENT_DESTRUCTION + 1;

        public PropertyList Properties {
            get;
            private set;
        }
        public string Name {
            get { return Properties.Get<string>("Name").Data; }
            set { Properties.Get<string>("Name").SetData(value); }
        }

        private string file;
        public string FileLocation {
            get { return file; }
            set {
                file = value;
                MasterData.SendEvent(new DataEvent(this, EVENT_FILE));
            }
        }

        public TextureData() {
            file = null;

            Properties = new PropertyList();
            Properties.Add(new StringProperty("Name"));
            Name = "Texture." + IDGen.Obtain();

            MasterData.SendEvent(new DataEvent(this, EVENT_CREATION));
        }
        public void Dispose() {
            MasterData.SendEvent(new DataEvent(this, EVENT_DESTRUCTION));
        }
    }
}
