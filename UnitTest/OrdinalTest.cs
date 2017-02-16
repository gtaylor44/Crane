using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using System.Linq;
using Model;
using Moq;
using SprocMapperLibrary.CustomException;
using SprocMapperLibrary.TestCommon;

namespace UnitTest
{
    [TestClass]
    public class OrdinalTest
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

            var dataTable = GetTestDataTable();

            //SprocMapper.SetOrdinal(dataTable, new List<ISprocObjectMap>() {objectMap});

            //var result = SprocMapper.GetObject<President>(objectMap, moq.Object);

            //Assert.AreEqual(5, result.Fans);
            //Assert.AreEqual("Donald", result.FirstName);
            //Assert.IsTrue(result.IsHonest);
        }

        [TestMethod]
        public void TestOrdinal()
        {
            DataTable schemaTable = GetTestDataTable();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .CustomColumnMapping(x => x.LastName, "Assistant Last Name")
                .CustomColumnMapping(x => x.FirstName, "Assistant First Name")
                .IgnoreColumn(x => x.Id)
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            //SprocMapper.SetOrdinal(schemaTable, list);

            //Assert.AreEqual(6, list.ElementAt(1).ColumnOrdinalDic["PresidentId"]);
        }

        [TestMethod]
        public void TestOrdinalForId()
        {
            DataTable schemaTable = GetTestDataTable();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);

            //SprocMapper.SetOrdinal(schemaTable, list);

            //Assert.AreEqual(0, list.ElementAt(0).ColumnOrdinalDic["Id"]);
        }

        [TestMethod]
        public void TestOrdinalForCustomMapping()
        {
            DataTable schemaTable = GetTestDataTable();

            List<ISprocObjectMap> list = new List<ISprocObjectMap>();

            var presidentObjectMap = PropertyMapper.MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var assPresidentObjectMap = PropertyMapper.MapObject<PresidentAssistant>()
                .CustomColumnMapping(x => x.LastName, "Assistant Last Name")
                .CustomColumnMapping(x => x.FirstName, "Assistant First Name")
                .IgnoreColumn(x => x.Id)
                .AddAllColumns()
                .GetMap();

            list.Add(presidentObjectMap);
            list.Add(assPresidentObjectMap);

            //SprocMapper.SetOrdinal(schemaTable, list);

            //Assert.AreEqual(8, list.ElementAt(1).ColumnOrdinalDic["Assistant Last Name"]);
        }

        [TestMethod]
        public void GetOrdinalPartition_ReturnsCorrectArr()
        {
            const string partitionOn = "Id|Id";
            DataTable schema = GetTestDataTableV2();

            var result = SprocMapper.GetOrdinalPartition(schema, partitionOn, 2);

            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(3, result[1]);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Invalid number of arguments entered for partitionOn. Expected 2 arguments but instead saw 3 arguments")]
        public void GetOrdinalPartition_ThrowsException_WhenInvalidNumberOfPartitionOnArgumentsEntered()
        {
            const string partitionOn = "Id|Id|Id";
            DataTable schema = GetTestDataTableV2();

            SprocMapper.GetOrdinalPartition(schema, partitionOn, 2);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "First partitionOn argument is incorrect. Expected Id but instead saw FirstName")]
        public void GetOrdinalPartition_ThrowsException_WhenFirstColumnNotAMatch()
        {
            const string partitionOn = "FirstName";
            DataTable schema = GetTestDataTableV2();

            SprocMapper.GetOrdinalPartition(schema, partitionOn, 1);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Please check that partitionOn arguments are all valid column names. Was only able to match 1 arguments")]
        public void GetOrdinalPartition_ThrowsException_WhenCantFindAllPartitionArguments()
        {
            const string partitionOn = "Id|MiddleName";
            DataTable schema = GetTestDataTableV2();

            SprocMapper.GetOrdinalPartition(schema, partitionOn, 2);
        }

        private DataTable GetTestDataTable()
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
            tab.Rows.Add("PresidentId", 6);
            tab.Rows.Add("Assistant First Name", 7);
            tab.Rows.Add("Assistant Last Name", 8);
            return tab;
        }

        private DataTable GetTestDataTableV2()
        {
            DataTable tab = new DataTable("Test") { };
            tab.Columns.Add("ColumnName");
            tab.Columns.Add("ColumnOrdinal");

            tab.Rows.Add("Id", 0);
            tab.Rows.Add("FirstName", 1);
            tab.Rows.Add("LastName", 2);
            tab.Rows.Add("Id", 3);
            tab.Rows.Add("FirstName", 4);
            tab.Rows.Add("LastName", 5);
            return tab;
        }
    }
}
