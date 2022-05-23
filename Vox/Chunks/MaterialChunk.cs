using System;
using System.Globalization;
using System.Linq;

namespace FileToVoxCore.Vox.Chunks
{
    public class MaterialChunk : IEquatable<MaterialChunk>
    {
        public int Id;
        public KeyValue[] Properties;

        public MaterialType Type
        {
            get
            {
                MaterialType result = MaterialType._diffuse;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_type");
                if (item.Key != null)
                    Enum.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Rough
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_rough");
                if (item.Key != null)
	                float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

				return result;
            }
        }

       
        public float Flux
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_flux");
                if (item.Key != null)
					float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				return result;
            }
        }

        public float Ior
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_ior");
                if (item.Key != null)
	                float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				return result;
            }
        }

        public float Att
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_att");
                if (item.Key != null)
					float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				return result;
            }
        }

        public float Alpha
        {
	        get
	        {
		        float result = 1f;
		        KeyValue item = Properties.FirstOrDefault(i => i.Key == "_alpha");
		        if (item.Key != null)
			        float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				return result;
	        }
        }

        public float Metal
        {
	        get
	        {
		        float result = 1f;
		        KeyValue item = Properties.FirstOrDefault(i => i.Key == "_metal");
		        if (item.Key != null)
					float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				return result;
	        }
		}

        public float Specular
        {
	        get
	        {
		        float result = 1f;
		        KeyValue item = Properties.FirstOrDefault(i => i.Key == "_sp");
		        if (item.Key != null)
			        float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				return result;
	        }
		}

        public float Emit
        {
	        get
	        {
		        float result = 1f;
		        KeyValue item = Properties.FirstOrDefault(i => i.Key == "_emit");
		        if (item.Key != null)
			        float.TryParse(item.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
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
	        return (Properties != null ? Properties.GetHashCode() : 0);
        }
    }
}
