using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Dtos
{
    public class VisitedPlace: DtoBase<VisitedPlace>
    {
        public int Id { get; set; }
    
        public IDateRange Date { get; set; }
        public string PlaceName { get; set; }
        public string Summary { get; set; }
        public int LocationId { get; set; }
        public string[] Types { get; set; }
        public string Icon { get; set; }
    }


    public enum HighliteTarget
    {
        Trip,
        Destination,
        Place,
        Photos,
        Location
        //Highlites
        //ObjectList
    }


    public abstract class DtoBase<T>
    {
        public string Serialize(bool pretty=false)
        {
            Formatting formatting = pretty ? Formatting.Indented : Formatting.None;

            string str = JsonConvert.SerializeObject(this, formatting);

            if (!pretty)
            {
                str = str.Replace(@"},", (@"}," + Environment.NewLine));
                str = str.Replace(@"[{,", (Environment.NewLine + @"[{"));
            }

            return str;
        }

        public void DebugWrite()
        {
            string str = JsonConvert.SerializeObject(this, Formatting.Indented);

            Debug.WriteLine(str);
        }
    }

    public class HighliteItem : IHighliteItem
    {
        public HighliteTarget Target { get; set; }

        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string Thumbnail { get; set; }
        public string PhotoUrl { get; set; }
        
        public string Description { get; set; }   
    }


    public class HighliteTopic:DtoBase<HighliteTopic>, IHighliteTopic
    {
        public string DisplayName { get; set; }

        public List<IHighliteItem> Items { get; set; }

        public HighliteTopic(string displayName)
        {
            DisplayName = displayName;
        }

        public HighliteTopic(string displayName, List<IHighliteItem> items)
        {
            DisplayName = displayName;
            Items = items;
        }
    }

}
