using System;
using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SProcWrapper.Data;

namespace SProcWrapper.Tests.Data
{
    [TestFixture]
    public class DataContextTests
    {
        [Test]
        public void Dispose_RollsBackAndDisposesTransaction()
        {
            var transactionMock = new Mock<DbTransaction>();
            
            var connectionMock = new Mock<DbConnection>();
            connectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>()).Returns(transactionMock.Object);
            connectionMock.Setup(c => c.State).Returns(ConnectionState.Closed);
            
            var factoryMock = new Mock<DbProviderFactory>();
            factoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            
            var dataContext = new DataContext("Server=myServerAddress;Database=myDataBase;", factoryMock.Object);
            
            dataContext.BeginTransaction(); // this calls OpenConnection and BeginTransaction on connection
            dataContext.Dispose();
            
            transactionMock.Verify(t => t.Rollback(), Times.Once);
            transactionMock.Protected().Verify("Dispose", Times.Once(), true);
        }

        [Test]
        public void OpenConnection_ThrowsArgumentOutOfRange_WhenStateIsUnexpected()
        {
            var connectionMock = new Mock<DbConnection>();
            connectionMock.Setup(c => c.State).Returns(ConnectionState.Executing); // An unexpected state
            
            var factoryMock = new Mock<DbProviderFactory>();
            factoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            
            var dataContext = new DataContext("Server=myServerAddress;Database=myDataBase;", factoryMock.Object);
            
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => dataContext.BeginTransaction());
            Assert.That(ex.ParamName, Is.EqualTo("State"));
        }
    }
}
