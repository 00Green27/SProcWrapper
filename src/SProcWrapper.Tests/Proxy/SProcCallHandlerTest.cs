using System;
using System.Linq;
using NUnit.Framework;
using SProcWrapper;
using SProcWrapper.Proxy;

namespace Psbst.Tests.SProcWrapper.Proxy
{
    [TestFixture]
    public class SProcCallHandlerTest
    {
        public interface ISprocCallSample
        {
            [SProcCall("X")]
            void A([SProcParam("var")] string dummyParam);

            [SProcCall("vAllDetail")]
            void B();
        }

        public interface ISprocSampleWithEmptyName
        {
            [SProcCall("")]
            void EmptyName();
        }

        public interface ISprocSampleWithNullName
        {
            [SProcCall(null)]
            void NullName();
        }

        [SProcService(Namespace = "NS")]
        public interface ISprocServiceSample
        {
        }

        [Test]
        public void FindSProcCallAnnotatedMethods_ShouldFindAllMethodsWithSprocCallAttribute()
        {
            var sProcCallAnnotatedMethods =
                SProcAttributesHandler.FindSProcCallAnnotatedMethods(typeof(ISprocCallSample));
            Assert.AreEqual(sProcCallAnnotatedMethods.Count(), 2);
        }

        [Test]
        public void Handle_ShouldCreateStoredProcedureWithNameX()
        {
            var annotatedMethods =
                SProcAttributesHandler.FindSProcCallAnnotatedMethods(typeof(ISprocCallSample)).ToArray();
            var handle = SProcAttributesHandler.Handle(typeof(ISprocCallSample));

            Assert.AreEqual("X", handle[annotatedMethods.First().methodInfo].Name);
        }

        [Test]
        public void Handle_ShouldThrowsExceptionWhenHandleStoredProcedureWithEmptyName()
        {
            Assert.Throws<ArgumentException>(() => SProcAttributesHandler.Handle(typeof(ISprocSampleWithEmptyName)));
        }

        [Test]
        public void Handle_ShouldThrowsExceptionWhenHandleStoredProcedureNullName()
        {
            Assert.Throws<ArgumentException>(() =>
                SProcAttributesHandler.Handle(typeof(ISprocSampleWithNullName)));
        }

        [Test]
        public void Handle_ShouldCreateStoredprocedureWithParameter()
        {
            var annotatedMethods = SProcAttributesHandler.FindSProcCallAnnotatedMethods(typeof(ISprocCallSample));
            var handle = SProcAttributesHandler.Handle(typeof(ISprocCallSample));

            var storedProcedure = handle[annotatedMethods.First().methodInfo];

            var sqlParameterList = storedProcedure.GetSqlParameterList();
            Assert.AreEqual(sqlParameterList, "@dummyParam");
        }

        [Test]
        public void Handle_ShouldCreateStoredprocedureWithParametrizedParameter()
        {
        }

        [Test]
        public void Handle_ShouldReturnEmptyMapWhenThereIsNoAnnotatedMethod()
        {
            var result = SProcAttributesHandler.Handle(typeof(string));
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void Handle_ShouldDefaultHandlerWhenClassNotHaveSprocServiceAttribute()
        {
            var result = SProcAttributesHandler.SprocServiceAttributeHandle(typeof(string));
            Assert.AreEqual(result, SProcAttributesHandler.DefaultPrefix);
        }

        [Test]
        public void Handle_ShouldSetPrefixWhenExist()
        {
            var result = SProcAttributesHandler.SprocServiceAttributeHandle(typeof(ISprocServiceSample));
            Assert.AreEqual(result, "NS_");
        }
    }
}