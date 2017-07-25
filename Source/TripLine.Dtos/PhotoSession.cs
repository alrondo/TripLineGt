using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Dtos
{

    public class PhotoSession : IDateRange
    {
        public int SessionId { get; set; }

        private string _displayName = string.Empty;

        public string DisplayName
        {
            get
            {
                return (_displayName != string.Empty) ? _displayName : Location?.DisplayName ?? "Unknown";
            }
            set
            {
                _displayName = value;
            }
        }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public TimeSpan Duration => (ToDate - FromDate);

        public Location Location { get; set; }
        public int NumPhotos { get; set; }
        public int Score { get; set; }

        public PhotoSession(int sessionId)
        {
            SessionId = sessionId;
        }
        public PhotoSession()
        {

        }

        public string ElapsedFrom => $"{FromDate.ToString(DtoDefs.DateFormatter)}, {(int) Duration.TotalDays,2} days";
        public string Describe => $"{SessionId,3}: {ElapsedFrom} Photo session at {DisplayName} => {NumPhotos} photos - {Score}% ";
    }

}
