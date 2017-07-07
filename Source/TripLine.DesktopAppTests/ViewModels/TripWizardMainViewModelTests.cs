using Microsoft.VisualStudio.TestTools.UnitTesting;
using TripLine.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.WPF.MVVM;
using Tripline.DesktopApp;

namespace TripLine.DesktopApp.ViewModels.Tests
{
    [TestClass()]
    public class TripWizardMainViewModelTests
    {
        private static Bootstrapper _bootstreapper = null;

        TripWizardMainViewModelTests()
        {
            if (_bootstreapper == null)
            {
                _bootstreapper = new Bootstrapper();
                _bootstreapper.Configure(new MvvmConfiguration());
            }

        }

        //[TestMethod()]
        //public void TripWizardMainViewModelTest()
        //{
        //    Assert.Fail();
        //}
    }
}