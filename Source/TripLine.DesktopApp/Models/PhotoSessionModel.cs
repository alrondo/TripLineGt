using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;

namespace TripLine.DesktopApp.Models
{
  
    public class PhotoSessionModel : PhotoSession
    {
        PhotoSessionModel CreatePhotoSessionModel(PhotoSession session)
        {
            PhotoSessionModel model = AutoMapper.Mapper.Map<PhotoSessionModel>(session);

            return model;
        }
    }
}
