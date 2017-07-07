using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.DesktopApp.Models
{
    public enum DestinationType
    {
        Continent,
        SubContinent,
        Country,
        Province,
        Region,
        NaturalPark,
        City,
        AttractionPark
    }
    

    public class CandidateModel
    {
        int Id { get; set; }

        string Name { get; set; }
        int NumPhotos { get; set; }

        string Where { get; set; }
        string When { get; set; }

        DateTime StartDate { get; set; }
        int NumDays { get; set; }

        // int  ImportanceVsSibbling
        // int  ImportanceInTrip
        // int  ImportanceInLibrary
        // bool SelectedForCreation
    }

    public class DestinationCandidateModel : CandidateModel
    {
        public int ParentTripId { get; set; }
        public int ParentDestinationId { get; set; }
        public DestinationType DestinationType { get; set; }
        public int DestinatonId { get; set; }
        public List<int> ChildDestionIds { get; set; }
        public List<int> SessionIds { get; set; }

        public static DestinationCandidateModel CreateDestinationCandidateModel(DestinationCandidate destinationCandidate)
        {
            DestinationCandidateModel model = AutoMapper.Mapper.Map<DestinationCandidateModel>(destinationCandidate);

            return model;
        }
    }


    public class TripCandidateModel : CandidateModel
    {
        int TripId { get; set; }

        public static TripCandidateModel CreateTripCandidateModel(TripCandidate trip)
        {
            TripCandidateModel model = AutoMapper.Mapper.Map<TripCandidateModel>(trip);

            return model;
        }
    }

}
