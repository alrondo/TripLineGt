namespace TripLine.Toolbox.Extensions
{
    public static class SizeExtension
    {
        public static long MB(this long size)
        {
            return 1024*1024*size;
        }
        public static long MB(this int size)
        {
            return 1024 * 1024 * size;
        }
        public static ulong MB(this ulong size)
        {
            return 1024 * 1024 * size;
        }

        public static long GB(this int size)
        {
            return 1024*1024*1024*(long)size;
        }

        public static long KB(this int size)
        {
            return 1024*size;
        }
        public static uint KB(this uint size)
        {
            return 1024*size;
        }
    }
}