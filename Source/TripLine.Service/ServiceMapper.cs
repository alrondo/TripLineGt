using System;
using AutoMapper;
using TinyIoC;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class ServiceMapper
    {
        public static void RegisterObjects(TinyIoCContainer ioc)
        {
            ioc.Register<LocationRepo>().AsSingleton();
            ioc.Register<LocationService>().AsSingleton();

            ioc.Register<LocalFileFolders>().AsSingleton();
            ioc.Register<PhotoStore>().AsSingleton();
            ioc.Register<PhotoRepo>().AsSingleton();
            ioc.Register<TripsRepo>().AsSingleton();
            ioc.Register<TripCreationService>().AsSingleton();

            ioc.Register<TripSmartBuilder>().AsSingleton();
        }

        public static void ConfigureMapper(IMapperConfigurationExpression config)
        {
            config.CreateMap<TripCandidate, Trip>();
            config.CreateMap<DestinationCandidate, Destination>();

        }

        public static TDestination Map<TDestination>(object source)
        {
            try
            {
                return Mapper.Instance.Map<TDestination>(source);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}