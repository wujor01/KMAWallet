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
        #region Fields
        protected IData objDataAccess = null;
        protected const string OUT_TEXT_DEFAULT = "v_out";
        #endregion

        public IData DataAccess
        {
            get { return objDataAccess; }
            set { objDataAccess = value; }
        }

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

        public static List<T> ExecQueryToObject<T>(string query, string connectionString)
        {
            DataTable dtResult = ExecQueryToDataTable(query, connectionString);
            dtResult = dtResult.ToUpperColumnName();
            List<T> result = dtResult.Translate<T>();
            return result;
        }

        public List<T> ExecStoreToObject<T>(List<object> listParameters, string storeName, string connectionString, int timeoutSecond = 0)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(connectionString);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();
                if (timeoutSecond == 0)
                {
                    objIData.CreateNewStoredProcedure(storeName);
                }
                else
                {
                    objIData.CreateNewStoredProcedure(storeName, timeoutSecond);
                }

                for (int i = 0; i < listParameters.Count; i++)
                {
                    objIData.AddParameter("p_" + (i + 1).ToString(), listParameters[i]);
                }

                DataTable dtResult = objIData.ExecStoreToDataTable();

                if (objDataAccess == null)
                    objIData.CommitTransaction();

                dtResult = dtResult.ToUpperColumnName();
                List<T> result = dtResult.Translate<T>();

                //List<T> result = MethodHelper.ConvertDataTable<T>(dtResult);

                return result;
            }
            catch (Exception objEx)
            {
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw objEx;
            }
            finally
            {
                if (objDataAccess == null)
                    objIData.Disconnect();
            }
        }

        /// <summary>
        /// Function excute store and convert to list object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listParameters"></param>
        /// <param name="storeName"></param>
        /// <returns></returns>
        public string ExecStoreToString(List<object> listParameters, string storeName, string connectionString, int timeoutSecond = 0)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(connectionString);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();

                if (timeoutSecond == 0)
                {
                    objIData.CreateNewStoredProcedure(storeName);
                }
                else
                {
                    objIData.CreateNewStoredProcedure(storeName, timeoutSecond);
                }

                for (int i = 0; i < listParameters.Count; i++)
                {
                    objIData.AddParameter("p_" + (i + 1).ToString(), listParameters[i]);
                }

                string dtResult = objIData.ExecStoreToString();

                if (objDataAccess == null)
                    objIData.CommitTransaction();

                return dtResult;
            }
            catch (Exception objEx)
            {
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw objEx;
            }
            finally
            {
                if (objDataAccess == null)
                    objIData.Disconnect();
            }
        }


        /// <summary>
        /// Function exec store none query
        /// </summary>
        /// <param name="listParameters"></param>
        /// <param name="storeName"></param>
        public void ExecStoreNoneQuery(List<object> listParameters, string storeName, string connectionString, int timeoutSecond = 0)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(connectionString);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();

                if (timeoutSecond == 0)
                {
                    objIData.CreateNewStoredProcedure(storeName);
                }
                else
                {
                    objIData.CreateNewStoredProcedure(storeName, timeoutSecond);
                }

                for (int i = 0; i < listParameters.Count; i++)
                {
                    objIData.AddParameter("p_" + (i + 1).ToString(), listParameters[i]);
                }
                objIData.ExecNonQuery();
                if (objDataAccess == null)
                    objIData.CommitTransaction();
            }
            catch (Exception objEx)
            {
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw objEx;
            }
            finally
            {
                if (objDataAccess == null)
                    objIData.Disconnect();
            }
        }

    }
}
