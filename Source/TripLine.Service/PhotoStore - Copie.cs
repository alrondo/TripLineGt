using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class PhotoStore
    {
        private readonly LocalFileFolders _fileFolder;

        private readonly LocationService _locationService;

        private readonly PhotoRepo _photoRepo;


        private IEnumerable<PhotoSession> GetUntaggedPhotosSessions()
        {
            throw new NotImplementedException();
        }


        public PhotoStore(PhotoRepo photoRepo, LocalFileFolders fileFolder, LocationService locationService)
        {
            _fileFolder = fileFolder;
            _locationService = locationService;
            _photoRepo = photoRepo;
        }

        public void PinPhotos(int sessionId, int tripId, int destinationId)
        {
            throw new NotImplementedException();
        }



        public List<FileExtendedInfo> NewPhotoFiles = new List<FileExtendedInfo>();

        public void LoadNewFiles()
        {
            NewPhotoFiles.Clear();

            var excludedfilesKey = _photoRepo.Content.ExcludedFileKeys;

            var files = _fileFolder.GetNewFiles().ToList();

            files = files.Where(f => ! excludedfilesKey.Exists(x => f.FileKey == x)).ToList();

            NewPhotoFiles = files.Where(f => !_photoRepo.Content.Photos.Exists(p => p.FileKey == f.FileKey)).ToList();
        }

        public void ReleaseNew()
        {
            NewPhotoFiles.Clear();
            NewTravelPhotos.Clear();
        }

        public List<Photo> GetPhotos()
        {
            return _photoRepo.Content.Photos;
        }

        public List<Photo> GetPhotosByTrip(int tripId)
        {
            return _photoRepo.Content.Photos.Where(p => p.TripId == tripId).ToList();
        }

		public List<Photo> GetPhotosAtLocation(int locationId)
        {
            return _photoRepo.Content.Photos.Where(p => p.Location.Id == locationId).ToList();
        }

        public void CreateNewPhotos(List<FileExtendedInfo> files)
        {
            files.ForEach(f => CreatePhoto(f));

            foreach (var photo in NewPhotosNotAddedToLibrary)
            {
                _photoRepo.Content.ExcludedFileKeys.Add(photo.FileKey);
            }
            
        }

        public List<Photo> NewTravelPhotos => _photoRepo.Content.Photos
            .Where(p => p.Unclassified && p.IsValid  && ! p.Location.Excluded)
            .OrderBy(f => f.Creation).ToList()
            .ToList();



        public int NumNewPhotosFiles => NewPhotoFiles.Count;
        
        public int NumNotImportedPhoto => NewPhotosNotAddedToLibrary.Count;

        public int NumInvalidPhotos => NewPhotosNotAddedToLibrary
            .Count(p => !p.IsValid );

        public int NumNonTravelPhotos => NewPhotosNotAddedToLibrary
            .Count(p => p.IsValid && p.Location.Excluded);

        public int TotalRepoTravelPhotos => _photoRepo.Content.Photos
            .Count(p => p.IsTravel && p.IsValid);

        public int TotalRepoInvalid => _photoRepo.Content.Photos
           .Count(p => ! p.IsValid);

        public int TotalRepoUnconfirmed => _photoRepo.Content.Photos
            .Count(p => p.Unclassified );

        public int TotalRepoPhotos => _photoRepo.Content.Photos.Count;
   

        public List<Photo> NewPhotosNotAddedToLibrary { get; } = new List<Photo>();

        public List<Photo> GetSessionPhoto(int sessionId)
            => _photoRepo.Content.Photos.Where(p => p.SessionId == sessionId).ToList();



        public List<Photo> PeakForNewPhotos()
        {
            //if (NewTravelPhotos.Count != 0)
            //    throw new InvalidOperationException("Bad state");  // already loaded

            LoadNewFiles();

            if (NewPhotoFiles.Count == 0)
                return new List<Photo>();

            CreateNewPhotos(NewPhotoFiles);
            var photos = NewTravelPhotos;

            ReleaseNew();
            return photos;
        }

        public void LoadNewPhotos()
        {
            if (NewTravelPhotos.Count != 0)
                return;  // already loaded

            LoadNewFiles();

            if (NewPhotoFiles.Count == 0)
                return;

            CreateNewPhotos(NewPhotoFiles);

            _photoRepo.Save();
        }



        public List<PhotoSession>   GetNewSessions(bool peakForNewPhotos = true)
        {
            LoadNewPhotos();
            
            return GetSessionsFromNewPhotos();
        }

        public List<PhotoSession> GetSessionsFromNewPhotos()
        {
            List<Photo> newPhotos = NewTravelPhotos.OrderBy(p => p.Creation).Where(p => p.Unclassified).ToList();

            var sessions = CreatePhotoSessions(newPhotos.Where(p => p.Location != null).ToList());

            sessions = sessions.OrderBy(s => s.FromDate).ToList();

            _photoRepo.Save();
            _locationService.SaveLocations();

            return sessions;
        }



        public List<int> ConfirmedSession = new List<int>();

        public void ConfirmPhotoSession(int sessionId, int? tripId, int? destinationId = null, int? placeId = null)
        {

            // p => p.Unclassified() &
            IEnumerable<Photo> targetPhotos = _photoRepo.Content.Photos.Where(p => p.SessionId == sessionId).ToList();

            foreach (var photo in targetPhotos)
                SetPinForPhoto(photo, tripId, destinationId, placeId);

            ConfirmedSession.Add(sessionId);

            _photoRepo.Save();
        }

        public void RejectAllUnconfirmed()
        {
            List<Photo> unconfirmed = (_photoRepo.Content.Photos.Where(p => p.Unclassified )).ToList();

            List<int> unconfirmedPhotoIds = unconfirmed.Select(ph => ph.Id).ToList();

            foreach (int photoId in unconfirmedPhotoIds )
            {
                var photo = _photoRepo.Content.Photos.FirstOrDefault(p => p.Id == photoId);
                _photoRepo.Content.Photos.Remove(photo);

                // ADD
                _photoRepo.Content.ExcludedFileKeys.Add(photo.FileKey);

            }
    
            _photoRepo.Save();
        }

     


        private void SetPinForPhoto(Photo photo, int? tripId, int? destinationId = 0, int? placeId = 0)
        {
            if (tripId.HasValue)
                photo.TripId = tripId.Value;

            if (destinationId.HasValue)
                photo.DestId = destinationId.Value;

            if (placeId.HasValue)
                photo.PlaceId = placeId.Value;

        }


        private bool IsUnique(List<int> allids, int id)
        {
            if (allids.Contains(id))
                return false;

            allids.Add(id);
            return true;
        }
        
        private List<PhotoSession>  CreatePhotoSessions(List<Photo> photos)
        {
            List<PhotoSession> sessions = new List<PhotoSession>();

            List<int> processedLocations = new List<int>();

            photos = photos.OrderBy(p => p.Creation).ToList();

            List<int> locationIds = photos
                    .Where(x => x.Location != null )     // && IsUnique(locationids, x.Location.Id))
                    .Select(q => q.Location.Id).ToList();

            foreach ( var locId in locationIds )
            {
                if (processedLocations.Contains(locId))
                    continue;

                var photosAtThisLocation = photos.Where( p => p.Location != null &&  p.Location.Id == locId).ToList();

                while (photosAtThisLocation.Any())
                {
                    var session = CreateOneSessionFromTop(photosAtThisLocation, maxHourBeforeEndOfSession: 48);
                    sessions.Add(session);
                    photosAtThisLocation = photosAtThisLocation.Skip(session.NumPhotos).ToList();
                }

                processedLocations.Add(locId);
            }

            return sessions;
        }


        private PhotoSession CreateOneSessionFromTop(IEnumerable<Photo> photos, int maxHourBeforeEndOfSession)
        {
            if (photos == null || photos.Count() == 0)
                return null;

            PhotoSession session = CreateSessionWithPhoto(photos.First());

            foreach (var photo in photos)
            {
                if ((photo.Creation - session.ToDate).TotalHours > maxHourBeforeEndOfSession)
                    break;  // remaining photo will be part of another session maybe!

                AddPhotoToSession(session, photo);
            }

            //Debug.WriteLine($"New session {session.Describe }");

            return session;
        }

        private PhotoSession CreateSessionWithPhoto(Photo photo)
        {
            PhotoSession newSession = new PhotoSession(_photoRepo.Content.NewSessionId++);
            newSession.FromDate = photo.Creation;
            newSession.ToDate = photo.Creation;
            newSession.NumPhotos = 1;

            newSession.Location = photo.Location;
            newSession.DisplayName = $"{photo?.Location.DisplayName}";
            return newSession;
        }

        private static void AddPhotoToSession(PhotoSession session, Photo photo)
        {
            // tag photo into session
            photo.SessionId = session.SessionId;

            session.NumPhotos += 1;
            session.ToDate = photo.Creation;
        }


        private Photo _lastPhoto =null;



        public Location FindPhotoLocation(FileExtendedInfo finfo)
        {
            Location location = null;

            if (finfo.ExifInfo == null)
                return null;

            if (finfo.ExifInfo.GPS_Latitude.HasValue && finfo.ExifInfo.GPS_Longitude.HasValue)
            {
                var position = new GeoPosition(finfo.ExifInfo.GPS_Latitude.Value, finfo.ExifInfo.GPS_Longitude.Value);

                location =  _locationService.GetLocation(position);
            }

            if (location == null)
            {
                location = _locationService.GetLocation(finfo.RelativePath);
            }

            return location;
        }

        public void CreatePhoto(FileExtendedInfo finfo)
        {
            var creationDate = DateHelper.LowestDate(finfo.Creation, finfo.LastWriteDateTimeUtc);

            if (!DateHelper.IsValidDate(creationDate))
            {
                creationDate = finfo.LastAccessTimeUtc;

                // todo try get date from filename
            }

            var photo = Photo.NewPhoto(_photoRepo.Content.NewId++, finfo.FilePath, finfo.FileKey, creationDate);

            if (finfo.ExifInfo != null )
            {
                if ( finfo.ExifInfo.GPS_Latitude.HasValue && finfo.ExifInfo.GPS_Longitude.HasValue )
                {
                    photo.Position = new GeoPosition( finfo.ExifInfo.GPS_Latitude.Value, finfo.ExifInfo.GPS_Longitude.Value);

                    photo.Location = _locationService.GetLocation(photo.Position);
                }

                if (finfo.ExifInfo.DateTime.HasValue)
                    photo.ExifDate = finfo.ExifInfo.DateTime.Value;
            }

            if (_lastPhoto == null || (_lastPhoto.Creation.DayOfYear != finfo.Creation.DayOfYear))
            {
                //Debug.WriteLine($"New Photo Date: {finfo.Creation.ToShortDateString()} {finfo.LastWriteDateTimeUtc} ");
                //Debug.WriteLine($"{JsonConvert.SerializeObject(finfo)} ");
            }


            if (   photo.Location == null  && _lastPhoto?.Location != null
                && ( _locationService.GetSearchPath(photo.PhotoUrl)  == _locationService.GetSearchPath(_lastPhoto.PhotoUrl)) 
                && ( (photo.Creation.DayOfYear - _lastPhoto.Creation.DayOfYear) <= 3) 
                )               
            {   // No GPS info nut photo has same search path as previous... 
                photo.Location = _lastPhoto.Location;
            }

            if (photo.Location == null   )
            {   
                photo.Location = _locationService.GetLocation(finfo.RelativePath);
            }
 
            _lastPhoto = photo;
            if (photo.IsValid  && !photo.Location.Excluded )
                _photoRepo.Content.Photos.Add(photo);
            else
            {
                NewPhotosNotAddedToLibrary.Add(photo);    
            }
        }



    }
}
