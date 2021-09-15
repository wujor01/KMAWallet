using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Common
{
    public class DAOHelper
    {
        public static List<JObject> ConvertToJObjectList(DataTable dataTable)
        {
            var list = new List<JObject>();

            foreach (DataRow row in dataTable.Rows)
            {
                var item = new JObject();

                foreach (DataColumn column in dataTable.Columns)
                {
                    item.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                }

                list.Add(item);
            }
            return list;
        }

        public static DataTable ExecQueryToDataTable(string query, string connectionString)
        {
            DataTable table = new DataTable();
            string sqlDataSource = connectionString;
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();

                }
            }
            return table;
        }

        public static List<T> ExecStoreToObject<T>(string query, string connectionString)
        {
            DataTable dtResult = ExecQueryToDataTable(query, connectionString);
            dtResult = dtResult.ToUpperColumnName();
            List<T> result = dtResult.Translate<T>();
            return result;
        }
    }
}
