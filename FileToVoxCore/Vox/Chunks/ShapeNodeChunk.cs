using System.Collections.Generic;

namespace FileToVoxCore.Vox.Chunks
{
    public class ShapeModel
    {
        public int ModelId;
        public Dictionary<string, string> Attributes;
    }

    public class ShapeNodeChunk : NodeChunk
    { // nSHP: Shape Node Chunk
        public ShapeModel[] Models;
        public override NodeType Type => NodeType.Shape;
    }
}
