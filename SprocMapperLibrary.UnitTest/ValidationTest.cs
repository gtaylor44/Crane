using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary.Core;
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
            DataTable table = DataTableFactory.GetInvalidSchema();

            SprocMapper.ValidateDuplicateSelectAliases(table, false, null, null);

        }

        [TestMethod]
        public void ValidateDuplicateSelectAliases_IsValid()
        {
            DataTable table = DataTableFactory.GetValidSchema();

            var result = SprocMapper.ValidateDuplicateSelectAliases(table, false, null, null);

            Assert.IsTrue(result);
        }
    }
}
