using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Model;
using Moq;

namespace UnitTest
{
    [TestClass]
    public class SqlReaderTest
    {

        [TestMethod]
        public void TestMethod3()
        {
            HashSet<string> columns = new HashSet<string>() {"Fans",  "FirstName", "IsHonest"};
 
            var moq = new Mock<IDataReader>();

            moq.Setup(x => x["Fans"]).Returns(5);
            moq.Setup(x => x["FirstName"]).Returns("Donald");
            moq.Setup(x => x["LastName"]).Returns("Trump");
            moq.Setup(x => x["IsHonest"]).Returns(true);
            moq.Setup(x => x["Id"]).Returns(1);

            var objectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var result = SprocMapperHelper.GetObject<President>(objectMap, moq.Object);

            Assert.AreEqual(5, result.Fans);
            Assert.AreEqual("Donald", result.FirstName);
            Assert.AreEqual(true, result.IsHonest);
        }

        [TestMethod]
        public void TestOrdinal()
        {
            DataTable schemaTable = GetTestSchema();

            Select sel = new Select();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            sel.SetOrdinal(schemaTable, list);

            Assert.AreEqual(6, list.ElementAt(1).ColumnOrdinalDic["PresidentId"]);
        }

        [TestMethod]
        public void TestOrdinalForId()
        {
            DataTable schemaTable = GetTestSchema();

            Select sel = new Select();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            sel.SetOrdinal(schemaTable, list);

            Assert.AreEqual(0, list.ElementAt(0).ColumnOrdinalDic["Id"]);
            Assert.AreEqual(5, list.ElementAt(1).ColumnOrdinalDic["Id"]);
        }

        [TestMethod]
        public void TestOrdinalForCustomMapping()
        {
            DataTable schemaTable = GetTestSchema();

            Select sel = new Select();

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

            sel.SetOrdinal(schemaTable, list);

            Assert.AreEqual(9, list.ElementAt(1).ColumnOrdinalDic["Assistant Last Name"]);
        }

        private DataTable GetTestSchema()
        {
            DataTable tab = new DataTable("Test") { };
            tab.Columns.Add("ColumnName");
            tab.Columns.Add("ColumnOrdinal");

            tab.Rows.Add("Id", 0);
            tab.Rows.Add("FirstName", 1);
            tab.Rows.Add("LastName", 2);
            tab.Rows.Add("Last Name", 3);
            tab.Rows.Add("Fans", 4);

            tab.Rows.Add("Id", 5);
            tab.Rows.Add("PresidentId", 6);
            tab.Rows.Add("FirstName", 7);
            tab.Rows.Add("Assistant Last Name", 9);

            return tab;
        }
    }
}
