using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIServices.Common
{
    [Serializable]
    public class PosgreSQLData : Data, IData
    {
        string _strConnect;
        #region Variables

        private NpgsqlConnection objConnection = null;
        private NpgsqlCommand objCommand = null;
        private NpgsqlTransaction objTransaction = null;
        private string strTableName = "TableNameDefault";
        private const string DEFAULT_REF = "@ref";
        #endregion

        #region Properties

        IDbConnection IData.IConnection
        {
            get { return objConnection; }
            set { objConnection = (NpgsqlConnection)value; }
        }

        IDbTransaction IData.ITransaction
        {
            get { return objTransaction; }
            set { objTransaction = (NpgsqlTransaction)value; }
        }

        IDbCommand IData.ICommand
        {
            get { return objCommand; }
            set { objCommand = (NpgsqlCommand)value; }
        }

        #endregion

        #region Constructors

        public PosgreSQLData()
        {

        }

        public PosgreSQLData(String strConnect)
        {
            //this.strConnect = strConnect;
            _strConnect = strConnect;
        }

        ~PosgreSQLData()
        {
            // Nếu còn kết nối thì ngắt kết nối
            if (IsConnected())
                Disconnect();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Connect Functions

        public bool Connect()
        {
            if (IsConnected())
                return true;

            if (objConnection == null)
            {
                string strConnectLocal = _strConnect.Replace(";Unicode=True", string.Empty);
                objConnection = new NpgsqlConnection(strConnectLocal);
            }

            try
            {
                objConnection.Open();
                objConnection.EnlistTransaction(System.Transactions.Transaction.Current);
            }
            catch (Exception objEx)
            {
                // ORA-02396: exceeded maximum idle time, please connect again
                // Nếu gặp lỗi này thì Reconnect lại 1 lần, nếu lỗi nữa thì through Exception

                if (objEx.Message.Contains("ORA-02396"))
                {
                    objConnection.Open();
                    return true;
                }

                throw objEx;
            }

            return true;
        }

        public bool Disconnect()
        {
            try
            {
                if (this.objCommand != null)
                    this.objCommand.Dispose();

                objConnection.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsConnected()
        {
            if (objConnection == null || objConnection.State != ConnectionState.Open)
                return false;
            return true;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Contructing IDbCommand follow DataBase
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        private NpgsqlCommand SetCommand(String strSQL)
        {
            objCommand = new NpgsqlCommand(strSQL, objConnection);
            if (objTransaction != null)
                objCommand.Transaction = objTransaction;
            return objCommand;
        }

        /// <summary>
        /// Contructing IDataAdapter follow DataBase
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        private NpgsqlDataAdapter SetDataAdapter(String strSQL)
        {
            return new NpgsqlDataAdapter(strSQL, objConnection);
        }

        /// <summary>
        /// Contructing IDataAdapter follow DataBase
        /// </summary>
        /// <param name="iCmd"></param>
        /// <returns></returns>
        private NpgsqlDataAdapter SetDataAdapter(NpgsqlCommand objCommand)
        {
            return new NpgsqlDataAdapter(objCommand);
        }


        /// <summary>
        /// Convert Data Type to OracleDataType
        /// </summary>
        /// <param name="enDataType"></param>
        /// <returns></returns>
        private NpgsqlDbType GetOleDBDataType(Globals.DATATYPE enDataType)
        {
            NpgsqlDbType enResult = NpgsqlDbType.Integer;

            switch (enDataType)
            {
                case Globals.DATATYPE.INTEGER:
                    enResult = NpgsqlDbType.Integer;
                    break;
                case Globals.DATATYPE.CHAR:
                    enResult = NpgsqlDbType.Char;
                    break;
                case Globals.DATATYPE.VARCHAR:
                    enResult = NpgsqlDbType.Varchar;
                    break;
                case Globals.DATATYPE.TEXT:
                    enResult = NpgsqlDbType.Text;
                    break;
                case Globals.DATATYPE.BINARY:
                    enResult = NpgsqlDbType.Bytea;
                    break;
                case Globals.DATATYPE.BLOB:
                    enResult = NpgsqlDbType.Bytea;
                    break;
                case Globals.DATATYPE.CLOB:
                    enResult = NpgsqlDbType.Text;
                    break;
                case Globals.DATATYPE.NCLOB:
                    enResult = NpgsqlDbType.Text;
                    break;
                case Globals.DATATYPE.SMALLINT:
                    enResult = NpgsqlDbType.Smallint;
                    break;
                case Globals.DATATYPE.TIMESTAMP:
                    enResult = NpgsqlDbType.Timestamp;
                    break;
                case Globals.DATATYPE.BOOLEAN:
                    enResult = NpgsqlDbType.Boolean;
                    break;
                case Globals.DATATYPE.BIGINT:
                    enResult = NpgsqlDbType.Bigint;
                    break;
                case Globals.DATATYPE.NUMERIC:
                    enResult = NpgsqlDbType.Numeric;
                    break;
                case Globals.DATATYPE.DATE:
                    enResult = NpgsqlDbType.Date;
                    break;
                case Globals.DATATYPE.REFCURSOR:
                    enResult = NpgsqlDbType.Refcursor;
                    break;
                default:
                    break;
            }

            return enResult;
        }

        //public DataTable ExecStoreToDataTable(String strOutParameter)
        //{
        //    if (!string.IsNullOrEmpty(strOutParameter))
        //    {
        //        NpgsqlParameter p = new NpgsqlParameter();
        //        //if (this.isMapParameterByName)
        //        //{
        //        //    p.ParameterName = "@" + strOutParameter.Replace("@", "");
        //        //}
        //        p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
        //        p.Direction = ParameterDirection.InputOutput;
        //        p.Value = strOutParameter;
        //        objCommand.Parameters.Insert(0, p);
        //    }
        //    objCommand.ExecuteNonQuery();
        //    // fetch data
        //    if (!string.IsNullOrEmpty(strOutParameter))
        //    {
        //        objCommand.CommandText = "fetch all in \"" + strOutParameter + "\"";
        //    }
        //    else
        //    {
        //        objCommand.CommandText = "fetch all in \"<unnamed portal 1>\"";
        //    }
        //    objCommand.CommandType = CommandType.Text;
        //    DataTable dataTable = new DataTable(this.strTableName);
        //    using (NpgsqlDataReader dr = objCommand.ExecuteReader())
        //    {
        //        //while (dr.Read())
        //        //{
        //        //    // do what you want with data, convert this to json or...
        //        //    Console.WriteLine(dr[0]);
        //        //}
        //        dataTable.Load(dr);
        //        //this.SetDataAdapter(this.objCommand).Fill(dataTable);
        //    }
        //    return dataTable;
        //}


        private NpgsqlDbType GetPGSQLDataType(Globals.DATATYPE enDataType)
        {
            NpgsqlDbType enResult = NpgsqlDbType.Integer;
            switch (enDataType)
            {
                case Globals.DATATYPE.SMALLINT:
                    enResult = NpgsqlDbType.Smallint;
                    break;
                case Globals.DATATYPE.INTEGER:
                case Globals.DATATYPE.NUMBER:
                case Globals.DATATYPE.NUMERIC:
                    enResult = NpgsqlDbType.Integer;
                    break;
                case Globals.DATATYPE.BIGINT:
                    enResult = NpgsqlDbType.Bigint;
                    break;
                case Globals.DATATYPE.CHAR:
                    enResult = NpgsqlDbType.Char;
                    break;
                case Globals.DATATYPE.VARCHAR:
                    enResult = NpgsqlDbType.Varchar;
                    break;
                case Globals.DATATYPE.NVARCHAR:
                    enResult = NpgsqlDbType.Varchar;
                    break;
                case Globals.DATATYPE.NTEXT:
                    enResult = NpgsqlDbType.Text;
                    break;
                case Globals.DATATYPE.BINARY:
                    enResult = NpgsqlDbType.Bytea;
                    break;
                case Globals.DATATYPE.BLOB:
                    enResult = NpgsqlDbType.Bytea;
                    break;
                case Globals.DATATYPE.CLOB:
                    enResult = NpgsqlDbType.Text;
                    break;
                case Globals.DATATYPE.NCLOB:
                    enResult = NpgsqlDbType.Text;
                    break;
                case Globals.DATATYPE.TIMESTAMP:
                    enResult = NpgsqlDbType.Timestamp;
                    break;
                case Globals.DATATYPE.BOOLEAN:
                    enResult = NpgsqlDbType.Boolean;
                    break;
                case Globals.DATATYPE.BIT:
                    enResult = NpgsqlDbType.Bit;
                    break;
                default:
                    break;
            }

            return enResult;
        }

        #endregion

        #region Transaction Functions

        public void BeginTransaction()
        {
            if (!IsConnected())
                Connect();

            this.objTransaction = objConnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (objTransaction != null)
                objTransaction.Commit();
        }

        public void RollBackTransaction()
        {
            if (objTransaction != null)
            {
                objTransaction.Rollback();
                objTransaction = null;
            }
        }

        #endregion

        #region Execute Text Queries

        public IDataReader ExecQueryToDataReader(String strSQL)
        {
            return SetCommand(strSQL).ExecuteReader();
        }

        //public Hashtable ExecQueryToHashtable(String strSQL)
        //{
        //    IDataReader drReader = ExecQueryToDataReader(strSQL);

        //    Hashtable hstbItem = Globals.ConvertHashTable(drReader);

        //    drReader.Close();

        //    return hstbItem;
        //}

        //public ArrayList ExecQueryToArrayList(String strSQL)
        //{
        //    IDataReader drReader = ExecQueryToDataReader(strSQL);

        //    ArrayList arrLstResult = Globals.ConvertArrayList(drReader);

        //    drReader.Close();

        //    return arrLstResult;
        //}

        public String ExecQueryToString(String strSQL)
        {
            Object objTemp = SetCommand(strSQL).ExecuteScalar();

            if (objTemp == null)
                return String.Empty;

            return objTemp.ToString().Trim();
        }

        public byte[] ExecQueryToBinary(String strSQL)
        {
            return (byte[])(SetCommand(strSQL).ExecuteScalar());
        }

        public void ExecUpdate(String strSQL)
        {
            SetCommand(strSQL).ExecuteNonQuery();
        }

        public void ExecUpdate(String strSQL, params IDataParameter[] objParameters)
        {
            SetCommand(strSQL);

            foreach (IDataParameter objPara in objParameters)
                objCommand.Parameters.Add(objPara);

            objCommand.ExecuteNonQuery();
        }

        public void ExecUpdate(String strSQL, ArrayList arrParameters)
        {
            SetCommand(strSQL);

            foreach (IDataParameter objPara in arrParameters)
                objCommand.Parameters.Add(objPara);

            objCommand.ExecuteNonQuery();
        }

        public IDataAdapter ExecQueryToDataAdapter(String strSQL)
        {
            return SetDataAdapter(strSQL);
        }

        public DataTable ExecQueryToDataTable(String strSQL)
        {
            DataSet dsResult = new DataSet();
            SetDataAdapter(strSQL).Fill(dsResult);
            return dsResult.Tables[0];

        }

        public DataSet ExecQueryToDataSet(String strSQL)
        {
            DataSet dsResult = new DataSet();
            SetDataAdapter(strSQL).Fill(dsResult);
            return dsResult;
        }

        #endregion

        #region Execute Stored Procedures

        public void CreateNewSqlText(String strSQL)
        {
            objCommand = SetCommand(strSQL);
            objCommand.CommandType = System.Data.CommandType.Text;
        }

        public void CreateNewStoredProcedure(String strStoreProName)
        {
            strTableName = strStoreProName;
            objCommand = SetCommand(strStoreProName);
            objCommand.CommandType = CommandType.StoredProcedure;
        }

        public void CreateNewStoredProcedure(String strStoreProName, int intTimeOut)
        {
            strTableName = strStoreProName;
            objCommand = SetCommand(strStoreProName);
            objCommand.CommandTimeout = intTimeOut;
            objCommand.CommandType = System.Data.CommandType.StoredProcedure;
        }

        public void AddParameter(String strParameterName, object objValue)
        {
            //if (objValue != null && objValue.ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
            //    objValue = 1;
            //else if (objValue != null && objValue.ToString().Equals("False", StringComparison.OrdinalIgnoreCase))
            //    objValue = 0;

            if (objValue != null)
                switch (objValue.GetType().Name)
                {
                    case "Boolean":
                        AddParameter(strParameterName, objValue, Globals.DATATYPE.BOOLEAN);
                        break;
                    case "Int64":
                        AddParameter(strParameterName, objValue, Globals.DATATYPE.BIGINT);
                        break;
                    case "Int16":
                        AddParameter(strParameterName, objValue, Globals.DATATYPE.SMALLINT);
                        break;
                    case "Double":
                        AddParameter(strParameterName, objValue, Globals.DATATYPE.NUMERIC);
                        break;
                    case "Decimal":
                        AddParameter(strParameterName, objValue, Globals.DATATYPE.NUMERIC);
                        break;
                    case "Int32":
                        AddParameter(strParameterName, objValue, Globals.DATATYPE.INTEGER);
                        break;
                    default:
                        // objCommand.Parameters.AddWithValue(strParameterName, objValue);
                        objCommand.Parameters.AddWithValue(objValue);
                        break;
                }
            else
                objCommand.Parameters.AddWithValue(DBNull.Value);
            //objCommand.Parameters.AddWithValue(strParameterName, DBNull.Value);
        }

        /// <summary>
        /// Vui lòng không dùng hàm này, hãy thay bằng hàm AddParameter(String strParameterName, object objValue, NpgsqlTypes.NpgsqlDbType enDataType)
        /// </summary>
        /// <param name="strParameterName"></param>
        /// <param name="objValue"></param>
        /// <param name="enDataType"></param>
        public void AddParameter(String strParameterName, object objValue, Globals.DATATYPE enDataType)
        {
            //if (objValue != null)
            //{
            //    //NpgsqlParameter objPara = new NpgsqlParameter(strParameterName.Replace("@", "v_"), GetOleDBDataType(enDataType));
            //    NpgsqlParameter objPara = new NpgsqlParameter(null, GetOleDBDataType(enDataType));
            //    objPara.Value = objValue;
            //    objCommand.Parameters.Add(objPara);
            //}

            if (objValue != null || strParameterName != null)
            {
                //NpgsqlParameter objPara = new NpgsqlParameter(strParameterName.Replace("@", "v_"), GetOleDBDataType(enDataType));
                NpgsqlParameter objPara = new NpgsqlParameter(null, GetOleDBDataType(enDataType));
                objPara.Value = objValue ?? (object)DBNull.Value;
                objCommand.Parameters.Add(objPara);
            }
        }

        /// <summary>
        /// Thêm 1 mảng các paramenter
        /// Dạng {"@paramName1", objValue1, "@paramName2", objValue2, ...}
        /// </summary>
        /// <param name="objArrParam"></param>
        public void AddParameter(params object[] objArrParam)
        {
            bool bolIsHasDATATYPE = false;
            if (objArrParam.Length > 3)
            {
                if (objArrParam[2].GetType().Name == "DATATYPE")
                    bolIsHasDATATYPE = true;
            }

            if (bolIsHasDATATYPE)
                for (int i = 0; i < objArrParam.Length; i += 3)
                    AddParameter(objArrParam[i].ToString().Trim(), objArrParam[i + 1].ToString().Trim(), (Globals.DATATYPE)objArrParam[i + 2]);
            else
                for (int i = 0; i < objArrParam.Length; i += 2)
                    AddParameter(objArrParam[i].ToString().Trim(), objArrParam[i + 1]);

        }

        public void AddParameter(Hashtable hstParameter)
        {
            IDictionaryEnumerator objDicEn = hstParameter.GetEnumerator();

            while (objDicEn.MoveNext())
            {
                AddParameter(objDicEn.Key.ToString(), objDicEn.Value);
            }
        }

        public IDataReader ExecStoreToDataReader()
        {
            return ExecStoreToDataReader("");
        }

        public IDataReader ExecStoreToDataReader(String strOutParameter)
        {
            IDataReader dataReader = (IDataReader)null;
            StringBuilder stringBuilder = new StringBuilder();
            using (NpgsqlDataReader npgsqlDataReader = this.objCommand.ExecuteReader())
            {
                if (((DbDataReader)npgsqlDataReader).Read())
                    stringBuilder.AppendLine(string.Format("FETCH ALL IN \"{0}\";", ((DbDataReader)npgsqlDataReader)[0]));
            }
            if (stringBuilder.Length > 0)
            {
                NpgsqlCommand npgsqlCommand = new NpgsqlCommand();
                npgsqlCommand.Connection = this.objCommand.Connection;
                npgsqlCommand.Transaction = this.objCommand.Transaction;
                ((DbCommand)npgsqlCommand).CommandTimeout = ((DbCommand)this.objCommand).CommandTimeout;
                ((DbCommand)npgsqlCommand).CommandText = stringBuilder.ToString();
                ((DbCommand)npgsqlCommand).CommandType = CommandType.Text;
                dataReader = (IDataReader)npgsqlCommand.ExecuteReader();
            }
            return dataReader;

            //if (strOutParameter.Trim().Length == 0)
            //    strOutParameter = DEFAULT_REF;

            //NpgsqlParameter p = new NpgsqlParameter();
            //p.ParameterName = "@" + strOutParameter.Replace("@", "");
            //p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
            //p.Direction = ParameterDirection.InputOutput;
            //p.Value = "ref";

            //objCommand.Parameters.Insert(0, p);

            //return objCommand.ExecuteReader();
        }

        public Hashtable ExecStoreToHashtable()
        {
            return ExecStoreToHashtable("");
        }

        public Hashtable ExecStoreToHashtable(String strOutParameter)
        {
            //if (strOutParameter.Trim().Length == 0)
            //    strOutParameter = DEFAULT_REF;

            //NpgsqlParameter p = new NpgsqlParameter();
            //p.ParameterName = "@" + strOutParameter.Replace("@", "");
            //p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
            //p.Direction = ParameterDirection.InputOutput;
            //p.Value = "ref";

            //objCommand.Parameters.Insert(0, p);
            Hashtable hstbItem = new Hashtable();

            NpgsqlDataReader dr = objCommand.ExecuteReader();

            while (dr.Read())
                for (int i = 0; i < dr.FieldCount; i++)
                    hstbItem.Add(dr.GetName(i), dr[i]);

            return hstbItem;
        }

        //public ArrayList ExecStoreToArrayList()
        //{
        //    return ExecStoreToArrayList("");
        //}

        //public ArrayList ExecStoreToArrayList(String strOutParameter)
        //{
        //if (strOutParameter.Trim().Length == 0)
        //    strOutParameter = DEFAULT_REF;

        //NpgsqlParameter p = new NpgsqlParameter();
        //p.ParameterName = "@" + strOutParameter.Replace("@", "");
        //p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
        //p.Direction = ParameterDirection.InputOutput;
        //p.Value = "ref";

        //objCommand.Parameters.Insert(0, p);

        //    return Globals.ConvertArrayList(objCommand.ExecuteReader());
        //}

        public String ExecStoreToString()
        {
            return ExecStoreToString("");
        }

        public String ExecStoreToString(String strOutParameter)
        {
            Object objTemp = objCommand.ExecuteScalar();
            if (objTemp == null)
                return "";
            return objTemp.ToString().Trim();
            //if (strOutParameter.Length == 0)
            //    strOutParameter = DEFAULT_OUT_PARAMETER;

            //try
            //{
            //    objCommand.Parameters.Add(strOutParameter,NpgsqlDbType.Varchar, DEFAULT_OUT_PARAMETER_LENGTH).Direction = ParameterDirection.Output;
            //    objCommand.ExecuteScalar();
            //    Object objTemp = objCommand.Parameters[strOutParameter].Value;

            //    if (Convert.IsDBNull(objTemp) || objTemp.ToString().Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
            //        return string.Empty;

            //    return objTemp.ToString().Trim();
            //}
            //catch (Exception ex)
            //{
            //    #region Kiểm tra lỗi maximum idle time, và kô thuộc transaction nào
            //    if (this.objTransaction == null && ex.Message.ToString().Contains("02396") && ex.Message.ToString().Contains("ORA"))
            //    {
            //        try
            //        {
            //            this.Connect();
            //            objCommand.ExecuteScalar();
            //            Object objTemp = objCommand.Parameters[strOutParameter].Value;

            //            if (Convert.IsDBNull(objTemp) || objTemp.ToString().Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
            //                return string.Empty;
            //            return objTemp.ToString().Trim();
            //        }
            //        catch (Exception exn) { throw exn; }
            //    }
            //    #endregion
            //    ProcessException(ex);
            //    throw ex;
            //}
        }

        public byte[] ExecStoreToBinary()
        {
            return ExecStoreToBinary("");
        }

        public byte[] ExecStoreToBinary(String strOutParameter)
        {
            throw new Exception("Vui lòng không sử dụng hàm này!");
        }

        public int ExecNonQuery()
        {
            return objCommand.ExecuteNonQuery();
        }

        public IDataAdapter ExecStoreToDataAdapter()
        {
            return ExecStoreToDataAdapter("");
        }

        public IDataAdapter ExecStoreToDataAdapter(String strOutParameter)
        {
            //if (strOutParameter.Trim().Length == 0)
            //    strOutParameter = DEFAULT_REF;

            //NpgsqlParameter p = new NpgsqlParameter();
            //p.ParameterName = "@" + strOutParameter.Replace("@", "");
            //p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar;
            //p.Direction = ParameterDirection.InputOutput;
            //p.Value = strOutParameter;

            //objCommand.Parameters.Insert(0, p);

            return SetDataAdapter(objCommand);
        }

        public DataTable ExecStoreToDataTable()
        {
            return ExecStoreToDataTable("");
        }

        //public DataTable ExecStoreToDataTable(String strOutParameter)
        //{
        //    if (objTransaction == null)
        //    {
        //        BeginTransaction();
        //    }

        //    if (!string.IsNullOrEmpty(strOutParameter))
        //    {
        //        NpgsqlParameter p = new NpgsqlParameter();
        //        p.ParameterName = "@" + strOutParameter.Replace("@", "");
        //        p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
        //        p.Direction = ParameterDirection.InputOutput;
        //        p.Value = strOutParameter;
        //        objCommand.Parameters.Insert(0, p);
        //    }
        //    //DataTable dtResult = new DataTable(strTableName);
        //    //SetDataAdapter(objCommand).Fill(dtResult);

        //    //return dtResult;


        //    DataTable dtResult = new DataTable(strTableName);

        //    //   NpgsqlTransaction tran = objConnection.BeginTransaction();
        //    //   objCommand.CommandType = CommandType.StoredProcedure;
        //    //if (!string.IsNullOrEmpty(strOutParameter))
        //    //    objCommand.Parameters.Add(
        //    //        new NpgsqlParameter(
        //    //        strOutParameter, NpgsqlTypes.NpgsqlDbType.Refcursor)
        //    //        {
        //    //            Direction = ParameterDirection.Input,
        //    //        });
        //    StringBuilder sql = new StringBuilder();
        //    using (var reader = objCommand.ExecuteReader(CommandBehavior.SequentialAccess))
        //        if (reader.Read())
        //            sql.AppendLine($"FETCH ALL IN \"{ reader[0] }\";");
        //    if (sql.Length > 0)
        //    {
        //        using (var cmd2 = new NpgsqlCommand())
        //        {
        //            cmd2.Connection = objCommand.Connection;
        //            cmd2.Transaction = objCommand.Transaction;
        //            cmd2.CommandTimeout = objCommand.CommandTimeout;
        //            cmd2.CommandText = sql.ToString();
        //            cmd2.CommandType = CommandType.Text;

        //            using (var reader = cmd2.ExecuteReader())
        //                dtResult.Load(reader);

        //            //Execute cmd2 and process the results as normal
        //        }
        //    }



        //    return dtResult;
        //}

        public DataTable ExecStoreToDataTable(String strOutParameter)
        {

            if (!string.IsNullOrEmpty(strOutParameter))
            {
                NpgsqlParameter p = new NpgsqlParameter();
                //p.ParameterName = "@" + strOutParameter.Replace("@", "");
                p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
                p.Direction = ParameterDirection.InputOutput;
                p.Value = strOutParameter;
                objCommand.Parameters.Insert(0, p);
            }
            //this.objCommand.Parameters[0].Direction = ParameterDirection.InputOutput;
            //this.objCommand.Parameters[0].Value = strOutParameter;
            //this.objCommand.Parameters[0].ParameterName = strOutParameter; -> Bỏ cái này mới chạy đc


            DataTable dataTable = new DataTable(this.strTableName);
            StringBuilder sql = new StringBuilder();
            using (var reader = objCommand.ExecuteReader())
                if (reader.Read())
                    sql.AppendLine($"FETCH ALL IN \"{ reader[0] }\";");
            if (sql.Length > 0)
            {
                using (var cmd2 = new NpgsqlCommand())
                {
                    cmd2.Connection = objCommand.Connection;
                    cmd2.Transaction = objCommand.Transaction;
                    cmd2.CommandTimeout = objCommand.CommandTimeout;
                    cmd2.CommandText = sql.ToString();
                    cmd2.CommandType = CommandType.Text;

                    using (var reader = cmd2.ExecuteReader())
                        dataTable.Load(reader);

                    //Execute cmd2 and process the results as normal
                }
            }

            //this.SetDataAdapter(this.objCommand).Fill(dataTable);
            return dataTable;
        }

        public DataSet ExecStoreToDataSet()
        {
            return ExecStoreToDataSet("");
        }

        public DataSet ExecStoreToDataSet(params String[] strOutParameter)
        {
            //for (int i = strOutParameter.Length - 1; i >= 0; i--)
            //{
            //    NpgsqlParameter p = new NpgsqlParameter();
            //    p.ParameterName = "@" + strOutParameter[i].Replace("@", "");
            //    p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Refcursor;
            //    p.Direction = ParameterDirection.InputOutput;
            //    p.Value = strOutParameter[i];

            //    objCommand.Parameters.Insert(0, p);
            //}

            DataSet dsResult = new DataSet();
            //NpgsqlDataAdapter objAdapter = SetDataAdapter(objCommand);
            //objAdapter.Fill(dsResult);

            List<string> lstCommand = new List<string>();
            int intNumTable = 0;
            using (var reader = objCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                while (reader.Read())
                {
                    lstCommand.Add($"FETCH ALL IN \"{ reader[0] }\";");
                }

            foreach (var itemCommand in lstCommand)
            {
                using (var cmd2 = new NpgsqlCommand())
                {
                    cmd2.Connection = objCommand.Connection;
                    cmd2.Transaction = objCommand.Transaction;
                    cmd2.CommandTimeout = objCommand.CommandTimeout;
                    cmd2.CommandText = itemCommand.ToString();
                    cmd2.CommandType = CommandType.Text;

                    using (var reader1 = cmd2.ExecuteReader())
                    {
                        dsResult.Tables.Add(new DataTable());
                        dsResult.Tables[intNumTable].Load(reader1);
                    }
                    //Execute cmd2 and process the results as normal
                    intNumTable++;
                }
            }

            return dsResult;
        }

        public void CreateNewBuckCopy(string strTableName, DataTable table)
        {
            throw new NotImplementedException();
        }
        public List<object> ExecStoreToListObject()
        {
            return new List<object>();
        }

        public List<object> ExecStoreToListObject(String strOutParameter)
        {
            return new List<object>();
        }

        private void ProcessException(Exception ex)
        {
            //Nếu sử dụng RO mà chưa compile được thì gọi compile Store
            if (this.objTransaction == null && ex.Message.ToString().Contains("16000") && ex.Message.ToString().Contains("ORA"))
            {
                IData objDataNew = Data.CreateData(_strConnect.Replace("RO", "RW"));
                try
                {
                    objDataNew.Connect();
                    objDataNew.ExecUpdate("ALTER PROCEDURE " + this.objCommand.CommandText + " COMPILE");

                }
                catch { }
                finally { objDataNew.Disconnect(); }
            }
        }


        #endregion

    }
}
