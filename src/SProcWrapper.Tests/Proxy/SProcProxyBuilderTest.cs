using Moq;
using NUnit.Framework;
using SProcWrapper.Data;
using SProcWrapper.Proxy;

namespace Psbst.Tests.SProcWrapper.Proxy
{
    public class SProcProxyBuilderTest
    {
        private IDataContext _dataContext;
        private IEmptySProcService _underTest;

        [SetUp]
        public void SetUp()
        {
            _dataContext = Mock.Of<IDataContext>();
            _underTest = SProcProxy.Build<IEmptySProcService>(_dataContext);
        }

        [Test]
        public void HashCode_OnDynamicProxyWorks()
        {
            Assert.That(_underTest.GetHashCode(), Is.Not.EqualTo(0), "GetHashCode() on dynamic proxy returned 0");
        }


        [Test]
        public void Equals_OnDynamicProxyWorks()
        {
            Assert.That(_underTest.Equals(_underTest), "proxy.Equals(proxy) is false");
        }

        [Test]
        public void ToString_OnDynamicProxyWorks()
        {
            Assert.That(_underTest.ToString(), Is.Not.EqualTo(null), "ToString() on dynamic proxy returned null");
        }


        public interface IEmptySProcService
        {
        }
    }
}