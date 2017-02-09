using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using UnitTest.Model;
using System.Data.SqlClient;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SprocMapper sprocMapper = new SprocMapper();

            var objectMapping = sprocMapper.MapObject<Politician>()
                .AddColumn(x => x.FirstName)
                .AddColumn(x => x.LastName)
                .GetMap();

            SprocMapper<Politician> sprocMapper2 = new SprocMapper<Politician>();
            var test = sprocMapper2
                .Select(objectMapping)
                .ExecuteReader(new SqlConnection(""), "dbo.storedproc");

        }

        [TestMethod]
        public void TestMethod2()
        {
            Politician pol = new Politician()
            {
                Fans = null
            };
            SprocMapperHelper.SetProperty(pol.GetType(), pol.GetType().GetProperty("Fans"), pol);
        }
    }
}
