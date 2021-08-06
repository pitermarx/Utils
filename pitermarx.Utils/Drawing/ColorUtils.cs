using System.Drawing;

namespace pitermarx.Drawing
{
    public static class ColorUtils
    {
        public static Color Dark(this Color color)
        {
            HSLColor hsb = color;
            hsb.Luminosity *= 0.7;
            return hsb;
        }

        public static Color DarkDark(this Color color)
        {
            HSLColor hsb = color;
            hsb.Luminosity *= 0.4;
            return hsb;
        }

        public static Color Light(this Color color)
        {
            HSLColor hsb = color;
            hsb.Luminosity *= 1.3;
            return hsb;
        }

        public static Color LightLight(this Color color)
        {
            HSLColor hsb = color;
            hsb.Luminosity *= 1.6;
            return hsb;
        }
    }
}