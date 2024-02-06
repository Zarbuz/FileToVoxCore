using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FileToVoxCore.Vox.Chunks
{
    public class MaterialChunk : IEquatable<MaterialChunk>
    {
        public int Id;
        public Dictionary<string, string> Properties;

        public MaterialType Type
        {
            get
            {
                MaterialType result = MaterialType._diffuse;
				if (Properties.TryGetValue("_type", out string value))
				{
					Enum.TryParse(value, out result);
				}
                return result;
            }
        }

        public float Rough
        {
            get
            {
                float result = 1f;
                if (Properties.TryGetValue("_rough", out string value))
                {
	                float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				}

				return result;
            }
        }
       
        public float Flux
        {
            get
            {
                float result = 1f;

                if (Properties.TryGetValue("_flux", out string value))
                {
	                float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                }

				return result;
            }
        }

        public float Ior
        {
            get
            {
                float result = 1f;

                if (Properties.TryGetValue("_ior", out string value))
                {
	                float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                }

				return result;
            }
        }

        public float Plastic
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_plastic", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
			
		        return result;
	        }
        }

		public float Att
        {
            get
            {
                float result = 1f;
                if (Properties.TryGetValue("_att", out string value))
                {
	                float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                }
				return result;
            }
        }

        public float Alpha
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_alpha", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
				return result;
	        }
        }

        public float Metal
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_metal", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
				return result;
	        }
		}

        public float Specular
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_sp", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
				
				return result;
	        }
		}

        public float Emit
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_emit", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
			
				return result;
	        }
        }

        public float Glow
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_glow", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
		        return result;
	        }
        }

        public float Unit
        {
	        get
	        {
		        float result = 1f;
		        if (Properties.TryGetValue("_unit", out string value))
		        {
			        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		        }
		        return result;
	        }
        }

		public float Smoothness => 1 - Rough;
        public float Emission => Type == MaterialType._emit ? Emit * Flux : 0;

        public override string ToString()
        {
	        return $"{Type}";
        }

        public bool Equals(MaterialChunk other)
        {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Properties.SequenceEqual(other.Properties);
        }

        public override bool Equals(object obj)
        {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((MaterialChunk)obj);
        }

        public override int GetHashCode()
        {
	        return Properties != null ? Properties.GetHashCode() : 0;
        }
    }
}
