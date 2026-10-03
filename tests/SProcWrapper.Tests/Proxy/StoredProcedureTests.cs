using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SProcWrapper.Proxy;

namespace SProcWrapper.Tests.Proxy
{
    [TestFixture]
    public class StoredProcedureTests
    {
        [Test]
        public void Constructor_GeneratesSelect_ForIEnumerable()
        {
            var callOptionsMock = new Mock<ICallOptionsParameters>();
            var sp = new StoredProcedure("TestProc", typeof(IEnumerable<string>), new List<StoredProcedureParameter>(), callOptionsMock.Object);
            
            Assert.That(sp.Query, Does.StartWith("SELECT * FROM TestProc"));
        }

        [Test]
        public void Constructor_GeneratesExecute_ForVoid()
        {
            var callOptionsMock = new Mock<ICallOptionsParameters>();
            var sp = new StoredProcedure("TestProc", typeof(void), new List<StoredProcedureParameter>(), callOptionsMock.Object);
            
            Assert.That(sp.Query, Does.StartWith("EXECUTE PROCEDURE TestProc"));
        }

        [Test]
        public void Constructor_GeneratesExecute_ForPrimitiveType()
        {
            var callOptionsMock = new Mock<ICallOptionsParameters>();
            var sp = new StoredProcedure("TestProc", typeof(int), new List<StoredProcedureParameter>(), callOptionsMock.Object);
            
            Assert.That(sp.Query, Does.StartWith("EXECUTE PROCEDURE TestProc"));
        }
    }
}
