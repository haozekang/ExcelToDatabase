using Kang.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelToDatabase.Models.Domain
{
    //information_schema.tables
    [KangSQLTable(Table = "information_schema")]
    public class TableItem
    {
        [KangSQLColumn(Name = "name")]
        private String name;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}
