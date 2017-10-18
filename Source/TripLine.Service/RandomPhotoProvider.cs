using System;
using System.Collections.Generic;
using System.Diagnostics;
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



        private Random _random = new Random((int)(DateTime.Now.Ticks));

        public List<Photo> GetRandomPhotos(List<Photo> photos, int numPhotoWanted = 5, int maxRepickSame=2)
        {
            Debug.Assert(photos.Count > 0);

            List<Photo> picks = new List<Photo>();

            for (var i = 0; i < photos.Count  && picks.Count() < numPhotoWanted; i++)
            {
                photos = photos.OrderBy(p => GetPickedCount(p)).ToList();

                var lowestPickCount = GetPickedCount(photos.First());

                if (lowestPickCount >= maxRepickSame)
                    return picks;  

                var choices = photos.Where(p => GetPickedCount(p) <= lowestPickCount).ToList();
                var selPhoto = choices[_random.Next(0, choices.Count - 1)];

                AddPickedCount(selPhoto);
                picks.Add(selPhoto);

                Console.WriteLine($"Random photo {selPhoto.PhotoUrl}  - num time: {GetPickedCount(selPhoto)}");
            }

            return picks;
        }

    }
}
