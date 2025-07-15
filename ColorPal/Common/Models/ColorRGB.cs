namespace ColorPal.Common.Models
{
    public class ColorRGB
    {
        public ColorRGB() { }

        public ColorRGB(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}
