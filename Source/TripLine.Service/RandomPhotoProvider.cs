using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class RandomPhotoProvider
    {


        private Dictionary<int, int> _allPickedPhotos = new Dictionary<int, int>();

        private int GetPickedCount(Photo photo)
        {
            if (!_allPickedPhotos.ContainsKey(photo.Id))
                return 0;

            return _allPickedPhotos[photo.Id];
        }

        private void AddPickedCount(Photo photo)
        {
            int count = GetPickedCount(photo);

            _allPickedPhotos[photo.Id] = ++count;
        }


        public List<Photo> GetRandomPhotos(List<Photo> photos, int numPhotoWanted = 5)
        {
            var num2Pick = Math.Min(photos.Count, numPhotoWanted);

            List<Photo> picks = new List<Photo>();
            List<Photo> backupPics = new List<Photo>();

            var random = new Random((int)(DateTime.Now.Ticks));

            for (var i = 0; i < photos.Count; i++)
            {
                var selPhoto = photos[random.Next(0, photos.Count - 1)];

                if (GetPickedCount(selPhoto) >= 1)
                {
                    if (GetPickedCount(selPhoto) == 1)
                        backupPics.Add(selPhoto);

                    continue;
                }

                AddPickedCount(selPhoto);
                picks.Add(selPhoto);

                if (picks.Count() >= num2Pick)
                    break;
            }

            return picks;
        }

    }
}
