using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using SprocMapperLibrary.Interface;
using SprocMapperLibrary.TestCommon;
using SprocMapperLibrary.TestCommon.Model;

namespace UnitTest
{
    [TestClass]
    public class ValidateSelectColumnsTest
    {
        [TestMethod]
        public void ValidateSelectColumns_SingleEntity_AllColumnsMatched()
        {
            // Arrange
            var schemaTable = DataTableFactory.GetPresidentSchema()?.Rows.Cast<DataRow>().ToList();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());

            list.ElementAt(0).ColumnOrdinalDic = GetValidPresidentOrdinalDic();

            // Act
            var result = SprocMapper.ValidateSelectColumns(schemaTable, list, null, "dbo.GetPresidents");

            // Assert
            Assert.IsTrue(result);

        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "'validateSelectColumns' flag is set to TRUE\n\nThe following columns from the select statement in 'dbo.GetPresidents' have " +
                                                           "not been mapped to target model 'President'.\n\nSelect column: 'FirstName'\nTarget model: 'President'\n")]
        public void ValidateSelectColumns_SingleEntity_OneColumnMissing()
        {
            // Arrange
            var schemaTable = DataTableFactory.GetPresidentSchema()?.Rows.Cast<DataRow>().ToList();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());

            list.ElementAt(0).ColumnOrdinalDic = GetValidPresidentOrdinalDic();
            list.ElementAt(0).ColumnOrdinalDic.Remove("FirstName");
            list.ElementAt(0).ColumnOrdinalDic.Add("First Name", 1);

            // Act
            var result = SprocMapper.ValidateSelectColumns(schemaTable, list, null, "dbo.GetPresidents");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateSelectColumns_TwoEntities_AllColumnsMatched()
        {
            // Arrange
            var schemaTable = DataTableFactory.GetPresidentAndAssistantSchema()?.Rows.Cast<DataRow>().ToList();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());

            SprocMapper.MapObject<PresidentAssistant>(list, new Dictionary<Type, Dictionary<string, string>>());

            list.ElementAt(0).ColumnOrdinalDic = GetValidPresidentOrdinalDic();
            list.ElementAt(1).ColumnOrdinalDic = GetValidPresidentAssistantOrdinalDic();

            // Act
            var result = SprocMapper.ValidateSelectColumns(schemaTable, list, new[] { 0, 5 }, "dbo.GetPresidents");

            Assert.IsTrue(result);
        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException))]
        public void ValidateSelectColumns_TwoEntities_TwoColumnsMissing()
        {
            // Arrange
            var schemaTable = DataTableFactory.GetPresidentAndAssistantSchema()?.Rows.Cast<DataRow>().ToList();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());

            SprocMapper.MapObject<PresidentAssistant>(list, new Dictionary<Type, Dictionary<string, string>>());

            list.ElementAt(0).ColumnOrdinalDic = GetValidPresidentOrdinalDic();
            list.ElementAt(1).ColumnOrdinalDic = GetValidPresidentAssistantOrdinalDic();
            list.ElementAt(0).ColumnOrdinalDic.Remove("FirstName");
            list.ElementAt(0).ColumnOrdinalDic.Add("First Name", 1);
            list.ElementAt(1).ColumnOrdinalDic.Remove("PresidentId");

            // Act
            var result = SprocMapper.ValidateSelectColumns(schemaTable, list, new []{0, 5}, "dbo.GetPresidents");

        }

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException))]
        public void ValidateSelectColumns_TwoEntities_ColumnMissingBecauseOfIncorrectPartition()
        {
            // Arrange
            var schemaTable = DataTableFactory.GetPresidentAndAssistantSchema()?.Rows.Cast<DataRow>().ToList();
            List<ISprocObjectMap> list = new List<ISprocObjectMap>();
            SprocMapper.MapObject<President>(list, new Dictionary<Type, Dictionary<string, string>>());

            SprocMapper.MapObject<PresidentAssistant>(list, new Dictionary<Type, Dictionary<string, string>>());

            list.ElementAt(0).ColumnOrdinalDic = GetValidPresidentOrdinalDic();
            list.ElementAt(1).ColumnOrdinalDic = GetValidPresidentAssistantOrdinalDic();

            // Act
            SprocMapper.ValidateSelectColumns(schemaTable, list, new[] { 0, 7 }, "dbo.GetPresidents");

        }

        private Dictionary<string, int> GetValidPresidentOrdinalDic()
        {
            return new Dictionary<string, int>()
            {
                { "Id", 0 },
                { "FirstName", 1 },
                { "LastName", 2 },
                { "Fans", 3 },
                { "IsHonest", 4 }
            };
        }

        private Dictionary<string, int> GetValidPresidentAssistantOrdinalDic()
        {
            return new Dictionary<string, int>()
            {
                { "Id", 5 },
                { "PresidentId", 6 },
                { "FirstName", 7 },
                { "LastName", 8 },
            };
        }
    }
}
