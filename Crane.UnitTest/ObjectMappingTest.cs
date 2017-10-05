using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Crane;
using Crane.TestCommon;
using Crane.TestCommon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTest
{
    [TestClass]
    public class ObjectMappingTest
    {
        [TestMethod]
        public void TestGetObject()
        {
            // Arrange
            List<ICraneObjectMap> list = new List<ICraneObjectMap>();
            CraneHelper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());
            var dataTable = DataTableFactory.GetTestDataTable()?.Rows.Cast<DataRow>().ToList();

            var moq = new Mock<IDataReader>();

            moq.Setup(x => x[4]).Returns(5);
            moq.Setup(x => x[1]).Returns("Donald");
            moq.Setup(x => x[2]).Returns("Trump");
            moq.Setup(x => x[0]).Returns(1);
            moq.Setup(x => x[5]).Returns(true);
            
            CraneHelper.SetOrdinal(dataTable, new List<ICraneObjectMap>() { list.ElementAt(0) }, null);

            // Act
            var result = CraneHelper.GetObject<President>(list.ElementAt(0), moq.Object);

            // Assert
            Assert.AreEqual(5, result.Fans);
            Assert.AreEqual("Donald", result.FirstName);
            Assert.IsTrue(result.IsHonest);
        }
    }
}
