using System;
using System.Collections.Generic;

namespace FileToVoxCore.Vox.Chunks
{
    public class RendererSettingChunk
    { // rOBJ: Renderer Setting Chunk (undocumented)
        public Dictionary<string, string> Attributes;

        public RenderSettingType Type
        {
	        get
	        {
		        RenderSettingType renderSettingType = RenderSettingType._setting;
		        if (Attributes.TryGetValue("_type", out string type))
		        {
					Enum.TryParse(type, out renderSettingType);
				}
				return renderSettingType;
			}
		}
    }
}
