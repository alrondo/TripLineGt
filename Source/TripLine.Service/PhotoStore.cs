using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

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


        private List<FileExtendedInfo> _newFilesReceived = new List<FileExtendedInfo>();

        public IEnumerable<FileExtendedInfo> PeekNewFiles()
        {
            var excludedfilesKey = _photoRepo.Content.ExcludedFileKeys;

            var _newFilesReceived = _fileFolder.GetFiles(_photoRepo.Content.LastFileDetectionTime);

            var newFilesToProcess = _newFilesReceived.Where(f => !excludedfilesKey.Any(x => f.FileKey == x)).ToList();

            newFilesToProcess = newFilesToProcess.Where(f => !_photoRepo.Content.Photos.Exists(p => p.FileKey == f.FileKey)).ToList();
            
            return newFilesToProcess;
        }



        public IEnumerable<Photo> GetTravelPhotos()
        {
            return _photoRepo.Content.Photos.Where(p => p.IsTravel);
        }

        public List<Photo> GetPhotos()
        {
            return _photoRepo.Content.Photos;
        }

        public List<Photo> GetPhotosByTrip(int tripId)
        {
            return _photoRepo.Content.Photos.Where(p => p.TripId == tripId).ToList();
        }


        public Photo GetPhoto(int photoId)
        {
            return _photoRepo.Content.Photos.FirstOrDefault(p => p.Id == photoId);
        }

        public List<Photo> GetPhotosAtLocation(int locationId)
        {
            return _photoRepo.Content.Photos.Where(p => p.Location != null &&   p.Location.Id == locationId).ToList();
        }


        public List<Photo> NewTravelPhotos => _photoRepo.Content.Photos
            .Where(p => p.IsTravelCandidate)
            .OrderBy(f => f.Creation).ToList()
            .ToList();

        public int NumNewPhotosFiles => NewImportPhotos?.Count() ?? 0;
        
        public int NumNotImportedPhoto => _photoRepo.Content.Photos
            .Count(p => p.Unclassified && !p.IsTravel && p.IsValid);

        public int NewInvalidPhotoCounts => NewImportPhotos.Count(p => !p.IsValid );

        public int NumNonTravelPhotos => NewImportPhotos.Count(p => p.IsValid && p.Location.Excluded);

        public int TotalRepoTravelPhotos => _photoRepo.Content.Photos
            .Count(p => p.IsTravel && p.IsValid);

        public int TotalRepoInvalid => _photoRepo.Content.Photos
           .Count(p => ! p.IsValid);

        public int TotalRepoUnconfirmed => _photoRepo.Content.Photos
            .Count(p => p.Unclassified );

        public int TotalRepoPhotos => _photoRepo.Content.Photos.Count;


        public IEnumerable<Photo> NewImportPhotos => _photoRepo.Content.Photos.Where(p => p.Unclassified);

        public List<Photo> GetSessionPhoto(int sessionId)
            => _photoRepo.Content.Photos.Where(p => p.SessionId == sessionId).ToList();
        

        public List<Photo> PeakForNewPhotos()
        {
            CreateNewPhotos();

            return NewImportPhotos.ToList();
        }

        public void CreateNewPhotos(bool stopOnFirstTravelPhoto=false)
        {
            var excludedfilesKey = _photoRepo.Content.ExcludedFileKeys;

            DateTime lastFileDetectedTime=DateTime.MinValue;
            foreach (var file in  _fileFolder.GetFiles(_photoRepo.Content.LastFileDetectionTime) )
            {
                lastFileDetectedTime = file.DetectedTime;

                if ( excludedfilesKey.Any(x => file.FileKey == x))
                    continue;

                if (_photoRepo.Content.Photos.Exists(p => p.FileKey == file.FileKey))
                    continue;

                var photo = CreatePhoto(file);
                
                AddPhoto(photo);

                if (stopOnFirstTravelPhoto && photo.IsTravelCandidate)
                    break;
            }

            if(lastFileDetectedTime > _photoRepo.Content.LastFileDetectionTime)
                _photoRepo.Content.LastFileDetectionTime = lastFileDetectedTime;
            _photoRepo.Save();
        }



        public List<PhotoSession>   CreateNewPhotoSessions(bool peakForNewPhotos = true)
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

            IEnumerable<Photo> targetPhotos = _photoRepo.Content.Photos.Where(p => p.SessionId == sessionId).ToList();

            foreach (var photo in targetPhotos)
                SetPinForPhoto(photo, tripId, destinationId, placeId);

            ConfirmedSession.Add(sessionId);

            if(_newFilesReceived.Any())
                _photoRepo.Content.LastFileDetectionTime = _newFilesReceived.Last().DetectedTime;

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

            if (_newFilesReceived.Any())
                _photoRepo.Content.LastFileDetectionTime = _newFilesReceived.Last().DetectedTime;

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


        private Photo _lastBuildedPhoto =null;



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

        public Photo CreatePhoto(FileExtendedInfo finfo)
        {
            var creationDate = GetDateForPhoto(finfo);
            var photo = Photo.NewPhoto(_photoRepo.GetNewId(), finfo.FilePath, finfo.FileKey, creationDate);
            GetLocationForPhoto(finfo, photo);

            photo.DisplayName = (photo.Location != null) ? photo.Location.GetShortDisplay() : "Unknown Place";
            photo.Excluded = photo.Location == null || photo.Location.Excluded || !photo.IsValid;
            photo.FileInfoContent = finfo.Serialize(pretty: true);

            if (photo.IsValid)
                _lastBuildedPhoto = photo;

            return photo;
        }

        private void GetLocationForPhoto(FileExtendedInfo finfo, Photo photo)
        {
            if (finfo.ExifInfo != null && finfo.ExifInfo.GPS_Latitude.HasValue && finfo.ExifInfo.GPS_Longitude.HasValue)
            {
                photo.Position = new GeoPosition(finfo.ExifInfo.GPS_Latitude.Value, finfo.ExifInfo.GPS_Longitude.Value);

                photo.Location = _locationService.GetLocation(photo.Position);

                if (photo.Location != null)
                {
                    photo.DebugInfo += "LocFromPos" + ";";
                    photo.PositionFromGps = true;

                    var place = _locationService.GetNearbyPlace(photo.Position, photo.Location.Id);

                    photo.PlaceId = place?.Id ?? 0;
                }
            }

            if (photo.Location == null && _lastBuildedPhoto?.Location != null
                && (_locationService.GetSearchPath(photo.PhotoUrl) == _locationService.GetSearchPath(_lastBuildedPhoto.PhotoUrl))
                && ((photo.Creation.DayOfYear - _lastBuildedPhoto.Creation.DayOfYear) <= 3)
                )
            {   // No GPS info nut photo has same search path as previous... 
                photo.Location = _lastBuildedPhoto.Location;
                photo.Position = photo.Location.Position;
                photo.PositionFromGps = false;

                photo.DebugInfo += "LocFromPhoto" + _lastBuildedPhoto.Id + ";";
            }

            if (photo.Location == null)
            {
                photo.DebugInfo += "FromPath" + ";";
                photo.Location = _locationService.GetLocation(finfo.RelativePath);

                if (photo.Location != null) photo.DebugInfo += "LocFromPath" + ";";
            }
        }

        private DateTime GetDateForPhoto (FileExtendedInfo finfo)
        {
            var creationDate = DateHelper.LowestDate(finfo.Creation, finfo.LastWriteDateTimeUtc);

            if (!DateHelper.IsValidDate(creationDate))
            {
                creationDate = DateHelper.LowestDate(finfo.LastAccessTimeUtc, finfo.LastAccessTimeUtc); ;
            }
            if (finfo?.ExifInfo.DateTime != null)
            {
                creationDate = DateHelper.LowestDate(creationDate, finfo.ExifInfo.DateTime.Value);
            }
            return creationDate;
        }


        public void AddPhoto(Photo photo)
        {
            _photoRepo.Content.Photos.Add(photo);
        }


    }
}
