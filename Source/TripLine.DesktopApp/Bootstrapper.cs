using System;
using System.Windows;
using System.Windows.Media;

using TripLine.Toolbox.Extensions;
using TripLine.WPF.MVVM;
using TripLine.DesktopApp;
using TripLine.DesktopApp.ViewModels;
//using TripLine.Toolbox.Extensions.Mapping;
using AutoMapper;
using TripLine.Dtos;
using TripLine.DesktopApp.Models;
using TripLine.Service;

namespace Tripline.DesktopApp
{

   


    public class Bootstrapper : MvvmBootstrap
    {
        public void Register(MainViewModel mainViewModel)
        {
            base.Config.IoC.Register<MainViewModel>(mainViewModel).AsSingleton();

        }




        public override void Configure(MvvmConfiguration config)
        {
            base.Configure(config);

            ServiceMapper.RegisterObjects(config.IoC);
           
            Mapper.Initialize(
                cfg =>
                {
                    ServiceMapper.ConfigureMapper(cfg);
                    ConfigureMapper(cfg);
                });

        }


        public static void ConfigureMapper(IMapperConfigurationExpression config)
        {
            config.CreateMap<TripCandidateModel, TripCandidate>();
            config.CreateMap<DestinationCandidateModel, DestinationCandidate>();
            config.CreateMap<PhotoSessionModel, PhotoSession>();
            config.CreateMap<HighliteTopicViewModel, HighliteTopic>();
            config.CreateMap<HighliteTopic, HighliteTopicViewModel>();

            config.CreateMap<HighliteItemViewModel, IHighliteItem>();
            config.CreateMap<IHighliteItem, HighliteItemViewModel>();
           
            ServiceMapper.ConfigureMapper(config);

            // to Album
            config.CreateMap<HighliteTopic, AlbumSectionViewModel>();
            config.CreateMap<Destination, AlbumSectionViewModel>();
            config.CreateMap<Location, AlbumSectionViewModel>();

            config.CreateMap<IHighliteItem, AlbumItemViewModel>();
            config.CreateMap<Photo, AlbumItemViewModel>();

        }
    }
}