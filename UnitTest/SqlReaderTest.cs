using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using System.Linq;
using Model;
using Moq;

namespace UnitTest
{
    [TestClass]
    public class SqlReaderTest
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

            var dataTable = GetTestSchemaTwoTables();

            Select.SetOrdinal(dataTable, new List<ISprocObjectMap>() {objectMap});

            var result = Select.GetObject<President>(objectMap, moq.Object);

            Assert.AreEqual(5, result.Fans);
            Assert.AreEqual("Donald", result.FirstName);
            Assert.IsTrue(result.IsHonest);
        }

        [TestMethod]
        public void TestOrdinal()
        {
            DataTable schemaTable = GetTestSchemaTwoTables();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            Select.SetOrdinal(schemaTable, list);

            Assert.AreEqual(7, list.ElementAt(1).ColumnOrdinalDic["PresidentId"]);
        }

        [TestMethod]
        public void TestOrdinalForId()
        {
            DataTable schemaTable = GetTestSchemaTwoTables();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            Select.SetOrdinal(schemaTable, list);

            Assert.AreEqual(0, list.ElementAt(0).ColumnOrdinalDic["Id"]);
            Assert.AreEqual(6, list.ElementAt(1).ColumnOrdinalDic["Id"]);
        }

        [TestMethod]
        public void TestOrdinalForCustomMapping()
        {
            DataTable schemaTable = GetTestSchemaTwoTables();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .CustomColumnMapping(x => x.LastName, "Assistant Last Name")
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            Select.SetOrdinal(schemaTable, list);

            Assert.AreEqual(9, list.ElementAt(1).ColumnOrdinalDic["Assistant Last Name"]);
        }

        private DataTable GetTestSchemaTwoTables()
        {
            DataTable tab = new DataTable("Test") { };
            tab.Columns.Add("ColumnName");
            tab.Columns.Add("ColumnOrdinal");

            tab.Rows.Add("Id", 0);
            tab.Rows.Add("FirstName", 1);
            tab.Rows.Add("LastName", 2);
            tab.Rows.Add("Last Name", 3);
            tab.Rows.Add("Fans", 4);
            tab.Rows.Add("IsHonest", 5);

            tab.Rows.Add("Id", 6);
            tab.Rows.Add("PresidentId", 7);
            tab.Rows.Add("FirstName", 8);
            tab.Rows.Add("Assistant Last Name", 9);

            return tab;
        }
    }
}
