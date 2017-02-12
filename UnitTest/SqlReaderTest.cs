using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using System.Data.SqlClient;
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

            Dictionary<string, PropertyInfo> concurrentDic = new Dictionary<string, PropertyInfo>();

            ISprocObjectMap objectMap = new SprocObjectMap<President>()
            {
                Columns = columns
            };

            var result = SprocMapperHelper.GetObject<President>(objectMap, moq.Object);

            Assert.AreEqual(5, result.Fans);
            Assert.AreEqual("Donald", result.FirstName);
            Assert.AreEqual(true, result.IsHonest);
            Assert.IsNull(result.LastName);
        }
    }
}
