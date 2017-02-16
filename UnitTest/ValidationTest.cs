using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary.CustomException;
using SprocMapperLibrary;
using SprocMapperLibrary.TestCommon;

namespace UnitTest
{
    [TestClass]
    public class ValidationTest
    {
        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Duplicate column in select not allowed. Ensure that all columns in stored procedure are unique. " +
                            "Try setting an alias for your column in your stored procedure " +
                            "and set up a custom column mapping. The offending column is 'Id'")]
        public void ValidateDuplicateSelectAliases_ThrowsException_WhenHasDuplicateColumns()
        {
            Select select = new Select();
            DataTable table = GetInvalidSchema();

            SprocMapper.ValidateDuplicateSelectAliases(table, false, null, null);

        }

        [TestMethod]
        public void ValidateDuplicateSelectAliases_IsValid()
        {
            DataTable table = GetValidSchema();

            var result = SprocMapper.ValidateDuplicateSelectAliases(table, false, null, null);

            Assert.IsTrue(result);
        }


        private DataTable GetInvalidSchema()
        {
            DataTable tab = new DataTable("Test") { };
            tab.Columns.Add("ColumnName");
            tab.Columns.Add("ColumnOrdinal");

            tab.Rows.Add("Id", 0);
            tab.Rows.Add("FirstName", 1);
            tab.Rows.Add("Last Name", 2);
            tab.Rows.Add("Id", 3);

            return tab;
        }

        private DataTable GetValidSchema()
        {
            DataTable tab = new DataTable("Test") { };
            tab.Columns.Add("ColumnName");
            tab.Columns.Add("ColumnOrdinal");

            tab.Rows.Add("Id", 0);
            tab.Rows.Add("FirstName", 1);
            tab.Rows.Add("Last Name", 2);

            return tab;
        }
    }
}
