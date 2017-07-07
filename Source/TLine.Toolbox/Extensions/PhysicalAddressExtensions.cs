using System.Net.NetworkInformation;

namespace TripLine.Toolbox.Extensions
{
    public static class PhysicalAddressExtensions
    {
        public static string ToStringWithDash(this PhysicalAddress address)
        {
            var bytes = address.GetAddressBytes();
            return string.Format("{0:X2}-{1:X2}-{2:X2}-{3:X2}-{4:X2}-{5:X2}", bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]);
        }
    }
}
