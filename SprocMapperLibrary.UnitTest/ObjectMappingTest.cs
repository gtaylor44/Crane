using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Moq;
using SprocMapperLibrary;
using SprocMapperLibrary.TestCommon;
using SprocMapperLibrary.Core;

namespace UnitTest
{
    [TestClass]
    public class ObjectMappingTest
    {
        [TestMethod]
        public void TestGetObject()
        {
            var moq = new Mock<IDataReader>();

            moq.Setup(x => x[4]).Returns(5);
            moq.Setup(x => x[1]).Returns("Donald");
            moq.Setup(x => x[2]).Returns("Trump");
            moq.Setup(x => x[0]).Returns(1);
            moq.Setup(x => x[5]).Returns(true);

            var objectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var dataTable = DataTableFactory.GetTestDataTable();

            SprocMapper.SetOrdinal(dataTable, new List<ISprocObjectMap>() { objectMap }, null);

            var result = SprocMapper.GetObject<President>(objectMap, moq.Object);

            Assert.AreEqual(5, result.Fans);
            Assert.AreEqual("Donald", result.FirstName);
            Assert.IsTrue(result.IsHonest);
        }





    }
}
