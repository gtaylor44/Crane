using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Model;
using SprocMapperLibrary;
using SprocMapperLibrary.Core;
using SprocMapperLibrary.TestCommon;

namespace UnitTest
{
    [TestClass]
    public class OrdinalTest
    {

        [TestMethod]
        public void TestOrdinalForId()
        {
            // Arrange
            DataTable schemaTable = DataTableFactory.GetTestDataTable();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());

            // Act
            SprocMapper.SetOrdinal(schemaTable, list, null);

            // Assert
            Assert.AreEqual(0, list.ElementAt(0).ColumnOrdinalDic["Id"]);
        }

        [TestMethod]
        public void TestOrdinalForCustomMapping()
        {
            // Arrange
            DataTable schemaTable = DataTableFactory.GetTestDataTable();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            Dictionary<Type, Dictionary<string, string>> customColumnMappingDic = new Dictionary<Type, Dictionary<string, string>>();
            Dictionary<string, string> customColumnMapping = new Dictionary<string, string>() { { "LastName", "Assistant Last Name" }, {"FirstName", "Assistant First Name"} };
            customColumnMappingDic.Add(typeof(PresidentAssistant), customColumnMapping);
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());
            SprocMapper.MapObject<PresidentAssistant>(list, customColumnMappingDic);

            // Act
            SprocMapper.SetOrdinal(schemaTable, list, "id|presidentId");

            // Assert
            Assert.AreEqual(8, list.ElementAt(1).ColumnOrdinalDic["Assistant Last Name"]);
        }

        [TestMethod]
        public void GetOrdinalPartition_ReturnsCorrectArr()
        {
            const string partitionOn = "Id|Id";
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            var result = SprocMapper.GetOrdinalPartition(schema, partitionOn, 2);

            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(3, result[1]);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Invalid number of arguments entered for partitionOn. Expected 2 arguments but instead saw 3 arguments")]
        public void GetOrdinalPartition_ThrowsException_WhenInvalidNumberOfPartitionOnArgumentsEntered()
        {
            const string partitionOn = "Id|Id|Id";
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            SprocMapper.GetOrdinalPartition(schema, partitionOn, 2);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "First partitionOn argument is incorrect. Expected Id but instead saw FirstName")]
        public void GetOrdinalPartition_ThrowsException_WhenFirstColumnNotAMatch()
        {
            const string partitionOn = "FirstName";
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            SprocMapper.GetOrdinalPartition(schema, partitionOn, 1);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Please check that partitionOn arguments are all valid column names. I was only able to match the following arguments: Id. Expecting a total of 2 valid arguments.")]
        public void GetOrdinalPartition_ThrowsException_WhenCantFindAllPartitionArguments()
        {
            const string partitionOn = "Id|MiddleName";
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            SprocMapper.GetOrdinalPartition(schema, partitionOn, 2);
        }

        [TestMethod]
        public void GetOrdinalPosition_WithDuplicateId_ReturnsCorrectOrdinal()
        {
            var dataTable = DataTableFactory.GetInvalidSchema().Rows.Cast<DataRow>().ToList();

            var result = SprocMapper.GetOrdinalPosition(dataTable, "Id", 3, 5);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void GetOrdinalPosition_WhenMaxRangeIsNull()
        {
            var dataTable = DataTableFactory.GetInvalidSchema().Rows.Cast<DataRow>().ToList();

            var result = SprocMapper.GetOrdinalPosition(dataTable, "MiddleName", 5, null);

            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void GetOrdinalPosition_ReturnsNullWhenColumnDoesNotExist()
        {
            var dataTable = DataTableFactory.GetInvalidSchema().Rows.Cast<DataRow>().ToList();

            var result = SprocMapper.GetOrdinalPosition(dataTable, "Absent", 0, null);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void GetOrdinalPosition_ReturnsNullWhenDoesNotExistInRange()
        {
            var dataTable = DataTableFactory.GetInvalidSchema().Rows.Cast<DataRow>().ToList();

            var result = SprocMapper.GetOrdinalPosition(dataTable, "Last Name", 3, 5);

            Assert.AreEqual(null, result);
        }

    }
}
