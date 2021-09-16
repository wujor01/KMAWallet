using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Common
{
    public interface IData
    {

        #region Properties

        IDbConnection IConnection
        {
            get;
            set;
        }

        IDbCommand ICommand
        {
            get;
            set;
        }

        IDbTransaction ITransaction
        {
            get;
            set;
        }

        #endregion

        #region Destructors

        void Dispose();

        #endregion

        #region Connect Functions

        /// <summary>
        /// Tạo kết nối với CSDL, hàm trả về 1 nếu kết nối thành công
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// Ngắt kết nối với hệ CSDL, trả về 1 nếu close thành công
        /// </summary>
        /// <returns></returns>
        bool Disconnect();

        /// <summary>
        /// Hàm cho biết đã kết nối hay chưa
        /// </summary>
        /// <returns>true: đã kết nối, false chưa kết nối</returns>
        bool IsConnected();

        #endregion

        #region Transaction Functions

        void BeginTransaction();

        void CommitTransaction();

        void RollBackTransaction();

        #endregion

        #region Database interactive by query

        ///// <summary>
        ///// Executing a Query to IDataReader
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <returns></returns>
        //IDataReader ExecQueryToDataReader(String strSQL);

        ///// <summary>
        ///// Executing a Query to Hashtable
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <returns></returns>
        //Hashtable ExecQueryToHashtable(String strSQL);

        ///// <summary>
        ///// Executing a Query to ArrayList
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <returns></returns>
        //ArrayList ExecQueryToArrayList(String strSQL);

        ///// <summary>
        ///// Executing a Query to String 
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <returns></returns>
        //String ExecQueryToString(String strSQL);

        ///// <summary>
        ///// Executing a Query to Byte[]
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <returns></returns>
        //byte[] ExecQueryToBinary(String strSQL);

        /// <summary>
        /// Executing a Query
        /// </summary>
        /// <param name="strSQL"></param>
        void ExecUpdate(String strSQL);

        ///// <summary>
        ///// Executing a Query with List Parameter
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <param name="objParameters"></param>
        //void ExecUpdate(String strSQL, params IDataParameter[] objParameters);

        ///// <summary>
        ///// Executing a Query with List Parameter
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <param name="arrParameters">IDataParameter array</param>
        //void ExecUpdate(String strSQL, ArrayList arrParameters);

        ///// <summary>
        ///// Execute Query to IDataAdapter
        ///// </summary>
        ///// <param name="strSQL"></param>
        ///// <returns></returns>
        //IDataAdapter ExecQueryToDataAdapter(String strSQL);

        ///// <summary>
        ///// Executing a Query to DataTable
        ///// </summary>
        ///// <returns></returns>
        //DataTable ExecQueryToDataTable(String strSQL);

        ///// <summary>
        ///// Executing a Query to DataSet
        ///// </summary>
        ///// <returns></returns>
        //DataSet ExecQueryToDataSet(String strSQL);

        #endregion

        #region Database interactive by store procedure

        /// <summary>
        /// Init a Query
        /// </summary>
        /// <param name="strSQL"></param>
        void CreateNewSqlText(String strSQL);

        /// <summary>
        /// Init a StoredProcedure
        /// </summary>
        /// <param name="strStoreProName">Tên store</param>
        void CreateNewStoredProcedure(String strStoreProName);

        /// <summary>
        /// Init a StoredProcedure with TimeOut setting
        /// </summary>
        /// <param name="strStoreProName">Tên StoreProcedure</param>
        /// <param name="intTimeOut">Thời gian hết hạn (seconds)</param>
        void CreateNewStoredProcedure(String strStoreProName, int intTimeOut);

        /// <summary>
        /// Add Parameter to StoredProcedure
        /// </summary>
        /// <param name="strParameterName"></param>
        /// <param name="objValue"></param>
        /// <returns></returns>
        void AddParameter(String strParameterName, object objValue);

        /// <summary>
        /// Add Parameter to StoredProcedure
        /// Lưu ý: kiểu NText cần đặt trong Transaction
        /// </summary>
        /// <param name="strParameterName"></param>
        /// <param name="objValue"></param>
        /// <returns></returns>
        void AddParameter(String strParameterName, object objValue, Globals.DATATYPE enDataType);

        /// <summary>
        /// Add Parameter to StoredProcedure
        /// </summary>
        /// <param name="objArrParam">Mảng các cặp {(<@tên tham số>, <giá trị>), ...}</param>
        void AddParameter(params object[] objArrParam);

        /// <summary>
        /// Add Parameter to StoredProcedure
        /// </summary>
        /// <param name="hstParameter"></param>
        /// <returns></returns>
        void AddParameter(Hashtable hstParameter);

        /// <summary>
        /// Executing a StoredProcedure to IDataReader
        /// </summary>
        /// <returns></returns>
        IDataReader ExecStoreToDataReader();

        /// <summary>
        /// Executing a StoredProcedure to IDataReader
        /// </summary>
        /// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        /// <returns></returns>
        IDataReader ExecStoreToDataReader(String strOutParameter);

        /// <summary>
        /// Executing a StoredProcedure to Hashtable
        /// </summary>
        /// <returns></returns>
        Hashtable ExecStoreToHashtable();

        /// <summary>
        /// Executing a StoredProcedure to Hashtable
        /// </summary>
        /// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        /// <returns></returns>
        Hashtable ExecStoreToHashtable(String strOutParameter);

        ///// <summary>
        ///// Executing a StoredProcedure to ArrayList
        ///// </summary>
        ///// <returns></returns>
        //ArrayList ExecStoreToArrayList();

        ///// <summary>
        ///// Executing a StoredProcedure to ArrayList
        ///// </summary>
        ///// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        ///// <returns></returns>
        //ArrayList ExecStoreToArrayList(String strOutParameter);

        /// <summary>
        /// Executing a StoredProcedure to String
        /// </summary>
        /// <returns></returns>
        String ExecStoreToString();

        /// <summary>
        /// Executing a StoredProcedure to String
        /// </summary>
        /// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        /// <returns></returns>
        String ExecStoreToString(String strOutParameter);

        ///// <summary>
        ///// Executing a StoredProcedure to Byte[]
        ///// </summary>
        ///// <returns></returns>
        //byte[] ExecStoreToBinary();

        ///// <summary>
        ///// Executing a StoredProcedure to Byte[]
        ///// </summary>
        ///// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        ///// <returns></returns>
        //byte[] ExecStoreToBinary(String strOutParameter);

        /// <summary>
        /// Executing a StoredProcedure
        /// </summary>
        /// <returns>Number of rows effect</returns>
        int ExecNonQuery();

        ///// <summary>
        ///// Executing a StoredProcedure to IDataAdapter
        ///// </summary>
        ///// <returns></returns>
        //IDataAdapter ExecStoreToDataAdapter();

        ///// <summary>
        ///// Executing a StoredProcedure to IDataAdapter
        ///// </summary>
        ///// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        ///// <returns></returns>
        //IDataAdapter ExecStoreToDataAdapter(String strOutParameter);

        /// <summary>
        /// Executing a StoredProcedure to DataTable
        /// </summary>
        /// <returns></returns>
        DataTable ExecStoreToDataTable();

        /// <summary>
        /// Executing a StoredProcedure to DataTable
        /// </summary>
        /// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        /// <returns></returns>
        DataTable ExecStoreToDataTable(String strOutParameter);

        /// <summary>
        /// Executing a StoredProcedure to DataSet
        /// </summary>
        /// <returns></returns>
        DataSet ExecStoreToDataSet();

        /// <summary>
        /// Executing a StoredProcedure to DataSet
        /// </summary>
        /// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        /// <returns></returns>
        DataSet ExecStoreToDataSet(params String[] strOutParameter);

        /// <summary>
        /// Executing a StoredProcedure to list object 
        /// </summary>
        /// <returns></returns>
        List<object> ExecStoreToListObject();

        /// <summary>
        /// Executing a StoredProcedure to list object 
        /// </summary>
        /// <param name="strOutParameter">Parameter Out Name ("" --> "v_Out")</param>
        /// <returns></returns>
        List<object> ExecStoreToListObject(String strOutParameter);

        void CreateNewBuckCopy(string strTableName, System.Data.DataTable table);

        #endregion

    }

    public class Globals
    {
        public enum DATATYPE
        {
            NUMBER,
            CHAR,
            VARCHAR,
            NVARCHAR,
            NTEXT,
            BINARY,
            BLOB,
            CLOB,
            NCLOB,
            SMALLINT,
            TIMESTAMP,
            BOOLEAN,
            BIGINT,
            INTEGER,
            TEXT,
            NUMERIC,
            DATE,
            DATETIME,
            REFCURSOR,
            BIT,
            TIME,
            DOUBLE,
            SINGLE,
            REAL
        }
    }
}
