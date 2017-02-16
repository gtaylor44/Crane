using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprocMapperLibrary.TestCommon
{
    public static class DataTableFactory
    {
        public static DataTable GetTestDataTable()
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

        public static DataTable GetTestDataTableV2()
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

        public static DataTable GetInvalidSchema()
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

        public static DataTable GetValidSchema()
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
