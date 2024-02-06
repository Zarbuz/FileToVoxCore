using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FileToVoxCore.Drawing
{
	[Serializable]
    public readonly struct Color : IEquatable<Color>
    {
        public static readonly Color Empty;

       //
        //  end "web" colors
        // -------------------------------------------------------------------

        // NOTE : The "zero" pattern (all members being 0) must represent
        //      : "not set". This allows "Color c;" to be correct.

        private const short StateKnownColorValid = 0x0001;
        private const short StateARGBValueValid = 0x0002;
        private const short StateValueMask = StateARGBValueValid;
        private const short StateNameValid = 0x0008;
        private const long NotDefinedValue = 0;

        // Shift counts and bit masks for A, R, G, B components in ARGB mode

        internal const int ARGBAlphaShift = 24;
        internal const int ARGBRedShift = 16;
        internal const int ARGBGreenShift = 8;
        internal const int ARGBBlueShift = 0;
        internal const uint ARGBAlphaMask = 0xFFu << ARGBAlphaShift;
        internal const uint ARGBRedMask = 0xFFu << ARGBRedShift;
        internal const uint ARGBGreenMask = 0xFFu << ARGBGreenShift;
        internal const uint ARGBBlueMask = 0xFFu << ARGBBlueShift;

        // User supplied name of color. Will not be filled in if
        // we map to a "knowncolor"
        private readonly string name; // Do not rename (binary serialization)

        // Standard 32bit sRGB (ARGB)
        private readonly long value; // Do not rename (binary serialization)


        // State flags.
        private readonly short state; // Do not rename (binary serialization)

        private Color(long value, short state, string name)
        {
            this.value = value;
            this.state = state;
            this.name = name;
        }

        public byte R => unchecked((byte)(Value >> ARGBRedShift));

        public byte G => unchecked((byte)(Value >> ARGBGreenShift));

        public byte B => unchecked((byte)(Value >> ARGBBlueShift));

        public byte A => unchecked((byte)(Value >> ARGBAlphaShift));

        public bool IsEmpty => state == 0;

        // Used for the [DebuggerDisplay]. Inlining in the attribute is possible, but
        // against best practices as the current project language parses the string with
        // language specific heuristics.

        private string NameAndARGBValue => $"{{Name={Name}, ARGB=({A}, {R}, {G}, {B})}}";

        public string Name
        {
            get
            {
                if ((state & StateNameValid) != 0)
                {
                    Debug.Assert(name != null);
                    return name;
                }
                // if we reached here, just encode the value
                //
                return Convert.ToString(value, 16);
            }
        }

        private long Value
        {
            get
            {
                if ((state & StateValueMask) != 0)
                {
                    return value;
                }

                return NotDefinedValue;
            }
        }

        private static void CheckByte(int value, string name)
        {
            //static void ThrowOutOfByteRange(int v, string n) =>
            //    //throw new ArgumentException(SR.Format(SR.InvalidEx2BoundArgument, n, v, byte.MinValue, byte.MaxValue));

            //if (unchecked((uint)value) > byte.MaxValue)
            //    ThrowOutOfByteRange(value, name);
        }

        private static Color FromArgb(uint argb) => new Color(argb, StateARGBValueValid, null);

        public static Color FromArgb(int argb) => FromArgb(unchecked((uint)argb));

        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            CheckByte(alpha, nameof(alpha));
            CheckByte(red, nameof(red));
            CheckByte(green, nameof(green));
            CheckByte(blue, nameof(blue));

            return FromArgb(
                (uint)alpha << ARGBAlphaShift |
                (uint)red << ARGBRedShift |
                (uint)green << ARGBGreenShift |
                (uint)blue << ARGBBlueShift
            );
        }

        public static Color FromArgb(int alpha, Color baseColor)
        {
            CheckByte(alpha, nameof(alpha));

            return FromArgb(
                (uint)alpha << ARGBAlphaShift |
                (uint)baseColor.Value & ~ARGBAlphaMask
            );
        }

        public static Color FromArgb(int red, int green, int blue) => FromArgb(byte.MaxValue, red, green, blue);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetRgbValues(out int r, out int g, out int b)
        {
            uint value = (uint)Value;
            r = (int)(value & ARGBRedMask) >> ARGBRedShift;
            g = (int)(value & ARGBGreenMask) >> ARGBGreenShift;
            b = (int)(value & ARGBBlueMask) >> ARGBBlueShift;
        }

        public float GetBrightness()
        {
            GetRgbValues(out int r, out int g, out int b);

            int min = Math.Min(Math.Min(r, g), b);
            int max = Math.Max(Math.Max(r, g), b);

            return (max + min) / (byte.MaxValue * 2f);
        }

        public float GetHue()
        {
            GetRgbValues(out int r, out int g, out int b);

            if (r == g && g == b)
                return 0f;

            int min = Math.Min(Math.Min(r, g), b);
            int max = Math.Max(Math.Max(r, g), b);

            float delta = max - min;
            float hue;

            if (r == max)
                hue = (g - b) / delta;
            else if (g == max)
                hue = (b - r) / delta + 2f;
            else
                hue = (r - g) / delta + 4f;

            hue *= 60f;
            if (hue < 0f)
                hue += 360f;

            return hue;
        }

        public float GetSaturation()
        {
            GetRgbValues(out int r, out int g, out int b);

            if (r == g && g == b)
                return 0f;

            int min = Math.Min(Math.Min(r, g), b);
            int max = Math.Max(Math.Max(r, g), b);

            int div = max + min;
            if (div > byte.MaxValue)
                div = byte.MaxValue * 2 - max - min;

            return (max - min) / (float)div;
        }

        public int ToArgb() => unchecked((int)Value);

        public override string ToString()
        {
            if ((state & StateValueMask) != 0)
            {
                return nameof(Color) + " [A=" + A.ToString() + ", R=" + R.ToString() + ", G=" + G.ToString() + ", B=" + B.ToString() + "]";
            }
            else
            {
                return nameof(Color) + " [Empty]";
            }
        }

        public static bool operator ==(Color left, Color right) =>
            left.value == right.value
                && left.state == right.state
                && left.name == right.name;

        public static bool operator !=(Color left, Color right) => !(left == right);

        public override bool Equals(object obj) => obj is Color other && Equals(other);

        public bool Equals(Color other) => this == other;

        public override int GetHashCode()
        {
            // Three cases:
            // 1. We don't have a name. All relevant data, including this fact, is in the remaining fields.
            // 2. We have a known name. The name will be the same instance of any other with the same
            // knownColor value, so we can ignore it for hashing. Note this also hashes different to
            // an unnamed color with the same ARGB value.
            // 3. Have an unknown name. Will differ from other unknown-named colors only by name, so we
            // can usefully use the names hash code alone.
            if (name != null)
                return name.GetHashCode();

            unchecked
            {
	            int hash = 17;
	            hash = hash * 31 + value.GetHashCode();
	            hash = hash * 31 + state.GetHashCode();
	            return hash;
            }
        }
    }
}
