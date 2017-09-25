using TripLine.Dtos;

namespace TripLine.Service
{
    public class HighliteSelectOptions
    {

        public bool Random { get; set; } = true;

        public int?  MaxNumberOfTarget { get; set; }

        public HighliteTarget? Target { get; set; } = null;

        //order by & and other stuff


        public HighliteSelectOptions(HighliteTarget target)
        {
            Target = target;
        }

        public HighliteSelectOptions()
        {
            Target = null;
        }

    }



    public enum TitleSource
    {
        Default,
        TripName,
        LocationName,
        PhotoName,
        FileName
    }
}