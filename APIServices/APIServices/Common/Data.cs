using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Common
{
    [Serializable]
    public class Data
    {
        #region Constants

        // Tên tham số Out mặc định trả về trong StoredProcedure (Oracle)
        protected const String DEFAULT_OUT_PARAMETER = "v_Out";
        // Kích thước vùng nhớ mặc định cho tham số Out trong StoredProcedure (Oracle)
        protected const int DEFAULT_OUT_PARAMETER_LENGTH = 4000;

        #endregion

        public static DATABASETYPE DataBaseType = DATABASETYPE.NONE;
        //protected static String strConnect = "";

        public Data()
        {
        }

        public enum DATABASETYPE
        {
            NONE = 0,
            SQLSERVER = 1,
            ORACLE = 2,
            MySQL = 3,
            MsAccess = 4,
            PosgreSQL = 5,
            SQLite = 6
        }

        /// <summary>
        /// Nhận dạng loại CSDL từ chuỗi kết nối
        /// </summary>
        /// <param name="strConnect"></param>
        /// <returns></returns>
        public static Data.DATABASETYPE RegconizeStringConnect(String strConnect)
        {
            String[] strOracle = { "Data Source", "User ID", "Password", "Unicode", "(Description", "LOAD_BALANCE", "ADDRESS_LIST", "SERVICE_NAME" };
            String[] strSQLSvr = { "Server", "DataBase", "UID", "Pwd", "Data Source", "User ID", "Password", "Initial Catalog" };
            String[] strMySQL = { "Server", "User ID", "Password", "DataBase" };
            String[] strMsAccess = { "Provider", "Microsoft", "Jet", "OLEDB", "Data Source" };
            String[] strPosgreSQL = { "Server", "Port", "User ID", "Password", "Database" };
            String[] strSQLite = { "Data Source", "Version", "Password" };

            //---------------------------------------------------------
            // Đếm sự có mặt của các từ khóa ORACLE trong chuỗi kết nối
            int intOraCount = 0;

            // Đếm tần suất xuất hiện của từ khóa Ora
            intOraCount += strConnect.ToUpper().Split(new String[] { "ORA" }, StringSplitOptions.None).Length;

            for (int i = 0; i < strOracle.Length; i++)
                if (strConnect.ToUpper().Contains(strOracle[i].ToUpper()))
                    intOraCount++;

            //---------------------------------------------------------
            // Đếm sự có mặt của các từ khóa SQL SERVER trong chuỗi kết nối
            int intSqlCount = 0;

            // Đếm tần suất xuất hiện của từ khóa Sql
            intSqlCount += strConnect.ToUpper().Split(new String[] { "SQL" }, StringSplitOptions.None).Length;

            for (int i = 0; i < strSQLSvr.Length; i++)
                if (strConnect.ToUpper().Contains(strSQLSvr[i].ToUpper()))
                    intSqlCount++;

            //---------------------------------------------------------
            // Đếm sự có mặt của các từ khóa MYSQL trong chuỗi kết nối
            int intMySqlCount = 0;

            for (int i = 0; i < strMySQL.Length; i++)
                if (strConnect.ToUpper().Contains(strMySQL[i].ToUpper()))
                    intMySqlCount++;

            //---------------------------------------------------------
            // Đếm sự có mặt của các từ khóa POSGRESQL trong chuỗi kết nối
            int intPosgreSQLCount = 0;

            for (int i = 0; i < strPosgreSQL.Length; i++)
                if (strConnect.ToUpper().Contains(strPosgreSQL[i].ToUpper()))
                    intPosgreSQLCount++;

            //---------------------------------------------------------
            // Đếm sự có mặt của các từ khóa MSACCESS trong chuỗi kết nối
            int intMsAccessCount = 0;

            for (int i = 0; i < strMsAccess.Length; i++)
                if (strConnect.ToUpper().Contains(strMsAccess[i].ToUpper()))
                    intMsAccessCount++;

            //---------------------------------------------------------
            // Đếm sự có mặt của các từ khóa SQLite trong chuỗi kết nối
            int intSQLite = 0;

            for (int i = 0; i < strSQLite.Length; i++)
                if (strConnect.ToUpper().Contains(strSQLite[i].ToUpper()))
                    intSQLite++;

            if (intSQLite == 3)
                return Data.DATABASETYPE.SQLite;

            if (intPosgreSQLCount >= 5)
                return Data.DATABASETYPE.PosgreSQL;

            if (intMySqlCount >= 4)
                return Data.DATABASETYPE.MySQL;

            if (intMsAccessCount >= 5)
                return Data.DATABASETYPE.MsAccess;

            // Trả về loại CSDL có nhiều từ khóa hơn
            return intOraCount >= intSqlCount ? Data.DATABASETYPE.ORACLE : Data.DATABASETYPE.SQLSERVER;
        }

        public static IData CreateData(String strConnect)
        {
            Data.DataBaseType = RegconizeStringConnect(strConnect);
            return new PosgreSQLData(strConnect);
        }
    }

}
