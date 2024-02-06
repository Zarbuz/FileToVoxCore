using System.Collections.Generic;

namespace FileToVoxCore.Vox.Chunks
{
	public enum NodeType { Transform, Group, Shape, }

    public abstract class NodeChunk
    {
        public int Id;
        public Dictionary<string, string> Attributes;
        public abstract NodeType Type { get; }
    }
}
