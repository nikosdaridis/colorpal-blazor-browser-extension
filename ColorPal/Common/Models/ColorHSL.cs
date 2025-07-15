namespace ColorPal.Common.Models
{
    public class ColorHSL
    {
        public ColorHSL() { }

        public ColorHSL(int h, int s, int l)
        {
            H = h;
            S = s;
            L = l;
        }

        public int H { get; set; }
        public int S { get; set; }
        public int L { get; set; }
    }
}
