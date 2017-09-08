using System;
using AutoMapper;
using TinyIoC;
using TripLine.Dtos;
using TripLine.WPF.MVVM;

namespace TripLine.Service
{
    public static class ServiceMapper
    {
        public static void RegisterObjects(TinyIoCContainer ioc)
        {
            ioc.Register<PlaceRepo>().AsSingleton();
            ioc.Register<LocalFileRepo>().AsSingleton();
            ioc.Register<LocationRepo>().AsSingleton();
            ioc.Register<LocationService>().AsSingleton();
            ioc.Register<PhotoRepo>().AsSingleton();
            ioc.Register<TripsRepo>().AsSingleton();

            ioc.Register<LocalFileFolders>().AsSingleton();
            ioc.Register<PhotoStore>().AsSingleton();
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

    public static class ServiceBootStrapper
    {
        public static void Configure()
        {
            var config = new MvvmConfiguration();

            //base.Configure(config);

            ServiceMapper.RegisterObjects(config.IoC);

            Mapper.Initialize(
                cfg =>
                {
                    ServiceMapper.ConfigureMapper(cfg);
                });

        }
    }
}