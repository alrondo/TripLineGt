namespace TripLine.Toolbox.Extensions.Mapping
{
    public static class MapperExt
    {
        public static Mapper Mapper(this object obj)
        {
            return new Mapper(obj);
        }
    }
}