using System.Collections.Generic;

namespace FileToVoxCore.Vox.Chunks
{
	public class LayerChunk
    { // LAYR: Layer Chunk
        public int Id;
        public Dictionary<string, string> Attributes;
        public int Unknown;

        public string Name
        {
            get
            {
				if (Attributes.TryGetValue("_name", out string name))
				{
					return name;
				}

				return string.Empty;
            }
        }
        public bool Hidden
        {
            get
            {
				if (Attributes.TryGetValue("_hidden", out string value))
				{
					return value != "0";
				}

				return false;
            }
        }
    }
}
