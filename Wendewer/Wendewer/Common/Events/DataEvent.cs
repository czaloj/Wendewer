using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wdw.Common.Data;

namespace Wdw.Common.Events {
    public enum DataSource {
        Scene,
        Object,
        Mesh,
        Material,
        Texture,
        None
    }

    public class DataEvent : EventArgs {
        // The Source Of The Event
        public DataSource SourceType;
        public object Source;
        public int MetaData;

        public DataEvent(int md = 0) {
            SourceType = DataSource.None;
            Source = null;
            MetaData = md;
        }
        public DataEvent(SceneData d, int md = 0) {
            SourceType = DataSource.Scene;
            Source = d;
            MetaData = md;
        }
        public DataEvent(ObjectData d, int md = 0) {
            SourceType = DataSource.Object;
            Source = d;
            MetaData = md;
        }
        public DataEvent(MeshData d, int md = 0) {
            SourceType = DataSource.Mesh;
            Source = d;
            MetaData = md;
        }
        public DataEvent(MaterialData d, int md = 0) {
            SourceType = DataSource.Material;
            Source = d;
            MetaData = md;
        }
        public DataEvent(TextureData d, int md = 0) {
            SourceType = DataSource.Texture;
            Source = d;
            MetaData = md;
        }
    }
}
