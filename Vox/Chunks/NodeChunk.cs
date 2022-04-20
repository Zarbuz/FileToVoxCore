using System;

namespace FileToVoxCore.Vox.Chunks
{
    public struct KeyValue : IEquatable<KeyValue>
    {
        public string Key, Value;

        public bool Equals(KeyValue other)
        {
	        return Key == other.Key && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
	        return obj is KeyValue other && Equals(other);
        }

        public override int GetHashCode()
        {
	        unchecked
	        {
		        return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
	        }
        }
    }

    public enum NodeType { Transform, Group, Shape, }

    public abstract class NodeChunk
    {
        public int Id;
        public KeyValue[] Attributes;
        public abstract NodeType Type { get; }
    }
}
