using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SprocMapperLibrary
{
    public abstract class AbstractSelect
    {
        private List<SqlParameter> _paramList;
        private List<ISprocObjectMap> _sprocObjectMapList;
        private List<Join> _joinManyList;
        private string _parentKey { get; set; }

        public AbstractSelect(List<ISprocObjectMap> sprocObjectMapList)
        {
            _paramList = new List<SqlParameter>();
            _joinManyList = new List<Join>();
            _sprocObjectMapList = sprocObjectMapList;
        }

    }
}
