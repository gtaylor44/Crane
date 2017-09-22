using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SprocMapperLibrary;
using SprocMapperLibrary.TestCommon;
using SprocMapperLibrary.TestCommon.Model;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UnitTest
{
    [TestClass]
    public class OrdinalTest
    {
        [TestMethod]
        public void TestOrdinalForId()
        {
            // Arrange
            var schemaTable = DataTableFactory.GetTestDataTable()?.Rows.Cast<DataRow>().ToList();
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
            var schemaTable = DataTableFactory.GetTestDataTable()?.Rows.Cast<DataRow>().ToList();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            Dictionary<Type, Dictionary<string, string>> customColumnMappingDic = new Dictionary<Type, Dictionary<string, string>>();
            Dictionary<string, string> customColumnMapping = new Dictionary<string, string>() { { "LastName", "Assistant Last Name" }, {"FirstName", "Assistant First Name"} };
            customColumnMappingDic.Add(typeof(PresidentAssistant), customColumnMapping);
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());
            SprocMapper.MapObject<PresidentAssistant>(list, customColumnMappingDic);
            int[] partitionOnOrdinal = {0, 6};

            // Act
            SprocMapper.SetOrdinal(schemaTable, list, partitionOnOrdinal);

            // Assert
            Assert.AreEqual(8, list.ElementAt(1).ColumnOrdinalDic["Assistant Last Name"]);
        }

        [TestMethod]
        public void GetOrdinalPartition_ReturnsCorrectArr()
        {
            string[] partitionOnArr = "Id|Id".Split('|');
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            var result = SprocMapper.GetOrdinalPartition(schema, partitionOnArr, 2);

            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(3, result[1]);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Invalid number of arguments entered for partitionOn. Expected 2 arguments but instead saw 3 arguments")]
        public void GetOrdinalPartition_ThrowsException_WhenInvalidNumberOfPartitionOnArgumentsEntered()
        {
            string[] partitionOnArr = "Id|Id|Id".Split('|');
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            SprocMapper.GetOrdinalPartition(schema, partitionOnArr, 2);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "First partitionOn argument is incorrect. Expected Id but instead saw FirstName")]
        public void GetOrdinalPartition_ThrowsException_WhenFirstColumnNotAMatch()
        {
            string[] partitionOnArr = "FirstName".Split('|');
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            SprocMapper.GetOrdinalPartition(schema, partitionOnArr, 1);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Please check that partitionOn arguments are all valid column names. SprocMapper was only able to match the following arguments: Id. Expecting a total of 2 valid arguments.")]
        public void GetOrdinalPartition_ThrowsException_WhenCantFindAllPartitionArguments()
        {
            string[] partitionOnArr = "Id|MiddleName".Split('|');
            var schema = DataTableFactory.GetTestDataTableV2().Rows.Cast<DataRow>().ToList();

            SprocMapper.GetOrdinalPartition(schema, partitionOnArr, 2);
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
