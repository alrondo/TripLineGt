using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Dtos
{
    public static class TripLineConfig
    {
        public static string BaseDocument { get; } = "C:\\TripLine\\";

        public static string PictureFolderPath = "c:\\Photos";

        public static string TripRepoPath = BaseDocument + "Repos\\TripRepo.txt";

        public static string PlaceRepoPath = BaseDocument + "Repos\\PlaceRepo.txt";

        public static string PhotoRepoPath = BaseDocument + "Repos\\PhotoRepo.txt";

        public static string FileInfoRepoPath = BaseDocument + "Repos\\FileInfoRepo.txt";

        public static string LocationRepoPath = BaseDocument + "Repos\\LocationRepo.txt";

        public static string TestLocationRepoPath = BaseDocument + "Repos\\TestLocationRepo.txt";
        public static string TestPlaceRepoPath = BaseDocument + "Repos\\TestPlaceRepo.txt";

    }
}
