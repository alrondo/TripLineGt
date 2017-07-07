namespace TripLine.WPF.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class MvvmBootstrap
    {

        public MvvmConfiguration Config { get; private set; }

        public virtual IEnumerable<Assembly> LoadAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }



        public virtual void Configure(MvvmConfiguration config)
        {
            Config = config;
        }



    }
}