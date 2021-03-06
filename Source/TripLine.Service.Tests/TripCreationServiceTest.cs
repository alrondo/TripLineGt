// <copyright file="TripCreationServiceTest.cs">Copyright ©  2016</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.Service.Tests
{
    /// <summary>This class contains parameterized unit tests for TripCreationService</summary>
    [PexClass(typeof(TripCreationService))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class TripCreationServiceTest
    {
        /// <summary>Test stub for AddAll()</summary>
        [PexMethod]
        public void AddAllTest([PexAssumeUnderTest]TripCreationService target)
        {
            target.AddAll();
            // TODO: add assertions to method TripCreationServiceTest.AddAllTest(TripCreationService)
        }

        /// <summary>Test stub for .ctor(TripStore, PhotoStore, LocationService)</summary>
        [PexMethod]
        public TripCreationService ConstructorTest(
            TripStore tripStore,
            PhotoStore photoStore,
            LocationService locationService
        )
        {
            TripCreationService target
               = new TripCreationService(tripStore, photoStore, locationService);
            return target;
            // TODO: add assertions to method TripCreationServiceTest.ConstructorTest(TripStore, PhotoStore, LocationService)
        }

        /// <summary>Test stub for DetectNewFiles()</summary>
        [PexMethod]
        public TripCreationDetectResult DetectNewFilesTest([PexAssumeUnderTest]TripCreationService target)
        {
            TripCreationDetectResult result = target.DetectNewFiles();
            return result;
            // TODO: add assertions to method TripCreationServiceTest.DetectNewFilesTest(TripCreationService)
        }

        /// <summary>Test stub for DetectTripsFromNewPhotos()</summary>
        [PexMethod]
        public TripCreationDetectResult DetectTripsFromNewPhotosTest([PexAssumeUnderTest]TripCreationService target)
        {
            TripCreationDetectResult result = target.DetectTripsFromNewPhotos();
            return result;
            // TODO: add assertions to method TripCreationServiceTest.DetectTripsFromNewPhotosTest(TripCreationService)
        }

        /// <summary>Test stub for GetTripCandidate(Int32)</summary>
        [PexMethod]
        public TripCandidate GetTripCandidateTest([PexAssumeUnderTest]TripCreationService target, int index)
        {
            TripCandidate result = target.GetTripCandidate(index);
            return result;
            // TODO: add assertions to method TripCreationServiceTest.GetTripCandidateTest(TripCreationService, Int32)
        }

        /// <summary>Test stub for IsTripSession(PhotoSession)</summary>
        [PexMethod]
        public bool IsTripSessionTest(
            [PexAssumeUnderTest]TripCreationService target,
            PhotoSession session
        )
        {
            bool result = target.IsTripSession(session);
            return result;
            // TODO: add assertions to method TripCreationServiceTest.IsTripSessionTest(TripCreationService, PhotoSession)
        }

        /// <summary>Test stub for RejectAll()</summary>
        [PexMethod]
        public void RejectAllTest([PexAssumeUnderTest]TripCreationService target)
        {
            target.RejectAll();
            // TODO: add assertions to method TripCreationServiceTest.RejectAllTest(TripCreationService)
        }

        /// <summary>Test stub for Stop()</summary>
        [PexMethod]
        public void StopTest([PexAssumeUnderTest]TripCreationService target)
        {
            target.Stop();
            // TODO: add assertions to method TripCreationServiceTest.StopTest(TripCreationService)
        }

        /// <summary>Test stub for get_TripCreationDetectResult()</summary>
        [PexMethod]
        public TripCreationDetectResult TripCreationDetectResultGetTest([PexAssumeUnderTest]TripCreationService target)
        {
            TripCreationDetectResult result = target.TripCreationDetectResult;
            return result;
            // TODO: add assertions to method TripCreationServiceTest.TripCreationDetectResultGetTest(TripCreationService)
        }
    }
}
