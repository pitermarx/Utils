using System.Drawing;

namespace pitermarx.Drawing
{
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        private double hue = 1.0;

        private double saturation = 1.0;
        private double luminosity = 1.0;

        public double Hue
        {
            get => hue * 360;
            set => hue = CheckRange(value / 360);
        }

        public double Saturation
        {
            get => saturation;
            set => saturation = CheckRange(value);
        }

        public double Luminosity
        {
            get => luminosity;
            set => luminosity = CheckRange(value);
        }

        private static double CheckRange(double value) =>
            value < 0 ? 0 :
            value > 1 ? 1 :
            value;

        public override string ToString() => $"H: {Hue:#0.##} S: {Saturation:#0.##} L: {Luminosity:#0.##}";

        public string ToRGBString()
        {
            Color color = this;
            return $"R: {color.R:#0.##} G: {color.G:#0.##} B: {color.B:#0.##}";
        }

        #region Casts to/from System.Drawing.Color

        public static implicit operator HSLColor(Color color) => new(color);

        public static implicit operator Color(HSLColor hslColor)
        {
            if (hslColor.luminosity == 0)
            {
                return Color.FromArgb(0, 0, 0);
            }

            if (hslColor.saturation == 0)
            {
                int v = (int)hslColor.luminosity;
                return Color.FromArgb(v, v, v);
            }

            double temp2 = hslColor.luminosity < 0.5
                ? hslColor.luminosity * (1.0 + hslColor.saturation)
                : hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);

            double temp1 = (2.0 * hslColor.luminosity) - temp2;

            var r = GetColorComponent(temp1, temp2, hslColor.hue + (1.0 / 3.0));
            var g = GetColorComponent(temp1, temp2, hslColor.hue);
            var b = GetColorComponent(temp1, temp2, hslColor.hue - (1.0 / 3.0));
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            return temp3 switch
            {
                < 1.0 / 6.0 => temp1 + (temp2 - temp1) * 6.0 * temp3,
                < 0.5 => temp2,
                < 2.0 / 3.0 => temp1 + (temp2 - temp1) * (2.0 / 3.0 - temp3) * 6.0,
                _ => temp1
            };
        }

        private static double MoveIntoRange(double value) => value switch
        {
            < 0.0 => value + 1.0,
            > 1.0 => value - 1.0,
            _ => value,
        };

        #endregion Casts to/from System.Drawing.Color

        public HSLColor()
        {
        }

        public HSLColor(int red, int green, int blue)
            : this(Color.FromArgb(red, green, blue))
        {
        }

        public HSLColor(Color color)
            : this(color.GetHue(), color.GetSaturation(), color.GetBrightness())
        {
        }

        public HSLColor(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }
    }
}